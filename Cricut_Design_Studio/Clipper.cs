using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Cricut_Design_Studio
{
	public class Clipper
	{
		private class KEdge
		{
			public int x1;

			public int y1;

			public int x2;

			public int y2;
		}

		private class KBucket
		{
			public ArrayList edges = new ArrayList();
		}

		public static bool enableDrawing;

		public static Graphics testG;

		private int nTotalVerts;

		public int nCoincidentIntersections;

		public int nBailedOut;

		private int polyId;

		private ArrayList tmpVertList;

		private ArrayList intersections = new ArrayList();

		private ArrayList kerningDistances = new ArrayList();

		public bool kerning;

		public GeneralPolygon inputGpoly;

		public GeneralPolygon outputGpoly = new GeneralPolygon();

		public static void drawBox(int x, int y, Color c)
		{
			if (enableDrawing)
			{
				Pen pen = new Pen(c);
				testG.DrawRectangle(pen, (float)x / 1000f - 5f, (float)y / 1000f - 5f, 10f, 10f);
				pen.Dispose();
			}
		}

		public static void drawCircle(int x, int y, Color c)
		{
			if (enableDrawing)
			{
				Pen pen = new Pen(c);
				RectangleF rect = new RectangleF((float)x / 1000f - 5f, (float)y / 1000f - 5f, 10f, 10f);
				testG.DrawArc(pen, rect, 0f, 360f);
				pen.Dispose();
			}
		}

		public static void drawVerts(ArrayList verts, Color c)
		{
			if (!enableDrawing)
			{
				return;
			}
			Pen pen = new Pen(c);
			Vertex vertex = null;
			Vertex vertex2 = null;
			foreach (Vertex vert in verts)
			{
				if (vertex == null)
				{
					vertex = vert;
				}
				else
				{
					float x = (float)vertex2.x / 1000f;
					float y = (float)vertex2.y / 1000f;
					float x2 = (float)vert.x / 1000f;
					float y2 = (float)vert.y / 1000f;
					testG.DrawLine(pen, x, y, x2, y2);
				}
				vertex2 = vert;
			}
			if (vertex != null && vertex2 != null)
			{
				float x3 = (float)vertex2.x / 1000f;
				float y3 = (float)vertex2.y / 1000f;
				float x4 = (float)vertex.x / 1000f;
				float y4 = (float)vertex.y / 1000f;
				testG.DrawLine(pen, x3, y3, x4, y4);
			}
			pen.Dispose();
		}

		public static float calcArea(ArrayList verts)
		{
			float num = 0f;
			for (int i = 0; i < verts.Count; i++)
			{
				Vertex vertex = (Vertex)verts[i];
				Vertex vertex2 = (Vertex)verts[(i + 1) % verts.Count];
				num += (float)vertex.x * (float)vertex2.y - (float)vertex.y * (float)vertex2.x;
			}
			return num;
		}

		public static void swap(ArrayList list, int i, int j)
		{
			object value = list[i];
			list[i] = list[j];
			list[j] = value;
		}

		public static void isort(ArrayList list, int left, int right)
		{
			if (left >= right)
			{
				return;
			}
			swap(list, left, (left + right) / 2);
			int num = left;
			for (int i = left + 1; i <= right; i++)
			{
				if ((int)list[i] < (int)list[left])
				{
					swap(list, ++num, i);
				}
			}
			swap(list, left, num);
			isort(list, left, num - 1);
			isort(list, num + 1, right);
		}

		public static void updateVertLinks(ArrayList verts)
		{
			for (int i = 0; i < verts.Count; i++)
			{
				int index = (i + 1) % verts.Count;
				Vertex vertex = (Vertex)verts[i];
				(vertex.next = (Vertex)verts[index]).prev = vertex;
			}
		}

		public static void updateVertList(ArrayList verts, Vertex firstv)
		{
			Vertex vertex = firstv;
			verts.Clear();
			do
			{
				verts.Add(vertex);
				vertex = vertex.next;
			}
			while (!vertex.Equals(firstv));
		}

		public void fixWinding(GeneralPolygon gp)
		{
			foreach (Polygon polygon3 in gp.polygons)
			{
				if (polygon3.verts.Count < 1)
				{
					continue;
				}
				bool flag = false;
				Vertex v = (Vertex)polygon3.verts[0];
				foreach (Polygon polygon4 in gp.polygons)
				{
					if (!polygon3.Equals(polygon4) && polygon4.isInside(v))
					{
						flag = true;
						break;
					}
				}
				float num = calcArea(polygon3.verts);
				if (flag)
				{
					polygon3.hole = true;
					if (num < 0f)
					{
						int count = polygon3.verts.Count;
						for (int i = 0; i < count / 2; i++)
						{
							object value = polygon3.verts[i];
							polygon3.verts[i] = polygon3.verts[count - 1 - i];
							polygon3.verts[count - 1 - i] = value;
						}
					}
				}
				else
				{
					polygon3.hole = false;
					if (num > 0f)
					{
						int count2 = polygon3.verts.Count;
						for (int j = 0; j < count2 / 2; j++)
						{
							object value2 = polygon3.verts[j];
							polygon3.verts[j] = polygon3.verts[count2 - 1 - j];
							polygon3.verts[count2 - 1 - j] = value2;
						}
					}
				}
				if (polygon3.hole)
				{
					drawVerts(polygon3.verts, Color.Blue);
				}
				else
				{
					drawVerts(polygon3.verts, Color.Red);
				}
			}
		}

		public void polyBgn()
		{
			inputGpoly = new GeneralPolygon();
		}

		public void polyEnd()
		{
			fixWinding(inputGpoly);
			foreach (Polygon polygon4 in inputGpoly.polygons)
			{
				updateVertLinks(polygon4.verts);
			}
			if (kerning)
			{
				if (outputGpoly.polygons.Count == 0)
				{
					outputGpoly = inputGpoly;
					return;
				}
				float num = findZeroDist(outputGpoly, inputGpoly);
				kerningDistances.Add(num);
				outputGpoly = inputGpoly;
				return;
			}
			if (outputGpoly.polygons.Count == 0)
			{
				outputGpoly = inputGpoly;
				return;
			}
			findGPolyIntersections(outputGpoly, inputGpoly);
			if (intersections.Count == 0)
			{
				foreach (Polygon polygon5 in inputGpoly.polygons)
				{
					outputGpoly.polygons.Add(polygon5);
				}
				return;
			}
			markIntersections();
			GeneralPolygon generalPolygon = createOutputPolygons();
			if (generalPolygon == null)
			{
				nBailedOut++;
				return;
			}
			GeneralPolygon generalPolygon2 = new GeneralPolygon();
			foreach (Polygon polygon6 in generalPolygon.polygons)
			{
				generalPolygon2.polygons.Add(polygon6);
			}
			foreach (Polygon polygon7 in outputGpoly.polygons)
			{
				if (!polygon7.hasIntersections)
				{
					generalPolygon2.polygons.Add(polygon7);
				}
			}
			foreach (Polygon polygon8 in inputGpoly.polygons)
			{
				if (!polygon8.hasIntersections)
				{
					generalPolygon2.polygons.Add(polygon8);
				}
			}
			outputGpoly = generalPolygon2;
		}

		public void addFirstVert(float x, float y)
		{
			tmpVertList = new ArrayList();
			int x2 = (int)(x * 10f + 0.5f);
			int y2 = (int)(y * 10f + 0.5f);
			Vertex vertex = new Vertex();
			vertex.x = x2;
			vertex.y = y2;
			tmpVertList.Add(vertex);
			nTotalVerts++;
		}

		public void addNextVert(float x, float y)
		{
			int x2 = (int)(x * 10f + 0.5f);
			int y2 = (int)(y * 10f + 0.5f);
			Vertex vertex = new Vertex();
			vertex.x = x2;
			vertex.y = y2;
			tmpVertList.Add(vertex);
			nTotalVerts++;
		}

		public void endContour()
		{
			if (tmpVertList == null)
			{
				return;
			}
			if (tmpVertList.Count < 2)
			{
				tmpVertList.Clear();
				tmpVertList = null;
				return;
			}
			bool flag = false;
			int num = 0;
			do
			{
				flag = false;
				num = tmpVertList.Count;
				for (int i = 0; i < tmpVertList.Count; i++)
				{
					Vertex vertex = (Vertex)tmpVertList[(i + num - 1) % num];
					Vertex vertex2 = (Vertex)tmpVertList[i];
					Vertex vertex3 = (Vertex)tmpVertList[(i + 1) % num];
					if ((vertex2.x == vertex.x && vertex2.y == vertex.y) || (vertex2.x == vertex3.x && vertex2.y == vertex3.y))
					{
						tmpVertList.RemoveAt(i);
						flag = true;
						break;
					}
					if (vertex2.y == vertex.y && vertex2.y == vertex3.y)
					{
						tmpVertList.RemoveAt(i);
						flag = true;
						break;
					}
				}
			}
			while (flag);
			foreach (Vertex tmpVert in tmpVertList)
			{
				tmpVert.x *= 100;
				tmpVert.y *= 100;
			}
			Polygon polygon = new Polygon();
			polygon.polyid = polyId++;
			polygon.verts = tmpVertList;
			inputGpoly.polygons.Add(polygon);
		}

		public void discardEdges()
		{
			if (tmpVertList != null)
			{
				tmpVertList.Clear();
				tmpVertList = null;
			}
		}

		private float perpDist(PointF p, PointF v1, PointF v2)
		{
			float num = v2.X - v1.X;
			float num2 = v2.Y - v1.Y;
			float num3 = num * num + num2 * num2;
			if (num3 < float.Epsilon)
			{
				num = p.X - v1.X;
				num2 = p.Y - v1.Y;
				return (float)Math.Sqrt(num * num + num2 * num2);
			}
			num = v1.X - p.X;
			num2 = v1.Y - p.Y;
			float num4 = num * num + num2 * num2;
			num = v2.X - p.X;
			num2 = v2.Y - p.Y;
			float num5 = num * num + num2 * num2;
			if (num3 + num4 <= num5 || num3 + num5 <= num4)
			{
				return Math.Min(num4, num5);
			}
			float num6 = v1.Y - v2.Y;
			float num7 = v2.X - v1.X;
			float num8 = num6 * v1.X + num7 * v1.Y;
			num3 = num6 * p.X + num7 * p.Y - num8;
			return (float)Math.Sqrt(num3 * num3 / (num6 * num6 + num7 * num7));
		}

		private bool overlap(Rectangle r, Rectangle s)
		{
			if (r.Left <= s.Right && s.Left <= r.Right && r.Top <= s.Bottom)
			{
				return s.Top <= r.Bottom;
			}
			return false;
		}

		public bool lineIntersection(PointF p1, PointF p2, PointF p3, PointF p4, ref PointF result, out int degeneracyCode)
		{
			degeneracyCode = 0;
			if (perpDist(p1, p3, p4) < float.Epsilon || perpDist(p2, p3, p4) < float.Epsilon || perpDist(p3, p1, p2) < float.Epsilon || perpDist(p4, p1, p2) < float.Epsilon)
			{
				degeneracyCode = 1;
				return false;
			}
			float num = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);
			float num2 = (p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X);
			float num3 = (p2.X - p1.X) * (p1.Y - p3.Y) - (p2.Y - p1.Y) * (p1.X - p3.X);
			if (Math.Abs(num) <= float.Epsilon)
			{
				if (Math.Abs(num2) <= float.Epsilon && Math.Abs(num3) <= float.Epsilon)
				{
					degeneracyCode = 2;
					return false;
				}
				degeneracyCode = 3;
				return false;
			}
			float num4 = num2 / num;
			float num5 = num3 / num;
			if (num4 >= 0f && num4 < 1f && num5 >= 0f && num5 < 1f)
			{
				result.X = p1.X + num4 * (p2.X - p1.X);
				result.Y = p1.Y + num4 * (p2.Y - p1.Y);
				if ((Math.Abs(result.X - p1.X) < float.Epsilon && Math.Abs(result.Y - p1.Y) < float.Epsilon) || (Math.Abs(result.X - p2.X) < float.Epsilon && Math.Abs(result.Y - p2.Y) < float.Epsilon) || (Math.Abs(result.X - p3.X) < float.Epsilon && Math.Abs(result.Y - p3.Y) < float.Epsilon) || (Math.Abs(result.X - p4.X) < float.Epsilon && Math.Abs(result.Y - p4.Y) < float.Epsilon))
				{
					degeneracyCode = 4;
					return false;
				}
				return true;
			}
			return false;
		}

		private float getLerpDist(PointF e1, PointF e2, PointF p)
		{
			float num = e2.X - e1.X;
			float num2 = e2.Y - e1.Y;
			float num3 = (float)Math.Sqrt(num * num + num2 * num2);
			num = p.X - e1.X;
			num2 = p.Y - e1.Y;
			float num4 = (float)Math.Sqrt(num * num + num2 * num2);
			if (num4 < float.Epsilon)
			{
				return 0f;
			}
			return num4 / num3;
		}

		private void findPolyIntersections(Polygon p1, Polygon p2)
		{
			ArrayList arrayList = new ArrayList();
			int num = 0;
			int num2 = 0;
			Random random = new Random(0);
			do
			{
				num = 0;
				for (int i = 0; i < p1.verts.Count; i++)
				{
					Vertex vertex = (Vertex)p1.verts[i];
					for (int j = 0; j < p2.verts.Count; j++)
					{
						Vertex vertex2 = (Vertex)p2.verts[j];
						if (vertex.x == vertex2.x && vertex.y == vertex2.y)
						{
							vertex.x = (int)Math.Round((double)vertex.x + random.NextDouble() * 3.0);
							vertex.y = (int)Math.Round((double)vertex.y + random.NextDouble() * 3.0);
							vertex2.x = (int)Math.Round((double)vertex2.x + random.NextDouble() * 3.0);
							vertex2.y = (int)Math.Round((double)vertex2.y + random.NextDouble() * 3.0);
							num++;
						}
					}
				}
				num2++;
			}
			while (num > 0);
			num2 = 0;
			do
			{
				arrayList.Clear();
				num = 0;
				for (int k = 0; k < p1.verts.Count; k++)
				{
					int index = (k + 1) % p1.verts.Count;
					Vertex vertex3 = (Vertex)p1.verts[k];
					Vertex vertex4 = (Vertex)p1.verts[index];
					Rectangle r = new Rectangle(Math.Min(vertex3.x, vertex4.x), Math.Min(vertex3.y, vertex4.y), Math.Abs(vertex3.x - vertex4.x), Math.Abs(vertex3.y - vertex4.y));
					for (int l = 0; l < p2.verts.Count; l++)
					{
						int index2 = (l + 1) % p2.verts.Count;
						Vertex vertex5 = (Vertex)p2.verts[l];
						Vertex vertex6 = (Vertex)p2.verts[index2];
						Rectangle s = new Rectangle(Math.Min(vertex5.x, vertex6.x), Math.Min(vertex5.y, vertex6.y), Math.Abs(vertex5.x - vertex6.x), Math.Abs(vertex5.y - vertex6.y));
						if (overlap(r, s))
						{
							PointF pointF = new PointF(vertex3.x, vertex3.y);
							PointF pointF2 = new PointF(vertex4.x, vertex4.y);
							PointF pointF3 = new PointF(vertex5.x, vertex5.y);
							PointF pointF4 = new PointF(vertex6.x, vertex6.y);
							PointF result = new PointF(0f, 0f);
							if (lineIntersection(pointF, pointF2, pointF3, pointF4, ref result, out var degeneracyCode))
							{
								Vertex vertex7 = new Vertex();
								vertex7.intersect = true;
								vertex7.x = (int)Math.Round(result.X);
								vertex7.y = (int)Math.Round(result.Y);
								vertex7.prev = vertex3;
								vertex7.next = vertex4;
								vertex7.alpha = getLerpDist(pointF, pointF2, result);
								vertex7.holes = (p1.hole ? 1 : 0) + (p2.hole ? 1 : 0);
								Vertex vertex8 = new Vertex();
								vertex8.intersect = true;
								vertex8.x = (int)Math.Round(result.X);
								vertex8.y = (int)Math.Round(result.Y);
								vertex8.prev = vertex5;
								vertex8.next = vertex6;
								vertex8.alpha = getLerpDist(pointF3, pointF4, result);
								vertex8.holes = (p1.hole ? 1 : 0) + (p2.hole ? 1 : 0);
								vertex7.neighbor = vertex8;
								vertex8.neighbor = vertex7;
								arrayList.Add(vertex7);
								arrayList.Add(vertex8);
								p1.hasIntersections = true;
								p2.hasIntersections = true;
							}
							else if (degeneracyCode != 0)
							{
								vertex3.x = (int)Math.Round((double)vertex3.x + random.NextDouble() * 3.0);
								vertex3.y = (int)Math.Round((double)vertex3.y + random.NextDouble() * 3.0);
								vertex4.x = (int)Math.Round((double)vertex4.x + random.NextDouble() * 3.0);
								vertex4.y = (int)Math.Round((double)vertex4.y + random.NextDouble() * 3.0);
								vertex5.x = (int)Math.Round((double)vertex5.x + random.NextDouble() * 3.0);
								vertex5.y = (int)Math.Round((double)vertex5.y + random.NextDouble() * 3.0);
								vertex6.x = (int)Math.Round((double)vertex6.x + random.NextDouble() * 3.0);
								vertex6.y = (int)Math.Round((double)vertex6.y + random.NextDouble() * 3.0);
								num++;
							}
						}
					}
				}
				num2++;
			}
			while (num > 0);
			foreach (Vertex item in arrayList)
			{
				Vertex prev = item.prev;
				Vertex next = item.next;
				if (next.Equals(prev.next))
				{
					prev.next = item;
					next.prev = item;
				}
				else
				{
					Vertex next2 = prev.next;
					while (!next2.next.Equals(next))
					{
						if (!next2.intersect)
						{
							Console.WriteLine("problem 1");
						}
						if (item.alpha < next2.alpha)
						{
							break;
						}
						next2 = next2.next;
					}
					if (item.alpha < next2.alpha)
					{
						item.prev = next2.prev;
						item.next = next2;
						next2.prev.next = item;
						next2.prev = item;
					}
					else
					{
						item.next = next2.next;
						item.prev = next2;
						next2.next.prev = item;
						next2.next = item;
					}
				}
				intersections.Add(item);
			}
		}

		private void findGPolyIntersections(GeneralPolygon g1, GeneralPolygon g2)
		{
			for (int i = 0; i < g1.polygons.Count; i++)
			{
				Polygon p = (Polygon)g1.polygons[i];
				for (int j = 0; j < g2.polygons.Count; j++)
				{
					Polygon p2 = (Polygon)g2.polygons[j];
					findPolyIntersections(p, p2);
				}
			}
		}

		private void markIntersections()
		{
			outputGpoly.markIntersections(this);
			inputGpoly.markIntersections(this);
		}

		private GeneralPolygon createOutputPolygons()
		{
			GeneralPolygon generalPolygon = new GeneralPolygon();
			foreach (Vertex intersection in intersections)
			{
				foreach (Vertex intersection2 in intersections)
				{
					if (!intersection.Equals(intersection2) && intersection.x == intersection2.x && intersection.y == intersection2.y && intersection.neighbor != intersection2)
					{
						nCoincidentIntersections++;
					}
				}
			}
			while (intersections.Count > 0)
			{
				Vertex vertex3 = null;
				foreach (Vertex intersection3 in intersections)
				{
					if (intersection3.holes == 0)
					{
						vertex3 = intersection3;
						break;
					}
				}
				if (vertex3 == null)
				{
					foreach (Vertex intersection4 in intersections)
					{
						if (1 == intersection4.holes)
						{
							vertex3 = intersection4;
							break;
						}
					}
				}
				if (vertex3 == null)
				{
					foreach (Vertex intersection5 in intersections)
					{
						if (2 == intersection5.holes)
						{
							vertex3 = intersection5;
							break;
						}
					}
				}
				intersections.Remove(vertex3);
				if (vertex3.visited)
				{
					continue;
				}
				Vertex vertex7 = vertex3;
				Polygon polygon = new Polygon();
				int num = 0;
				do
				{
					polygon.verts.Add(new Vertex(vertex3.x, vertex3.y));
					num++;
					vertex3.visited = true;
					if (vertex3.entry)
					{
						do
						{
							vertex3 = vertex3.prev;
							polygon.verts.Add(new Vertex(vertex3.x, vertex3.y));
							num++;
							vertex3.visited = true;
						}
						while (!vertex3.intersect);
					}
					else
					{
						do
						{
							vertex3 = vertex3.next;
							polygon.verts.Add(new Vertex(vertex3.x, vertex3.y));
							num++;
							vertex3.visited = true;
						}
						while (!vertex3.intersect);
					}
					vertex3 = vertex3.neighbor;
					if (num > nTotalVerts * 4)
					{
						return null;
					}
				}
				while (vertex3.x != vertex7.x || vertex3.y != vertex7.y);
				if (polygon.verts.Count > 0)
				{
					generalPolygon.polygons.Add(polygon);
				}
			}
			fixWinding(generalPolygon);
			foreach (Polygon polygon2 in generalPolygon.polygons)
			{
				updateVertLinks(polygon2.verts);
			}
			return generalPolygon;
		}

		public void draw()
		{
			Pen pen = new Pen(Color.Black, 2f);
			testG.SmoothingMode = SmoothingMode.AntiAlias;
			enableDrawing = true;
			outputGpoly.draw(testG, pen);
			enableDrawing = false;
			pen.Dispose();
		}

		private int rv(float f)
		{
			return (int)Math.Round(f);
		}

		public void cutVertList(PcControl pc, ArrayList verts)
		{
			Vertex vertex = null;
			Vertex vertex2 = null;
			pc.drawBgn();
			foreach (Vertex vert in verts)
			{
				if (vertex == null)
				{
					pc.moveTo(rv((float)vert.x / 1000f), rv((float)vert.y / 1000f));
					vertex = vert;
				}
				else if (vertex2 == null)
				{
					pc.drawTick(0, rv((float)vert.x / 1000f), rv((float)vert.y / 1000f));
					vertex2 = vert;
				}
				else
				{
					pc.drawTo(rv((float)vert.x / 1000f), rv((float)vert.y / 1000f));
				}
			}
			if (vertex != null)
			{
				pc.drawTo(rv((float)vertex.x / 1000f), rv((float)vertex.y / 1000f));
				if (vertex2 != null)
				{
					pc.drawTo(rv((float)vertex2.x / 1000f), rv((float)vertex2.y / 1000f));
				}
			}
			pc.drawEnd();
		}

		public void cut(PcControl pc, out int nerrors)
		{
			foreach (Polygon polygon in outputGpoly.polygons)
			{
				updateVertList(polygon.verts, (Vertex)polygon.verts[0]);
				for (int i = 0; i < Form1.myRootForm.multiCut + 1; i++)
				{
					cutVertList(pc, polygon.verts);
				}
			}
			nerrors = nBailedOut;
		}

		public void addEdges(ArrayList elist, Polygon p, ref int minY, ref int maxY)
		{
			for (int i = 0; i < p.verts.Count; i++)
			{
				Vertex vertex = (Vertex)p.verts[i];
				Vertex vertex2 = (Vertex)p.verts[(i + 1) % p.verts.Count];
				KEdge kEdge = new KEdge();
				if (vertex.y > vertex2.y)
				{
					kEdge.x1 = vertex2.x / 10;
					kEdge.y1 = vertex2.y / 10;
					kEdge.x2 = vertex.x / 10;
					kEdge.y2 = vertex.y / 10;
				}
				else
				{
					kEdge.x1 = vertex.x / 10;
					kEdge.y1 = vertex.y / 10;
					kEdge.x2 = vertex2.x / 10;
					kEdge.y2 = vertex2.y / 10;
				}
				if (kEdge.y1 < minY)
				{
					minY = kEdge.y1;
				}
				if (kEdge.y1 > maxY)
				{
					maxY = kEdge.y1;
				}
				elist.Add(kEdge);
			}
		}

		public int processSpans(GeneralPolygon gp1, GeneralPolygon gp2)
		{
			ArrayList arrayList = new ArrayList();
			ArrayList arrayList2 = new ArrayList();
			int minY = int.MaxValue;
			int maxY = int.MinValue;
			int num = 0;
			int num2 = 0;
			foreach (Polygon polygon3 in gp1.polygons)
			{
				if (!polygon3.hole)
				{
					addEdges(arrayList, polygon3, ref minY, ref maxY);
					num++;
				}
			}
			foreach (Polygon polygon4 in gp2.polygons)
			{
				if (!polygon4.hole)
				{
					addEdges(arrayList2, polygon4, ref minY, ref maxY);
					num2++;
				}
			}
			KBucket[] array = new KBucket[maxY - minY + 1];
			KBucket[] array2 = new KBucket[maxY - minY + 1];
			foreach (KEdge item in arrayList)
			{
				int num3 = item.y1 - minY;
				if (array[num3] == null)
				{
					array[num3] = new KBucket();
				}
				array[num3].edges.Add(item);
			}
			foreach (KEdge item2 in arrayList2)
			{
				int num4 = item2.y1 - minY;
				if (array2[num4] == null)
				{
					array2[num4] = new KBucket();
				}
				array2[num4].edges.Add(item2);
			}
			ArrayList arrayList3 = new ArrayList();
			int[] array3 = new int[maxY - minY + 1];
			int[] array4 = new int[maxY - minY + 1];
			for (int i = 0; i < maxY - minY + 1; i++)
			{
				int num5 = i + minY;
				for (int num6 = arrayList3.Count - 1; num6 >= 0; num6--)
				{
					KEdge kEdge3 = (KEdge)arrayList3[num6];
					if (num5 > kEdge3.y2)
					{
						arrayList3.RemoveAt(num6);
					}
				}
				if (array[i] != null)
				{
					foreach (KEdge edge in array[i].edges)
					{
						arrayList3.Add(edge);
					}
				}
				int num7 = int.MinValue;
				foreach (KEdge item3 in arrayList3)
				{
					if (item3.y1 == item3.y2)
					{
						int num8 = ((item3.x1 > item3.x2) ? item3.x1 : item3.x2);
						if (num8 > num7)
						{
							num7 = num8;
						}
						continue;
					}
					float num9 = (float)(num5 - item3.y1) / (float)(item3.y2 - item3.y1);
					int num10 = (int)Math.Round(num9 * (float)(item3.x2 - item3.x1) + (float)item3.x1);
					if (num10 > num7)
					{
						num7 = num10;
					}
				}
				array3[i] = num7;
			}
			arrayList3.Clear();
			for (int j = 0; j < maxY - minY + 1; j++)
			{
				int num11 = j + minY;
				for (int num12 = arrayList3.Count - 1; num12 >= 0; num12--)
				{
					KEdge kEdge5 = (KEdge)arrayList3[num12];
					if (num11 > kEdge5.y2)
					{
						arrayList3.RemoveAt(num12);
					}
				}
				if (array2[j] != null)
				{
					foreach (KEdge edge2 in array2[j].edges)
					{
						arrayList3.Add(edge2);
					}
				}
				int num13 = int.MaxValue;
				foreach (KEdge item4 in arrayList3)
				{
					if (item4.y1 == item4.y2)
					{
						int num14 = ((item4.x1 < item4.x2) ? item4.x1 : item4.x2);
						if (num14 < num13)
						{
							num13 = num14;
						}
						continue;
					}
					float num15 = (float)(num11 - item4.y1) / (float)(item4.y2 - item4.y1);
					int num16 = (int)Math.Round(num15 * (float)(item4.x2 - item4.x1) + (float)item4.x1);
					if (num16 < num13)
					{
						num13 = num16;
					}
				}
				array4[j] = num13;
			}
			int num17 = int.MaxValue;
			for (int k = 0; k < maxY - minY + 1; k++)
			{
				if (int.MinValue != array3[k] && int.MaxValue != array3[k] && int.MinValue != array4[k] && int.MinValue != array4[k])
				{
					int num18 = array4[k] - array3[k];
					if (num18 < num17)
					{
						num17 = num18;
					}
				}
			}
			if (int.MaxValue == num17)
			{
				return 0;
			}
			return num17;
		}

		public float findZeroDist(GeneralPolygon gpleft, GeneralPolygon gpright)
		{
			int num = processSpans(gpleft, gpright);
			return num;
		}

		public float[] kern(float dist)
		{
			if (kerningDistances.Count < 1)
			{
				return null;
			}
			float[] array = new float[kerningDistances.Count];
			for (int i = 0; i < kerningDistances.Count; i++)
			{
				array[i] = (float)kerningDistances[i] / 100f;
			}
			return array;
		}
	}
}
