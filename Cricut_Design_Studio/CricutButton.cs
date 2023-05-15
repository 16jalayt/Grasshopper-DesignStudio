using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Cricut_Design_Studio
{
	[Description("Custom Button")]
	public class CricutButton : Button
	{
		public enum BitmapAlignment
		{
			AlignTop,
			AlignBottom,
			AlignCenter,
			AlignNone
		}

		public Bitmap normalImg;

		public Bitmap hotImg;

		public Bitmap pressedImg;

		public Bitmap selectedImg;

		public Bitmap selectedDownImg;

		public bool selected;

		private bool mousePressed;

		private bool mouseOver;

		public BitmapAlignment bitmapAlignment = BitmapAlignment.AlignNone;

		public bool drawEdge;

		public CricutButton()
		{
			SetStyle(ControlStyles.StandardClick, value: true);
		}

		private Bitmap lerp(Bitmap img1, Bitmap img2)
		{
			Bitmap bitmap = (Bitmap)img1.Clone();
			for (int i = 0; i < bitmap.Height; i++)
			{
				for (int j = 0; j < bitmap.Width; j++)
				{
					Color pixel = img1.GetPixel(j, i);
					Color pixel2 = img2.GetPixel(j, i);
					bitmap.SetPixel(j, i, Color.FromArgb(pixel.A, (pixel.R + pixel2.R) / 2, (pixel.G + pixel2.G) / 2, (pixel.B + pixel2.B) / 2));
				}
			}
			return bitmap;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Font font = Font;
			SolidBrush brush = new SolidBrush(Color.Black);
			e.Graphics.MeasureString(Text, font);
			Rectangle clientRectangle = base.ClientRectangle;
			int destY = 0;
			int num = base.Height;
			e.Graphics.Clear(BackColor);
			e.Graphics.CompositingMode = CompositingMode.SourceOver;
			if (normalImg != null)
			{
				switch (bitmapAlignment)
				{
				case BitmapAlignment.AlignTop:
					destY = 0;
					clientRectangle.Location = new Point(clientRectangle.Location.X, clientRectangle.Location.Y + 4);
					break;
				case BitmapAlignment.AlignCenter:
					destY = (clientRectangle.Height - normalImg.Height) / 2;
					clientRectangle.Location = new Point(clientRectangle.Location.X, clientRectangle.Location.Y);
					num = 24;
					break;
				case BitmapAlignment.AlignBottom:
					destY = clientRectangle.Height - normalImg.Height;
					clientRectangle.Location = new Point(clientRectangle.Location.X, clientRectangle.Location.Y - 4);
					break;
				}
			}
			if (mousePressed)
			{
				if (selected && selectedDownImg != null)
				{
					DrawImage(e.Graphics, selectedDownImg, 0, destY, num);
				}
				else if (pressedImg != null)
				{
					DrawImage(e.Graphics, pressedImg, 0, destY, num);
				}
				clientRectangle.Location = new Point(clientRectangle.Location.X, clientRectangle.Location.Y);
			}
			else if (mouseOver)
			{
				if (selected && selectedImg != null)
				{
					DrawImage(e.Graphics, selectedImg, 0, destY, num);
				}
				else if (hotImg != null)
				{
					DrawImage(e.Graphics, hotImg, 0, destY, num);
				}
				clientRectangle.Location = new Point(clientRectangle.Location.X - 1, clientRectangle.Location.Y - 1);
			}
			else
			{
				if (selected && selectedImg != null)
				{
					DrawImage(e.Graphics, selectedImg, 0, destY, num);
				}
				else if (normalImg != null)
				{
					DrawImage(e.Graphics, normalImg, 0, destY, num);
				}
				clientRectangle.Location = new Point(clientRectangle.Location.X - 1, clientRectangle.Location.Y - 1);
			}
			if (drawEdge)
			{
				Pen pen = new Pen(Color.FromArgb(64, 0, 0, 0));
				e.Graphics.DrawLine(pen, 5, 0, base.Width - 6, 0);
				e.Graphics.DrawLine(pen, 5, base.Height - 1, base.Width - 6, base.Height - 1);
			}
			StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Center;
			stringFormat.LineAlignment = StringAlignment.Center;
			if (Text != null && Text.Length > 0)
			{
				e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
				e.Graphics.DrawString(Text, font, brush, clientRectangle, stringFormat);
			}
		}

		private void DrawImage(Graphics graphics, Bitmap img, int destX, int destY, int height)
		{
			RectangleF destRect = new RectangleF(0f, 0f, base.Width, base.Height);
			RectangleF srcRect = new RectangleF(destX, -destY, img.Width, height);
			graphics.DrawImage(img, destRect, srcRect, GraphicsUnit.Pixel);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				mousePressed = true;
			}
			base.OnMouseDown(e);
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			mouseOver = true;
			base.OnMouseEnter(e);
			Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			mouseOver = false;
			Invalidate();
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (mousePressed)
			{
				mousePressed = false;
				Invalidate();
			}
		}
	}
}
