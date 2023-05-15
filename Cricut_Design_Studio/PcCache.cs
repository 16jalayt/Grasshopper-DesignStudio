using System;
using System.Collections;
using System.Drawing;
using System.Threading;

namespace Cricut_Design_Studio
{
	public class PcCache : PcControl
	{
		private class CacheCmd
		{
			public ushort cmd;

			public int x0;

			public int y0;

			public int x1;

			public int y1;

			public int x2;

			public int y2;

			public int x3;

			public int y3;

			public int tickCode = -1;

			public CacheCmd(ushort cmdCode)
			{
				cmd = cmdCode;
			}

			public CacheCmd(ushort cmdCode, int x, int y)
			{
				cmd = cmdCode;
				x0 = x;
				y0 = y;
			}

			public CacheCmd(ushort cmdCode, int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
			{
				cmd = cmdCode;
				this.x0 = x0;
				this.y0 = y0;
				this.x1 = x1;
				this.y1 = y1;
				this.x2 = x2;
				this.y2 = y2;
				this.x3 = x3;
				this.y3 = y3;
			}
		}

		private const byte RGTBDRY = 1;

		private const byte TOPBDRY = 2;

		private const byte LFTBDRY = 4;

		private const byte BOTBDRY = 8;

		private ArrayList cache = new ArrayList();

		public static bool trialMode = true;

		public static int cricutModel = 0;

		private bool penIsDown;

		public override bool bgn()
		{
			cache.Clear();
			return base.bgn();
		}

		public void errorEnd()
		{
			cache.Clear();
		}

		private int rv(float f)
		{
			return (int)Math.Round(f);
		}

