using System;
using System.Collections;
using System.Drawing;

namespace Cricut_Design_Studio
{
	public class Polygon
	{
		public ArrayList verts = new ArrayList();

		public int polyid;

		public bool hasIntersections;

		public bool hole;

		public static bool useOldWelder;

		public void draw(Graphics g, Pen pen)
		{
			if (!Clipper.enableDrawing)
			{
				return;
			}
			Vertex vertex = (Vertex)verts[0];
			do
			{
				float x = (float)vertex.prev.x / 1000f;
				float y = (float)vertex.prev.y / 1000f;
				float x2 = (float)vertex.x / 1000f;
				float y2 = (float)vertex.y / 1000f;
				g.DrawLine(pen, x, y, x2, y2);
				if (vertex.intersect)
				{
					Clipper.drawCircle(vertex.x, vertex.y, Color.Black);
				}
				vertex = vertex.next;
			}
			while (!vertex.Equals(verts[0]));
		}

		public bool isInside(Vertex v)
		{
			bool flag = false;
			for (int i = 0; i < verts.Count; i++)
			{
				Vertex vertex = (Vertex)verts[i];
				Vertex vertex2 = (Vertex)verts[(i + 1) % verts.Count];
				if (Math.Min(vertex.y, vertex2.y) < v.y && Math.Max(vertex.y, vertex2.y) >= v.y)
				{
					float num = (float)(vertex2.x - vertex.x) * (float)(v.y - vertex.y) / (float)(vertex2.y - vertex.y) + (float)vertex.x;
					if (num < (float)v.x)
					{
						flag = !flag;
					}
				}
			}
			return flag;
		}

		public bool isInside(Vertex v, Clipper clipper)
		{
			bool flag = false;
			foreach (Polygon polygon3 in clipper.inputGpoly.polygons)
			{
				if (!Equals(polygon3) && polygon3.isInside(v))
				{
					flag = !flag;
				}
			}
			foreach (Polygon polygon4 in clipper.outputGpoly.polygons)
			{
				if (!Equals(polygon4) && polygon4.isInside(v))
				{
					flag = !flag;
				}
			}
			return flag;
		}

		public void markIntersections(Clipper clipper)
		{
			Vertex vertex = (Vertex)verts[0];
			bool flag = !isInside(vertex, clipper);
			bool flag2 = (useOldWelder ? flag : (flag ^ hole));
			if (flag)
			{
				Clipper.drawBox(vertex.x, vertex.y, Color.Red);
			}
			else
			{
				Clipper.drawBox(vertex.x, vertex.y, Color.Blue);
			}
			do
			{
				if (vertex.intersect)
				{
					vertex.entry = flag2;
					flag2 = !flag2;
				}
				vertex = vertex.next;
			}
			while (!vertex.Equals(verts[0]));
		}

		public void adjustX(int dx)
		{
			foreach (Vertex vert in verts)
			{
				vert.x += dx;
			}
		}
	}
}
