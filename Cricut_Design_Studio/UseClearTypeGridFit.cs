using System;
using System.Drawing;
using System.Drawing.Text;

namespace Cricut_Design_Studio
{
	public class UseClearTypeGridFit : IDisposable
	{
		private Graphics _g;

		private TextRenderingHint _old;

		public UseClearTypeGridFit(Graphics g)
		{
			_g = g;
			_old = _g.TextRenderingHint;
			_g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
		}

		public void Dispose()
		{
			_g.TextRenderingHint = _old;
		}
	}
}
