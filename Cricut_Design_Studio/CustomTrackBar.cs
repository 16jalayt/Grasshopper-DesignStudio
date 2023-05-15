using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Cricut_Design_Studio
{
	internal class CustomTrackBar : Control
	{
		private Timer timer = new Timer();

		private int numberTicks = 10;

		private Rectangle trackRectangle = default(Rectangle);

		private Rectangle ticksRectangle = default(Rectangle);

		private Rectangle thumbRectangle = default(Rectangle);

		private int currentTickPosition = 5;

		private float tickSpace;

		private bool thumbClicked;

		private TrackBarThumbState thumbState = TrackBarThumbState.Normal;

		private Bitmap bkgnd;

		private Bitmap thumb;

		private Bitmap thumb_hot;

		public CustomTrackBar(int ticks, int x, int y, Size trackBarSize)
		{
			base.Location = new Point(x, y);
			base.Size = trackBarSize;
			numberTicks = ticks;
			DoubleBuffered = true;
			bkgnd = MarcusResources.TrackBarBkgnd;
			thumb = MarcusResources.TrackBarThumb;
			thumb_hot = MarcusResources.TrackBarThumb_hot;
			SetupTrackBar();
			timer.Interval = 250;
			timer.Tick += timer_Tick;
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			int num = 0;
			switch (currentTickPosition)
			{
			case 0:
				timer.Interval = 50;
				num = -1;
				break;
			case 1:
				timer.Interval = 100;
				num = -1;
				break;
			case 2:
				timer.Interval = 200;
				num = -1;
				break;
			case 3:
				timer.Interval = 250;
				num = -1;
				break;
			case 4:
				timer.Interval = 500;
				num = -1;
				break;
			case 5:
				timer.Interval = 50;
				num = 0;
				break;
			case 6:
				timer.Interval = 500;
				num = 1;
				break;
			case 7:
				timer.Interval = 250;
				num = 1;
				break;
			case 8:
				timer.Interval = 200;
				num = 1;
				break;
			case 9:
				timer.Interval = 100;
				num = 1;
				break;
			case 10:
				timer.Interval = 50;
				num = 1;
				break;
			}
			if (num != 0)
			{
				Form1.myRootForm.sizeTrackBar_ValueChanged(num);
			}
		}

		private void SetupTrackBar()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			using (CreateGraphics())
			{
				trackRectangle.X = clientRectangle.X + 2;
				trackRectangle.Y = clientRectangle.Y + 2;
				trackRectangle.Width = clientRectangle.Width - 4;
				trackRectangle.Height = 11;
				ticksRectangle.X = trackRectangle.X + 2;
				ticksRectangle.Y = trackRectangle.Y - 8;
				ticksRectangle.Width = trackRectangle.Width - 4;
				ticksRectangle.Height = 4;
				tickSpace = ((float)ticksRectangle.Width - 1f) / ((float)numberTicks - 1f);
				thumbRectangle.Size = new Size(thumb.Width, thumb.Height);
				thumbRectangle.X = CurrentTickXCoordinate();
				thumbRectangle.Y = trackRectangle.Y + 2;
			}
		}

		private int CurrentTickXCoordinate()
		{
			if (tickSpace == 0f)
			{
				return 0;
			}
			return (int)Math.Round(tickSpace) * currentTickPosition;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.Parent.Text = "CustomTrackBar Enabled";
			e.Graphics.DrawImage(bkgnd, trackRectangle.X, trackRectangle.Y, bkgnd.Width, bkgnd.Height);
			SolidBrush solidBrush = new SolidBrush(Color.FromArgb(255, 128, 128, 128));
			e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			Font font = new Font("Arial", 9f);
			e.Graphics.DrawString("_", font, solidBrush, 2f, 6f);
			font = new Font("Arial", 10f);
			e.Graphics.DrawString("+", font, solidBrush, trackRectangle.Width - 9, 10f);
			solidBrush.Dispose();
			if (TrackBarThumbState.Hot == thumbState || TrackBarThumbState.Pressed == thumbState)
			{
				e.Graphics.DrawImage(thumb_hot, thumbRectangle.X, thumbRectangle.Y, thumb.Width, thumb.Height);
			}
			else
			{
				e.Graphics.DrawImage(thumb, thumbRectangle.X, thumbRectangle.Y, thumb.Width, thumb.Height);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (thumbRectangle.Contains(e.Location))
			{
				thumbClicked = true;
				thumbState = TrackBarThumbState.Pressed;
				timer.Interval = 50;
				timer.Start();
			}
			else
			{
				int num = (trackRectangle.Left + trackRectangle.Right) / 2;
				if (e.X < num)
				{
					int num2 = (int)Math.Round((float)(num - e.X) / tickSpace);
					currentTickPosition = numberTicks / 2 - num2;
					thumbRectangle.X = CurrentTickXCoordinate();
					thumbClicked = true;
					thumbState = TrackBarThumbState.Pressed;
					timer.Interval = 500;
					timer.Start();
				}
				else if (e.X > num)
				{
					int num3 = (int)Math.Round((float)(e.X - num) / tickSpace);
					currentTickPosition = numberTicks / 2 + num3;
					thumbRectangle.X = CurrentTickXCoordinate();
					thumbClicked = true;
					thumbState = TrackBarThumbState.Pressed;
					timer.Interval = 500;
					timer.Start();
				}
				int num4 = 0;
				switch (currentTickPosition)
				{
				case 0:
					num4 = -16;
					break;
				case 1:
					num4 = -8;
					break;
				case 2:
					num4 = -4;
					break;
				case 3:
					num4 = -2;
					break;
				case 4:
					num4 = -1;
					break;
				case 5:
					num4 = 0;
					break;
				case 6:
					num4 = 1;
					break;
				case 7:
					num4 = 2;
					break;
				case 8:
					num4 = 4;
					break;
				case 9:
					num4 = 8;
					break;
				case 10:
					num4 = 16;
					break;
				}
				if (num4 != 0)
				{
					Form1.myRootForm.sizeTrackBar_ValueChanged(num4);
				}
			}
			Invalidate();
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (thumbClicked)
			{
				if (e.Location.X > trackRectangle.X && e.Location.X < trackRectangle.X + trackRectangle.Width - thumbRectangle.Width)
				{
					thumbClicked = false;
					thumbState = TrackBarThumbState.Hot;
					Invalidate();
				}
				thumbClicked = false;
			}
			timer.Stop();
			currentTickPosition = numberTicks / 2;
			thumbRectangle.X = CurrentTickXCoordinate();
			Invalidate();
			Form1.myRootForm.refreshMattePicBox();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (thumbClicked)
			{
				if (currentTickPosition < numberTicks - 1 && e.Location.X > CurrentTickXCoordinate() + (int)tickSpace)
				{
					currentTickPosition++;
				}
				else if (currentTickPosition > 0 && e.Location.X < CurrentTickXCoordinate() - (int)(tickSpace / 2f))
				{
					currentTickPosition--;
				}
				thumbRectangle.X = CurrentTickXCoordinate();
			}
			else
			{
				thumbState = ((!thumbRectangle.Contains(e.Location)) ? TrackBarThumbState.Normal : TrackBarThumbState.Hot);
			}
			Invalidate();
		}
	}
}
