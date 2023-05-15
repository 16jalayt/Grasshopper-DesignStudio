using System.Collections;
using System.Drawing;

namespace Cricut_Design_Studio
{
	public class GeneralPolygon
	{
		public ArrayList polygons = new ArrayList();

		public int gpolyId;

		public void draw(Graphics g, Pen pen)
		{
			foreach (Polygon polygon in polygons)
			{
				polygon.draw(g, pen);
			}
		}

		public void markIntersections(Clipper clipper)
		{
			foreach (Polygon polygon in polygons)
			{
				polygon.markIntersections(clipper);
			}
		}
	}
}
