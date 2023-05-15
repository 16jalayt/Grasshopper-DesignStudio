using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Cricut_Design_Studio
{
	public class UseAntiAlias : IDisposable
	{
		private Graphics _g;

		private SmoothingMode _old;

		public UseAntiAlias(Graphics g)
		{
			_g = g;
			_old = _g.SmoothingMode;
			_g.SmoothingMode = SmoothingMode.AntiAlias;
		}

		public void Dispose()
		{
			_g.SmoothingMode = _old;
		}
	}
}
