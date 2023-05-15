using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AlphaFormTransformer
{
	public class AlphaFormTransformer : Panel
	{
		private bool m_drag;

		private Point m_dragStart;

		private LayeredWindowForm m_lwin;

		private Bitmap m_alphaBitmap;

		private uint m_dragSleep = 30u;

		[DefaultValue(30)]
		[Category("Alpha Transformer Properties")]
		[Description("Drag Sleep in Milliseconds (Ignored on Vista)")]
		public uint DragSleep
		{
			get
			{
				return m_dragSleep;
			}
			set
			{
				m_dragSleep = value;
			}
		}

		private Form ParentForm => (Form)base.TopLevelControl;

		public AlphaFormTransformer()
		{
			base.Size = new Size(250, 250);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			if (m_alphaBitmap != null && m_lwin != null)
			{
				m_lwin.SetBits(m_alphaBitmap);
			}
		}

		public void UpdateSkin(Bitmap bm)
		{
			if (!Image.IsCanonicalPixelFormat(bm.PixelFormat) || !Image.IsAlphaPixelFormat(bm.PixelFormat))
			{
				throw new ApplicationException("The bitmap must be 32 bits per pixel with an alpha channel.");
			}
			m_alphaBitmap = bm;
			BitmapData bitmapData = m_alphaBitmap.LockBits(new Rectangle(0, 0, m_alphaBitmap.Width, m_alphaBitmap.Height), ImageLockMode.ReadOnly, m_alphaBitmap.PixelFormat);
			byte[,] array = new byte[m_alphaBitmap.Width, m_alphaBitmap.Height];
			byte[] array2 = new byte[m_alphaBitmap.Height * bitmapData.Stride];
			Marshal.Copy(bitmapData.Scan0, array2, 0, array2.Length);
			for (int i = 0; i < m_alphaBitmap.Height; i++)
			{
				int num = i * bitmapData.Stride + 3;
				int num2 = 0;
				while (num2 < m_alphaBitmap.Width)
				{
					array[num2, i] = array2[num];
					num2++;
					num += 4;
				}
			}
			m_alphaBitmap.UnlockBits(bitmapData);
			Rectangle bounds = default(Rectangle);
			ArrayList rectList = new ArrayList();
			foreach (Control control in base.Controls)
			{
				if (typeof(AlphaFormMarker).IsInstanceOfType(control))
				{
					AlphaFormMarker alphaFormMarker = (AlphaFormMarker)control;
					UpdateRectListFromAlpha(rectList, ref bounds, new Point(alphaFormMarker.Location.X + alphaFormMarker.Width / 2, alphaFormMarker.Location.Y + alphaFormMarker.Height / 2), array, m_alphaBitmap.Width, m_alphaBitmap.Height, (int)alphaFormMarker.FillBorder);
					alphaFormMarker.Visible = false;
				}
			}
			ParentForm.Region = RegionFromRectList(rectList, bounds);
			m_lwin.SetBits(m_alphaBitmap);
		}

		public void TransformForm()
		{
			if (base.DesignMode)
			{
				return;
			}
			m_lwin = new LayeredWindowForm();
			m_lwin.TopMost = ParentForm.TopMost;
			m_lwin.ShowInTaskbar = false;
			m_lwin.MouseDown += LayeredFormMouseDown;
			m_lwin.MouseMove += LayeredFormMouseMove;
			m_lwin.MouseUp += LayeredFormMouseUp;
			ParentForm.Move += ParentFormMove;
			m_lwin.Show(ParentForm);
			m_lwin.Location = ParentForm.Location;
			if (BackgroundImage != null)
			{
				Bitmap bitmap;
				if (BackgroundImage.Size != ParentForm.Size)
				{
					bitmap = new Bitmap(ParentForm.Width, ParentForm.Height);
					Graphics graphics = Graphics.FromImage(bitmap);
					graphics.SmoothingMode = SmoothingMode.HighQuality;
					graphics.CompositingQuality = CompositingQuality.HighQuality;
					graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
					graphics.DrawImage(BackgroundImage, new Rectangle(0, 0, ParentForm.Width, ParentForm.Height), new Rectangle(0, 0, BackgroundImage.Width, BackgroundImage.Height), GraphicsUnit.Pixel);
					graphics.Dispose();
				}
				else
				{
					bitmap = new Bitmap(BackgroundImage);
				}
				UpdateSkin(bitmap);
				BackgroundImage = ParentForm.BackgroundImage;
				BackgroundImageLayout = ParentForm.BackgroundImageLayout;
				if (BackColor == Color.Transparent)
				{
					BackColor = SystemColors.Control;
				}
			}
		}

		private void PushSeg(Stack stack, int x1, int xr, int y, int dy, int height)
		{
			int num = y + dy;
			if (num >= 0 && num < height)
			{
				stack.Push(new LineSeg(x1, xr, y, dy));
			}
		}

		private void PopSeg(Stack stack, out int xl, out int xr, out int y, out int dy)
		{
			LineSeg lineSeg = (LineSeg)stack.Pop();
			xl = lineSeg.x1;
			xr = lineSeg.x2;
			dy = lineSeg.dy;
			y = lineSeg.y + dy;
		}

		private void UpdateRectListFromAlpha(ArrayList rectList, ref Rectangle bounds, Point seedPt, byte[,] alphaArr, int width, int height, int border)
		{
			Stack stack = new Stack();
			Rectangle rectangle = default(Rectangle);
			if (rectList.Count == 0)
			{
				bounds.X = int.MaxValue;
				bounds.Y = int.MaxValue;
				bounds.Width = 0;
				bounds.Height = 0;
			}
			if (alphaArr[seedPt.X, seedPt.Y] != 0 || seedPt.X < 0 || seedPt.X >= width || seedPt.Y < 0 || seedPt.Y >= height)
			{
				return;
			}
			PushSeg(stack, seedPt.X, seedPt.X, seedPt.Y, 1, height);
			PushSeg(stack, seedPt.X, seedPt.X, seedPt.Y + 1, -1, height);
			while (stack.Count > 0)
			{
				PopSeg(stack, out var xl, out var xr, out var num, out var dy);
				int num2 = xl;
				while (num2 >= 0 && alphaArr[num2, num] == 0)
				{
					rectangle.X = num2 - border;
					rectangle.Y = num - border;
					rectangle.Width = 2 * border + 1;
					rectangle.Height = rectangle.Width;
					rectList.Add(rectangle);
					if (rectangle.Left < bounds.Left)
					{
						bounds.X = rectangle.Left;
					}
					if (rectangle.Top < bounds.Top)
					{
						bounds.Y = rectangle.Top;
					}
					if (rectangle.Width > bounds.Width)
					{
						bounds.Width = rectangle.Width;
					}
					if (rectangle.Height > bounds.Height)
					{
						bounds.Height = rectangle.Height;
					}
					alphaArr[num2, num] = 1;
					num2--;
				}
				int num3;
				if (num2 < xl)
				{
					num3 = num2 + 1;
					if (num3 < xl)
					{
						PushSeg(stack, num3, xl - 1, num, -dy, height);
					}
					num2 = xl + 1;
				}
				else
				{
					for (num2++; num2 <= xr && alphaArr[num2, num] != 0; num2++)
					{
					}
					num3 = num2;
					if (num2 > xr)
					{
						continue;
					}
				}
				while (true)
				{
					if (num2 < width && alphaArr[num2, num] == 0)
					{
						rectangle.X = num2 - border;
						rectangle.Y = num - border;
						rectangle.Width = 2 * border + 1;
						rectangle.Height = rectangle.Width;
						rectList.Add(rectangle);
						if (rectangle.X < bounds.Left)
						{
							bounds.X = rectangle.Left;
						}
						if (rectangle.Y < bounds.Top)
						{
							bounds.Y = rectangle.Top;
						}
						if (rectangle.Width > bounds.Width)
						{
							bounds.Width = rectangle.Width;
						}
						if (rectangle.Height > bounds.Height)
						{
							bounds.Height = rectangle.Height;
						}
						alphaArr[num2, num] = 1;
						num2++;
					}
					else
					{
						PushSeg(stack, num3, num2 - 1, num, dy, height);
						if (num2 > xr + 1)
						{
							PushSeg(stack, xr + 1, num2 - 1, num, -dy, height);
						}
						for (num2++; num2 <= xr && alphaArr[num2, num] != 0; num2++)
						{
						}
						num3 = num2;
						if (num2 > xr)
						{
							break;
						}
					}
				}
			}
		}

		private Region RegionFromRectList(ArrayList rectList, Rectangle bounds)
		{
			uint num = (uint)(32 + rectList.Count * 16);
			IntPtr intPtr = Marshal.AllocHGlobal((int)num);
			int[] array = new int[rectList.Count * 4 + 8];
			array[0] = 32;
			array[1] = 1;
			array[2] = rectList.Count;
			array[3] = rectList.Count * 16;
			array[4] = bounds.Left;
			array[5] = bounds.Top;
			array[6] = bounds.Width;
			array[7] = bounds.Height;
			for (int i = 0; i < rectList.Count; i++)
			{
				Rectangle rectangle = (Rectangle)rectList[i];
				array[4 * i + 8] = rectangle.Left;
				array[4 * i + 9] = rectangle.Top;
				array[4 * i + 10] = rectangle.Right;
				array[4 * i + 11] = rectangle.Bottom;
			}
			Marshal.Copy(array, 0, intPtr, array.Length);
			IntPtr hrgn = Win32.ExtCreateRegion(new IntPtr(0), num, intPtr);
			Marshal.FreeHGlobal(intPtr);
			return Region.FromHrgn(hrgn);
		}

		private void LayeredFormMouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				m_drag = true;
				m_dragStart = e.Location;
			}
		}

		private void LayeredFormMouseMove(object sender, MouseEventArgs e)
		{
			if (m_drag)
			{
				int num = e.X - m_dragStart.X;
				int num2 = e.Y - m_dragStart.Y;
				ParentForm.Location = new Point(ParentForm.Location.X + num, ParentForm.Location.Y + num2);
				ParentForm.Update();
			}
		}

		private void LayeredFormMouseUp(object sender, MouseEventArgs e)
		{
			m_drag = false;
		}

		private void ParentFormMove(object sender, EventArgs e)
		{
			m_lwin.Location = ParentForm.Location;
			if (m_drag && Environment.OSVersion.Version.Major < 6)
			{
				Thread.Sleep((int)m_dragSleep);
			}
		}
	}
}
