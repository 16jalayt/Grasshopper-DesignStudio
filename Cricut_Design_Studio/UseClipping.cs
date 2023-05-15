using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Cricut_Design_Studio
{
	public class UseClipping : IDisposable
	{
		private Graphics _g;

		private Region _old;

		public UseClipping(Graphics g, GraphicsPath path)
		{
			_g = g;
			_old = g.Clip;
			Region region = _old.Clone();
			region.Intersect(path);
			_g.Clip = region;
		}

		public UseClipping(Graphics g, Region region)
		{
			_g = g;
			_old = g.Clip;
			Region region2 = _old.Clone();
			region2.Intersect(region);
			_g.Clip = region2;
		}

		public void Dispose()
		{
			_g.Clip = _old;
		}
	}
}
