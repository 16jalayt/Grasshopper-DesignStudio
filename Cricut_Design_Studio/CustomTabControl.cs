using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Cricut_Design_Studio
{
	[ToolboxBitmap(typeof(TabControl))]
	public class CustomTabControl : TabControl
	{
		public enum TabControlDisplayManager
		{
			Default,
			Custom
		}

		public Color dimTabColor = Color.FromArgb(255, 128, 128, 128);

		public Color backColor = Color.FromArgb(255, 255, 255, 255);

		public Color borderColor = Color.Black;

		private TabControlDisplayManager _DisplayManager = TabControlDisplayManager.Custom;

		[DefaultValue(typeof(TabControlDisplayManager), "Custom")]
		public TabControlDisplayManager DisplayManager
		{
			get
			{
				return _DisplayManager;
			}
			set
			{
				if (_DisplayManager != value)
				{
					if (_DisplayManager.Equals(TabControlDisplayManager.Custom))
					{
						SetStyle(ControlStyles.UserPaint, value: true);
						base.ItemSize = new Size(0, 15);
					}
					else
					{
						SetStyle(ControlStyles.UserPaint, value: false);
						base.ItemSize = new Size(0, 0);
					}
				}
			}
		}

		public CustomTabControl()
		{
			if (_DisplayManager.Equals(TabControlDisplayManager.Custom))
			{
				SetStyle(ControlStyles.UserPaint, value: true);
				base.ItemSize = new Size(0, 15);
			}
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			base.ResizeRedraw = true;
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			if (base.DesignMode)
			{
				SolidBrush solidBrush = new SolidBrush(backColor);
				pevent.Graphics.FillRectangle(solidBrush, base.Bounds);
				solidBrush.Dispose();
			}
			else
			{
				PaintEventArgs e = new PaintEventArgs(pevent.Graphics, base.Bounds);
				InvokePaint(base.Parent, e);
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			PaintAllTheTabs(e);
			PaintTheTabPageBorder(e);
			PaintTheSelectedTab(e);
		}

		private void PaintAllTheTabs(PaintEventArgs e)
		{
			if (base.TabCount > 0)
			{
				for (int i = 0; i < base.TabCount; i++)
				{
					PaintTab(e, i);
				}
			}
		}

		private void PaintTab(PaintEventArgs e, int index)
		{
			GraphicsPath path = GetPath(index);
			PaintTabBackground(e.Graphics, index, path);
			PaintTabBorder(e.Graphics, index, path);
			PaintTabText(e.Graphics, index);
		}

		private void PaintTabBackground(Graphics graph, int index, GraphicsPath path)
		{
			GetTabRect(index);
			Brush brush = new SolidBrush(dimTabColor);
			if (index == base.SelectedIndex)
			{
				brush = new SolidBrush(backColor);
			}
			graph.FillPath(brush, path);
			brush.Dispose();
		}

		private void PaintTabBorder(Graphics graph, int index, GraphicsPath path)
		{
			Pen pen = new Pen(Color.FromArgb(255, 64, 64, 64));
			if (index == base.SelectedIndex)
			{
				pen = new Pen(borderColor);
			}
			graph.DrawPath(pen, path);
			pen.Dispose();
		}

		private void PaintTabText(Graphics graph, int index)
		{
			Rectangle tabRect = GetTabRect(index);
			string s = base.TabPages[index].Text;
			StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Center;
			stringFormat.LineAlignment = StringAlignment.Center;
			stringFormat.Trimming = StringTrimming.EllipsisCharacter;
			SizeF sizeF = graph.MeasureString(s, Font);
			int num = tabRect.Width - (int)sizeF.Width;
			new Rectangle(tabRect.Left + num / 2, tabRect.Top, tabRect.Width - num, tabRect.Height);
			SolidBrush solidBrush = new SolidBrush(ForeColor);
			graph.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			graph.DrawString(s, Font, solidBrush, tabRect, stringFormat);
			solidBrush.Dispose();
		}

		private void PaintTheTabPageBorder(PaintEventArgs e)
		{
			if (base.TabCount > 0)
			{
				Rectangle bounds = base.TabPages[0].Bounds;
				bounds.Inflate(1, 1);
				ControlPaint.DrawBorder(e.Graphics, bounds, borderColor, ButtonBorderStyle.Solid);
			}
		}

		private void PaintTheSelectedTab(PaintEventArgs e)
		{
			if (base.SelectedIndex < base.TabPages.Count)
			{
				int num = 0;
				Pen pen = new Pen(backColor);
				switch (base.SelectedIndex)
				{
				case 0:
				{
					Rectangle tabRect = GetTabRect(base.SelectedIndex);
					num = tabRect.Right;
					e.Graphics.DrawLine(pen, tabRect.Left + 2, tabRect.Bottom + 1, num - 2, tabRect.Bottom + 1);
					break;
				}
				default:
				{
					Rectangle tabRect = GetTabRect(base.SelectedIndex);
					num = tabRect.Right;
					e.Graphics.DrawLine(pen, tabRect.Left, tabRect.Bottom + 1, num - 2, tabRect.Bottom + 1);
					break;
				}
				case -1:
					break;
				}
			}
		}

		private GraphicsPath GetPath(int index)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.Reset();
			Rectangle tabRect = GetTabRect(index);
			if (index == 0)
			{
				graphicsPath.AddLine(tabRect.Left + 1, tabRect.Bottom + 1, tabRect.Left + 1, tabRect.Top + 2);
				graphicsPath.AddLine(tabRect.Left + 1, tabRect.Top + 2, tabRect.Left + 3, tabRect.Top);
				graphicsPath.AddLine(tabRect.Left + 3, tabRect.Top, tabRect.Right - 3, tabRect.Top);
				graphicsPath.AddLine(tabRect.Right - 3, tabRect.Top, tabRect.Right - 1, tabRect.Top + 2);
				graphicsPath.AddLine(tabRect.Right - 1, tabRect.Top + 2, tabRect.Right - 1, tabRect.Bottom + 1);
			}
			else
			{
				graphicsPath.AddLine(tabRect.Left, tabRect.Bottom + 1, tabRect.Left, tabRect.Top + 2);
				graphicsPath.AddLine(tabRect.Left, tabRect.Top + 2, tabRect.Left + 2, tabRect.Top);
				graphicsPath.AddLine(tabRect.Left + 2, tabRect.Top, tabRect.Right - 3, tabRect.Top);
				graphicsPath.AddLine(tabRect.Right - 3, tabRect.Top, tabRect.Right - 1, tabRect.Top + 2);
				graphicsPath.AddLine(tabRect.Right - 1, tabRect.Top + 2, tabRect.Right - 1, tabRect.Bottom + 1);
			}
			return graphicsPath;
		}
	}
}
