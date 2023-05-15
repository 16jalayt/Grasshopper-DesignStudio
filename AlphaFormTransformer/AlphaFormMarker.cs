using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AlphaFormTransformer
{
	public class AlphaFormMarker : UserControl
	{
		private uint m_fillBorder = 4u;

		[Description("Fill Border (Pixels)")]
		[DefaultValue(4)]
		[Category("Marker Properties")]
		public uint FillBorder
		{
			get
			{
				return m_fillBorder;
			}
			set
			{
				m_fillBorder = value;
			}
		}

		public AlphaFormMarker()
		{
			base.Bounds = new Rectangle(base.Location, new Size(17, 17));
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			InitializeStyles();
			base.OnHandleCreated(e);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			SolidBrush solidBrush = new SolidBrush(Color.FromArgb(255, 255, 0, 0));
			Pen pen = new Pen(solidBrush, 1f);
			e.Graphics.DrawEllipse(pen, new Rectangle(0, 0, base.ClientSize.Width - 1, base.ClientSize.Height - 1));
			e.Graphics.DrawLine(pen, base.Bounds.Width / 2, 0, base.Bounds.Width / 2, base.Bounds.Height);
			e.Graphics.DrawLine(pen, 0, base.Bounds.Height / 2, base.Bounds.Width, base.Bounds.Height / 2);
			pen.Dispose();
			solidBrush.Dispose();
		}

		private void InitializeStyles()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			UpdateStyles();
		}
	}
}