		public virtual void subdivCurve(int tick, int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
		{
			PointF p = new PointF(x0, y0);
			PointF pointF = new PointF(x1, y1);
			PointF pointF2 = new PointF(x2, y2);
			PointF p2 = new PointF(x3, y3);
			PointF p3 = PointF_mid(p, pointF);
			PointF pointF3 = PointF_mid(pointF, pointF2);
			PointF p4 = PointF_mid(pointF2, p2);
			PointF p5 = PointF_mid(p3, pointF3);
			PointF p6 = PointF_mid(pointF3, p4);
			PointF pointF4 = PointF_mid(p5, p6);
			switch (tick)
			{
			case -1:
				clipCurve(-1, rv(p.X), rv(p.Y), rv(p3.X), rv(p3.Y), rv(p5.X), rv(p5.Y), rv(pointF4.X), rv(pointF4.Y));
				clipCurve(-1, rv(pointF4.X), rv(pointF4.Y), rv(p6.X), rv(p6.Y), rv(p4.X), rv(p4.Y), rv(p2.X), rv(p2.Y));
				break;
			case 0:
				clipCurve(0, rv(p.X), rv(p.Y), rv(p3.X), rv(p3.Y), rv(p5.X), rv(p5.Y), rv(pointF4.X), rv(pointF4.Y));
				clipCurve(-1, rv(pointF4.X), rv(pointF4.Y), rv(p6.X), rv(p6.Y), rv(p4.X), rv(p4.Y), rv(p2.X), rv(p2.Y));
				break;
			case 1:
				clipCurve(-1, rv(p.X), rv(p.Y), rv(p3.X), rv(p3.Y), rv(p5.X), rv(p5.Y), rv(pointF4.X), rv(pointF4.Y));
				clipCurve(1, rv(pointF4.X), rv(pointF4.Y), rv(p6.X), rv(p6.Y), rv(p4.X), rv(p4.Y), rv(p2.X), rv(p2.Y));
				break;
			}
		}

		public override bool end()
		{
			int x = 202;
			int y = 252;
			int x2 = 202;
			int y2 = 252;
			int num = 0;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = true;
			int x3 = 0;
			int y3 = 0;
			for (int i = 0; i < cache.Count - 1; i++)
			{
				int index = i + 1;
				CacheCmd cacheCmd = (CacheCmd)cache[i];
				CacheCmd cacheCmd2 = (CacheCmd)cache[index];
				if (cacheCmd.tickCode == 0 && 19796 == cacheCmd2.cmd)
				{
					cacheCmd.tickCode = -1;
				}
			}
			foreach (CacheCmd item in cache)
			{
				switch (item.cmd)
				{
				case 1:
					base.drawBgn();
					flag = true;
					break;
				case 2:
					Thread.Sleep(100);
					base.drawEnd();
					flag = false;
					break;
				case 19796:
					penIsDown = false;
					x3 = item.x0;
					y3 = item.y0;
					x = item.x0;
					y = item.y0;
					flag3 = false;
					break;
				case 17492:
					clipLine(-1, x3, y3, item.x0, item.y0);
					x3 = item.x0;
					y3 = item.y0;
					x2 = item.x0;
					y2 = item.y0;
					flag3 = false;
					break;
				case 17524:
					clipLine(item.tickCode, x3, y3, item.x0, item.y0);
					x3 = item.x0;
					y3 = item.y0;
					x2 = item.x0;
					y2 = item.y0;
					flag3 = false;
					break;
				case 17236:
					subdivCurve(-1, item.x0, item.y0, item.x1, item.y1, item.x2, item.y2, item.x3, item.y3);
					x3 = item.x3;
					y3 = item.y3;
					x2 = item.x0;
					y2 = item.y0;
					flag3 = false;
					break;
				case 17268:
					subdivCurve(item.tickCode, item.x0, item.y0, item.x1, item.y1, item.x2, item.y2, item.x3, item.y3);
					x3 = item.x3;
					y3 = item.y3;
					x2 = item.x0;
					y2 = item.y0;
					flag3 = false;
					break;
				}
				if (Form1.myRootForm.cuttingBackgroundWorker.CancellationPending || cricutStatus != 0)
				{
					flag2 = true;
					break;
				}
				int percentProgress = (int)Math.Round((float)num++ / (float)cache.Count * 100f);
				Form1.myRootForm.cuttingBackgroundWorker.ReportProgress(percentProgress);
			}
			if (flag)
			{
				if (!flag3)
				{
					base.moveTo(x2, y2);
				}
				base.drawEnd();
			}
			else if (!flag2 && !flag3)
			{
				base.drawBgn();
				base.moveTo(x, y);
				base.drawEnd();
			}
			base.drawEnd();
			cache.Clear();
			if (Form1.myRootForm.cuttingBackgroundWorker.IsBusy)
			{
				try
				{
					Form1.myRootForm.cuttingBackgroundWorker.ReportProgress(100);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
			return base.end();
		}

		public override void drawBgn()
		{
			cache.Add(new CacheCmd(1));
		}

		public override void drawEnd()
		{
			cache.Add(new CacheCmd(2));
		}

		public override void moveTo(int x, int y)
		{
			cache.Add(new CacheCmd(19796, x, y));
		}

		public override void drawTo(int x, int y)
		{
			cache.Add(new CacheCmd(17492, x, y));
		}

		public override void curveTo(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
		{
			cache.Add(new CacheCmd(17236, x0, y0, x1, y1, x2, y2, x3, y3));
		}

		public override void drawTick(int tickCode, int x, int y)
		{
			CacheCmd cacheCmd = new CacheCmd(17524, x, y);
			cacheCmd.tickCode = tickCode;
			cache.Add(cacheCmd);
		}

		public override void curveTick(int tickCode, int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
		{
			CacheCmd cacheCmd = new CacheCmd(17268, x0, y0, x1, y1, x2, y2, x3, y3);
			cacheCmd.tickCode = tickCode;
			cache.Add(cacheCmd);
		}

		private void clipLine(int tickCode, int x0, int y0, int x1, int y1)
		{
			if (regionCode(x0, y0) != 0)
			{
				penIsDown = false;
			}
			if (cs_clip(ref x0, ref y0, ref x1, ref y1))
			{
				if (!penIsDown)
				{
					base.moveTo(x0, y0);
				}
				if (-1 == tickCode)
				{
					base.drawTo(x1, y1);
				}
				else
				{
					base.drawTick(tickCode, x1, y1);
				}
				penIsDown = true;
			}
			else
			{
				penIsDown = false;
			}
		}

		private PointF PointF_mid(PointF p1, PointF p2)
		{
			return new PointF((p1.X + p2.X) / 2f, (p1.Y + p2.Y) / 2f);
		}

		private float PointF_dist(PointF p1, PointF p2)
		{
			float num = p2.X - p1.X;
			float num2 = p2.Y - p1.Y;
			float num3 = num * num + num2 * num2;
			if ((double)num3 < 1E-05)
			{
				return 0f;
			}
			return (float)Math.Sqrt(num3);
		}

		private void subdivCurve(PointF p0, PointF p1, PointF p2, PointF p3)
		{
			if (regionCode(p0.X, p0.Y) == 0 && regionCode(p1.X, p1.Y) == 0 && regionCode(p2.X, p2.Y) == 0 && regionCode(p3.X, p3.Y) == 0)
			{
				if (!penIsDown)
				{
					base.moveTo((int)Math.Round(p0.X), (int)Math.Round(p0.Y));
				}
				Math.Round(p1.X);
				Math.Round(p1.Y);
				Math.Round(p2.X);
				Math.Round(p2.Y);
				base.curveTo((int)Math.Round(p0.X), (int)Math.Round(p0.Y), (int)Math.Round(p1.X), (int)Math.Round(p1.Y), (int)Math.Round(p2.X), (int)Math.Round(p2.Y), (int)Math.Round(p3.X), (int)Math.Round(p3.Y));
				penIsDown = true;
				return;
			}
			float num = PointF_dist(p0, p1) + PointF_dist(p1, p2) + PointF_dist(p2, p3);
			if (num < 100f)
			{
				clipLine(-1, (int)Math.Round(p0.X), (int)Math.Round(p0.Y), (int)Math.Round(p1.X), (int)Math.Round(p1.Y));
				clipLine(-1, (int)Math.Round(p1.X), (int)Math.Round(p1.Y), (int)Math.Round(p2.X), (int)Math.Round(p2.Y));
				clipLine(-1, (int)Math.Round(p2.X), (int)Math.Round(p2.Y), (int)Math.Round(p3.X), (int)Math.Round(p3.Y));
				return;
			}
			PointF p4 = PointF_mid(p0, p1);
			PointF pointF = PointF_mid(p1, p2);
			PointF p5 = PointF_mid(p2, p3);
			PointF pointF2 = PointF_mid(p4, pointF);
			PointF pointF3 = PointF_mid(pointF, p5);
			PointF pointF4 = PointF_mid(pointF2, pointF3);
			subdivCurve(p0, p4, pointF2, pointF4);
			subdivCurve(pointF4, pointF3, p5, p3);
		}

		private void clipCurve(int tickCode, int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
		{
			if (regionCode(x0, y0) == 0 && regionCode(x1, y1) == 0 && regionCode(x2, y2) == 0 && regionCode(x3, y3) == 0)
			{
				if (!penIsDown)
				{
					base.moveTo(x0, y0);
				}
				if (-1 == tickCode)
				{
					base.curveTo(x0, y0, x1, y1, x2, y2, x3, y3);
				}
				else
				{
					base.curveTick(tickCode, x0, y0, x1, y1, x2, y2, x3, y3);
				}
				penIsDown = true;
			}
			else
			{
				PointF p = new PointF(x0, y0);
				PointF p2 = new PointF(x1, y1);
				PointF p3 = new PointF(x2, y2);
				PointF p4 = new PointF(x3, y3);
				subdivCurve(p, p2, p3, p4);
			}
		}

		public byte regionCode(float x, float y)
		{
			return regionCode((int)Math.Round(x), (int)Math.Round(y));
		}

		public byte regionCode(int x, int y)
		{
			byte b = 0;
			if (x < PaperOriginX)
			{
				b = (byte)(b + 4);
			}
			else if (x > PaperCornerX)
			{
				b = (byte)(b + 1);
			}
			if (y < PaperOriginY)
			{
				b = (byte)(b + 8);
			}
			else if (y > PaperCornerY)
			{
				b = (byte)(b + 2);
			}
			return b;
		}

		public bool cs_clip(ref int x0, ref int y0, ref int x1, ref int y1)
		{
			byte b = regionCode(x0, y0);
			byte b2 = regionCode(x1, y1);
			while ((b | b2) != 0)
			{
				if ((b & b2) != 0)
				{
					return false;
				}
				float num = 0f;
				float num2 = 0f;
				byte b3 = b;
				if (b3 == 0)
				{
					b3 = b2;
				}
				if ((4u & b3) != 0)
				{
					num = PaperOriginX;
					num2 = y0 + (y1 - y0) * (PaperOriginX - x0) / (x1 - x0);
				}
				else if (((true ? 1u : 0u) & (uint)b3) != 0)
				{
					num = PaperCornerX;
					num2 = y0 + (y1 - y0) * (PaperCornerX - x0) / (x1 - x0);
				}
				else if ((8u & b3) != 0)
				{
					num = x0 + (x1 - x0) * (PaperOriginY - y0) / (y1 - y0);
					num2 = PaperOriginY;
				}
				else if ((2u & b3) != 0)
				{
					num = x0 + (x1 - x0) * (PaperCornerY - y0) / (y1 - y0);
					num2 = PaperCornerY;
				}
				if (b3 == b)
				{
					x0 = (int)Math.Round(num);
					y0 = (int)Math.Round(num2);
					b = regionCode(x0, y0);
				}
				else
				{
					x1 = (int)Math.Round(num);
					y1 = (int)Math.Round(num2);
					b2 = regionCode(x1, y1);
				}
			}
			return true;
		}
	}
}
