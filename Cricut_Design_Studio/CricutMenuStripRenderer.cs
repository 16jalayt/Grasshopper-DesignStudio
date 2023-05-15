using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cricut_Design_Studio
{
	public class CricutMenuStripRenderer : ToolStripProfessionalRenderer
	{
		private class GradientItemColors
		{
			public Color InsideTop1;

			public Color InsideTop2;

			public Color InsideBottom1;

			public Color InsideBottom2;

			public Color FillTop1;

			public Color FillTop2;

			public Color FillBottom1;

			public Color FillBottom2;

			public Color Border1;

			public Color Border2;

			public GradientItemColors(Color insideTop1, Color insideTop2, Color insideBottom1, Color insideBottom2, Color fillTop1, Color fillTop2, Color fillBottom1, Color fillBottom2, Color border1, Color border2)
			{
				InsideTop1 = insideTop1;
				InsideTop2 = insideTop2;
				InsideBottom1 = insideBottom1;
				InsideBottom2 = insideBottom2;
				FillTop1 = fillTop1;
				FillTop2 = fillTop2;
				FillBottom1 = fillBottom1;
				FillBottom2 = fillBottom2;
				Border1 = border1;
				Border2 = border2;
			}
		}

		private static int _gripOffset;

		private static int _gripSquare;

		private static int _gripSize;

		private static int _gripMove;

		private static int _gripLines;

		private static int _checkInset;

		private static int _marginInset;

		private static int _separatorInset;

		private static float _cutToolItemMenu;

		private static float _cutContextMenu;

		private static float _cutMenuItemBack;

		private static float _contextCheckTickThickness;

		private static Blend _statusStripBlend;

		private static Color _c1;

		private static Color _c2;

		private static Color _c3;

		private static Color _c4;

		private static Color _c5;

		private static Color _c6;

		private static Color borderColor;

		private static Color fillColor;

		private static Color _r1;

		private static Color _r2;

		private static Color _r3;

		private static Color _r4;

		private static Color _r5;

		private static Color _r6;

		private static Color _r7;

		private static Color _r8;

		private static Color _r9;

		private static Color _rA;

		private static Color _o1;

		private static Color _o2;

		private static Color _rB;

		private static Color _rC;

		private static Color _rD;

		private static Color _rE;

		private static Color _rF;

		private static Color _rG;

		private static Color _rH;

		private static Color _rI;

		private static Color _rJ;

		private static Color _rK;

		private static Color _rL;

		private static Color _rM;

		private static Color _rN;

		private static Color _rO;

		private static Color _rP;

		private static Color _rQ;

		private static Color _rR;

		private static Color _rS;

		private static Color _rT;

		private static Color _rU;

		private static Color _rV;

		private static Color _rW;

		private static Color _rX;

		private static Color _rY;

		private static Color _rZ;

		private static Color _pb1;

		private static Color _pb2;

		private static Color _pb3;

		private static Color _pb4;

		private static Color _textDisabled;

		private static Color _textMenuStripItem;

		private static Color _textStatusStripItem;

		private static Color _textContextMenuItem;

		private static Color _arrowDisabled;

		private static Color _arrowLight;

		private static Color _arrowDark;

		private static Color _separatorMenuLight;

		private static Color _separatorMenuDark;

		private static Color _contextMenuBack;

		private static Color _contextCheckBorder;

		private static Color _contextCheckTick;

		private static Color _statusStripBorderDark;

		private static Color _statusStripBorderLight;

		private static Color _gripDark;

		private static Color _gripLight;

		private static GradientItemColors _itemContextItemEnabledColors;

		private static GradientItemColors _itemDisabledColors;

		private static GradientItemColors _itemToolItemSelectedColors;

		private static GradientItemColors _itemToolItemPressedColors;

		private static GradientItemColors _itemToolItemCheckedColors;

		private static GradientItemColors _itemToolItemCheckPressColors;

		static CricutMenuStripRenderer()
		{
			_gripOffset = 1;
			_gripSquare = 2;
			_gripSize = 3;
			_gripMove = 4;
			_gripLines = 3;
			_checkInset = 1;
			_marginInset = 2;
			_separatorInset = 31;
			_cutToolItemMenu = 1f;
			_cutContextMenu = 0f;
			_cutMenuItemBack = 1.2f;
			_contextCheckTickThickness = 1.6f;
			_c1 = Color.FromArgb(167, 167, 167);
			_c2 = Color.FromArgb(21, 66, 139);
			_c3 = Color.FromArgb(76, 83, 92);
			_c4 = Color.FromArgb(250, 250, 250);
			_c5 = Color.FromArgb(248, 248, 248);
			_c6 = Color.FromArgb(243, 243, 243);
			borderColor = Color.FromArgb(206, 227, 240);
			fillColor = Color.FromArgb(206, 227, 240);
			_r1 = borderColor;
			_r2 = borderColor;
			_r3 = borderColor;
			_r4 = borderColor;
			_r5 = fillColor;
			_r6 = fillColor;
			_r7 = fillColor;
			_r8 = fillColor;
			_r9 = Color.FromArgb(53, 75, 111);
			_rA = Color.FromArgb(53, 75, 111);
			_o1 = Color.FromArgb(53, 75, 111);
			_o2 = Color.FromArgb(53, 75, 111);
			_rB = Color.FromArgb(182, 190, 192);
			_rC = Color.FromArgb(155, 163, 167);
			_rD = Color.FromArgb(233, 168, 97);
			_rE = Color.FromArgb(247, 164, 39);
			_rF = Color.FromArgb(246, 156, 24);
			_rG = Color.FromArgb(253, 173, 17);
			_rH = Color.FromArgb(254, 185, 108);
			_rI = Color.FromArgb(253, 164, 97);
			_rJ = Color.FromArgb(252, 143, 61);
			_rK = Color.FromArgb(255, 208, 134);
			_rL = Color.FromArgb(249, 192, 103);
			_rM = Color.FromArgb(250, 195, 93);
			_rN = Color.FromArgb(248, 190, 81);
			_rO = Color.FromArgb(255, 208, 49);
			_rP = Color.FromArgb(254, 214, 168);
			_rQ = Color.FromArgb(252, 180, 100);
			_rR = Color.FromArgb(252, 161, 54);
			_rS = Color.FromArgb(254, 238, 170);
			_rT = Color.FromArgb(249, 202, 113);
			_rU = Color.FromArgb(250, 205, 103);
			_rV = Color.FromArgb(248, 200, 91);
			_rW = Color.FromArgb(255, 218, 59);
			_rX = Color.FromArgb(254, 185, 108);
			_rY = Color.FromArgb(252, 161, 54);
			_rZ = Color.FromArgb(254, 238, 170);
			_pb1 = Color.FromArgb(53, 75, 111);
			_pb2 = Color.FromArgb(35, 50, 74);
			_pb3 = Color.FromArgb(227, 213, 158);
			_pb4 = Color.FromArgb(255, 248, 219);
			_textDisabled = _c1;
			_textMenuStripItem = _pb1;
			_textStatusStripItem = _pb2;
			_textContextMenuItem = _pb2;
			_arrowDisabled = _c1;
			_arrowLight = Color.FromArgb(106, 126, 197);
			_arrowDark = Color.FromArgb(64, 70, 90);
			_separatorMenuLight = Color.FromArgb(245, 245, 245);
			_separatorMenuDark = Color.FromArgb(197, 197, 197);
			_contextMenuBack = _pb4;
			_contextCheckBorder = Color.FromArgb(140, 160, 120);
			_contextCheckTick = Color.FromArgb(66, 75, 138);
			_statusStripBorderDark = Color.FromArgb(86, 125, 176);
			_statusStripBorderLight = Color.White;
			_gripDark = Color.FromArgb(114, 152, 204);
			_gripLight = _c5;
			_itemContextItemEnabledColors = new GradientItemColors(_r1, _r2, _r3, _r4, _r5, _r6, _r7, _r8, _o1, _o2);
			_itemDisabledColors = new GradientItemColors(_c4, _c6, Color.FromArgb(236, 236, 236), Color.FromArgb(230, 230, 230), _c6, Color.FromArgb(224, 224, 224), Color.FromArgb(200, 200, 200), Color.FromArgb(210, 210, 210), Color.FromArgb(212, 212, 212), Color.FromArgb(195, 195, 195));
			_itemToolItemSelectedColors = new GradientItemColors(_r1, _r2, _r3, _r4, _r5, _r6, _r7, _r8, _r9, _rA);
			_itemToolItemPressedColors = new GradientItemColors(_rD, _rE, _rF, _rG, _rH, _rI, _rJ, _rK, _r9, _rA);
			_itemToolItemCheckedColors = new GradientItemColors(_rL, _rM, _rN, _rO, _rP, _rQ, _rR, _rS, _r9, _rA);
			_itemToolItemCheckPressColors = new GradientItemColors(_rT, _rU, _rV, _rW, _rX, _rI, _rY, _rZ, _r9, _rA);
			_statusStripBlend = new Blend();
			_statusStripBlend.Positions = new float[6] { 0f, 0.25f, 0.25f, 0.57f, 0.86f, 1f };
			_statusStripBlend.Factors = new float[6] { 0.1f, 0.6f, 1f, 0.4f, 0f, 0.95f };
		}

		public CricutMenuStripRenderer()
			: base(new CricutColorTable())
		{
		}

		protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
		{
			if (e.ArrowRectangle.Width <= 0 || e.ArrowRectangle.Height <= 0)
			{
				return;
			}
			using (GraphicsPath graphicsPath = CreateArrowPath(e.Item, e.ArrowRectangle, e.Direction))
			{
				RectangleF bounds = graphicsPath.GetBounds();
				bounds.Inflate(1f, 1f);
				Color color = (e.Item.Enabled ? _arrowLight : _arrowDisabled);
				Color color2 = (e.Item.Enabled ? _arrowDark : _arrowDisabled);
				float angle = 0f;
				switch (e.Direction)
				{
				case ArrowDirection.Right:
					angle = 0f;
					break;
				case ArrowDirection.Left:
					angle = 180f;
					break;
				case ArrowDirection.Down:
					angle = 90f;
					break;
				case ArrowDirection.Up:
					angle = 270f;
					break;
				}
				using (LinearGradientBrush brush = new LinearGradientBrush(bounds, color, color2, angle))
				{
					e.Graphics.FillPath(brush, graphicsPath);
				}
			}
		}

		protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
		{
			ToolStripButton toolStripButton = (ToolStripButton)e.Item;
			if (toolStripButton.Selected || toolStripButton.Pressed || toolStripButton.Checked)
			{
				RenderToolButtonBackground(e.Graphics, toolStripButton, e.ToolStrip);
			}
		}

		protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
		{
			if (e.Item.Selected || e.Item.Pressed)
			{
				RenderToolDropButtonBackground(e.Graphics, e.Item, e.ToolStrip);
			}
		}

		protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
		{
			Rectangle imageRectangle = e.ImageRectangle;
			imageRectangle.Inflate(1, 1);
			if (imageRectangle.Top > _checkInset)
			{
				int num = imageRectangle.Top - _checkInset;
				imageRectangle.Y -= num;
				imageRectangle.Height += num;
			}
			if (imageRectangle.Height <= e.Item.Bounds.Height - _checkInset * 2)
			{
				int num2 = e.Item.Bounds.Height - _checkInset * 2 - imageRectangle.Height;
				imageRectangle.Height += num2;
			}
			using (new UseAntiAlias(e.Graphics))
			{
				using (GraphicsPath path = CreateBorderPath(imageRectangle, _cutMenuItemBack))
				{
					using (SolidBrush brush = new SolidBrush(base.ColorTable.CheckBackground))
					{
						e.Graphics.FillPath(brush, path);
					}
					using (Pen pen = new Pen(_contextCheckBorder))
					{
						e.Graphics.DrawPath(pen, path);
					}
					if (e.Image == null)
					{
						return;
					}
					CheckState checkState = CheckState.Unchecked;
					if (e.Item is ToolStripMenuItem)
					{
						ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)e.Item;
						checkState = toolStripMenuItem.CheckState;
					}
					switch (checkState)
					{
					case CheckState.Checked:
					{
						using (GraphicsPath path3 = CreateTickPath(imageRectangle))
						{
							using (Pen pen2 = new Pen(_contextCheckTick, _contextCheckTickThickness))
							{
								e.Graphics.DrawPath(pen2, path3);
								break;
							}
						}
					}
					case CheckState.Indeterminate:
					{
						using (GraphicsPath path2 = CreateIndeterminatePath(imageRectangle))
						{
							using (SolidBrush brush2 = new SolidBrush(_contextCheckTick))
							{
								e.Graphics.FillPath(brush2, path2);
								break;
							}
						}
					}
					}
				}
			}
		}

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			if (e.ToolStrip is MenuStrip || e.ToolStrip != null || e.ToolStrip is ContextMenuStrip || e.ToolStrip is ToolStripDropDownMenu)
			{
				if (!e.Item.Enabled)
				{
					e.TextColor = _textDisabled;
				}
				else if (e.ToolStrip is MenuStrip && !e.Item.Pressed && !e.Item.Selected)
				{
					e.TextColor = _textMenuStripItem;
				}
				else if (e.ToolStrip is StatusStrip && !e.Item.Pressed && !e.Item.Selected)
				{
					e.TextColor = _textStatusStripItem;
				}
				else
				{
					e.TextColor = _textContextMenuItem;
				}
				using (new UseClearTypeGridFit(e.Graphics))
				{
					base.OnRenderItemText(e);
					return;
				}
			}
			base.OnRenderItemText(e);
		}

		protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
		{
			if (e.ToolStrip is ContextMenuStrip || e.ToolStrip is ToolStripDropDownMenu)
			{
				if (e.Image != null)
				{
					if (e.Item.Enabled)
					{
						e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
						e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
						e.Graphics.InterpolationMode = InterpolationMode.High;
						e.Graphics.DrawImage(e.Image, e.ImageRectangle);
					}
					else
					{
						e.Graphics.DrawImage(e.Image, e.ImageRectangle);
					}
				}
			}
			else
			{
				base.OnRenderItemImage(e);
			}
		}

		protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
		{
			if (e.ToolStrip is MenuStrip || e.ToolStrip is ContextMenuStrip || e.ToolStrip is ToolStripDropDownMenu)
			{
				if (e.Item.Pressed && e.ToolStrip is MenuStrip)
				{
					DrawContextMenuHeader(e.Graphics, e.Item);
				}
				else
				{
					if (!e.Item.Selected)
					{
						return;
					}
					if (e.Item.Enabled)
					{
						if (e.ToolStrip is MenuStrip)
						{
							DrawGradientToolItem(e.Graphics, e.Item, _itemToolItemSelectedColors);
						}
						else
						{
							DrawGradientContextMenuItem(e.Graphics, e.Item, _itemContextItemEnabledColors);
						}
						return;
					}
					Point pt = e.ToolStrip.PointToClient(Control.MousePosition);
					if (!e.Item.Bounds.Contains(pt))
					{
						if (e.ToolStrip is MenuStrip)
						{
							DrawGradientToolItem(e.Graphics, e.Item, _itemDisabledColors);
						}
						else
						{
							DrawGradientContextMenuItem(e.Graphics, e.Item, _itemDisabledColors);
						}
					}
				}
			}
			else
			{
				base.OnRenderMenuItemBackground(e);
			}
		}

		protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
		{
			if (e.Item.Selected || e.Item.Pressed)
			{
				ToolStripSplitButton toolStripSplitButton = (ToolStripSplitButton)e.Item;
				RenderToolSplitButtonBackground(e.Graphics, toolStripSplitButton, e.ToolStrip);
				Rectangle dropDownButtonBounds = toolStripSplitButton.DropDownButtonBounds;
				OnRenderArrow(new ToolStripArrowRenderEventArgs(e.Graphics, toolStripSplitButton, dropDownButtonBounds, SystemColors.ControlText, ArrowDirection.Down));
			}
			else
			{
				base.OnRenderSplitButtonBackground(e);
			}
		}

		protected override void OnRenderStatusStripSizingGrip(ToolStripRenderEventArgs e)
		{
			using (SolidBrush darkBrush = new SolidBrush(_gripDark))
			{
				using (SolidBrush lightBrush = new SolidBrush(_gripLight))
				{
					bool flag = e.ToolStrip.RightToLeft == RightToLeft.Yes;
					int num = e.AffectedBounds.Bottom - _gripSize * 2 + 1;
					for (int num2 = _gripLines; num2 >= 1; num2--)
					{
						int num3 = (flag ? (e.AffectedBounds.Left + 1) : (e.AffectedBounds.Right - _gripSize * 2 + 1));
						for (int i = 0; i < num2; i++)
						{
							DrawGripGlyph(e.Graphics, num3, num, darkBrush, lightBrush);
							num3 -= (flag ? (-_gripMove) : _gripMove);
						}
						num -= _gripMove;
					}
				}
			}
		}

		protected override void OnRenderToolStripContentPanelBackground(ToolStripContentPanelRenderEventArgs e)
		{
			base.OnRenderToolStripContentPanelBackground(e);
			if (e.ToolStripContentPanel.Width > 0 && e.ToolStripContentPanel.Height > 0)
			{
				using (LinearGradientBrush brush = new LinearGradientBrush(e.ToolStripContentPanel.ClientRectangle, base.ColorTable.ToolStripContentPanelGradientEnd, base.ColorTable.ToolStripContentPanelGradientBegin, 90f))
				{
					e.Graphics.FillRectangle(brush, e.ToolStripContentPanel.ClientRectangle);
				}
			}
		}

		protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
		{
			if (e.ToolStrip is ContextMenuStrip || e.ToolStrip is ToolStripDropDownMenu)
			{
				using (Pen lightPen = new Pen(_separatorMenuLight))
				{
					using (Pen darkPen = new Pen(_separatorMenuDark))
					{
						DrawSeparator(e.Graphics, e.Vertical, e.Item.Bounds, lightPen, darkPen, _separatorInset, e.ToolStrip.RightToLeft == RightToLeft.Yes);
						return;
					}
				}
			}
			if (e.ToolStrip is StatusStrip)
			{
				using (Pen lightPen2 = new Pen(base.ColorTable.SeparatorLight))
				{
					using (Pen darkPen2 = new Pen(base.ColorTable.SeparatorDark))
					{
						DrawSeparator(e.Graphics, e.Vertical, e.Item.Bounds, lightPen2, darkPen2, 0, rtl: false);
						return;
					}
				}
			}
			base.OnRenderSeparator(e);
		}

		protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
		{
			if (e.ToolStrip is ContextMenuStrip || e.ToolStrip is ToolStripDropDownMenu)
			{
				using (GraphicsPath path2 = CreateBorderPath(e.AffectedBounds, _cutContextMenu))
				{
					using (GraphicsPath path = CreateClipBorderPath(e.AffectedBounds, _cutContextMenu))
					{
						using (new UseClipping(e.Graphics, path))
						{
							using (SolidBrush brush = new SolidBrush(_contextMenuBack))
							{
								e.Graphics.FillPath(brush, path2);
								return;
							}
						}
					}
				}
			}
			if (e.ToolStrip is StatusStrip)
			{
				RectangleF rect = new RectangleF(0f, 1.5f, e.ToolStrip.Width, e.ToolStrip.Height - 2);
				if (rect.Width > 0f && rect.Height > 0f)
				{
					using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, base.ColorTable.StatusStripGradientBegin, base.ColorTable.StatusStripGradientEnd, 90f))
					{
						linearGradientBrush.Blend = _statusStripBlend;
						e.Graphics.FillRectangle(linearGradientBrush, rect);
					}
				}
			}
			else
			{
				base.OnRenderToolStripBackground(e);
			}
		}

		protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
		{
			if (e.ToolStrip is ContextMenuStrip || e.ToolStrip is ToolStripDropDownMenu)
			{
				Rectangle affectedBounds = e.AffectedBounds;
				bool flag = e.ToolStrip.RightToLeft == RightToLeft.Yes;
				affectedBounds.Y += _marginInset;
				affectedBounds.Height -= _marginInset * 2;
				if (!flag)
				{
					affectedBounds.X += _marginInset;
				}
				else
				{
					affectedBounds.X += _marginInset / 2;
				}
				using (SolidBrush brush = new SolidBrush(base.ColorTable.ImageMarginGradientBegin))
				{
					e.Graphics.FillRectangle(brush, affectedBounds);
				}
				using (Pen pen = new Pen(_separatorMenuLight))
				{
					using (Pen pen2 = new Pen(_separatorMenuDark))
					{
						if (!flag)
						{
							e.Graphics.DrawLine(pen, affectedBounds.Right, affectedBounds.Top, affectedBounds.Right, affectedBounds.Bottom);
							e.Graphics.DrawLine(pen2, affectedBounds.Right - 1, affectedBounds.Top, affectedBounds.Right - 1, affectedBounds.Bottom);
						}
						else
						{
							e.Graphics.DrawLine(pen, affectedBounds.Left - 1, affectedBounds.Top, affectedBounds.Left - 1, affectedBounds.Bottom);
							e.Graphics.DrawLine(pen2, affectedBounds.Left, affectedBounds.Top, affectedBounds.Left, affectedBounds.Bottom);
						}
						return;
					}
				}
			}
			base.OnRenderImageMargin(e);
		}

		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
		{
			if (e.ToolStrip is ContextMenuStrip || e.ToolStrip is ToolStripDropDownMenu)
			{
				if (!e.ConnectedArea.IsEmpty)
				{
					using (SolidBrush brush = new SolidBrush(_contextMenuBack))
					{
						e.Graphics.FillRectangle(brush, e.ConnectedArea);
					}
				}
				using (GraphicsPath path3 = CreateBorderPath(e.AffectedBounds, e.ConnectedArea, _cutContextMenu))
				{
					using (GraphicsPath path2 = CreateInsideBorderPath(e.AffectedBounds, e.ConnectedArea, _cutContextMenu))
					{
						using (GraphicsPath path = CreateClipBorderPath(e.AffectedBounds, e.ConnectedArea, _cutContextMenu))
						{
							using (Pen pen2 = new Pen(base.ColorTable.MenuBorder))
							{
								using (Pen pen = new Pen(_separatorMenuLight))
								{
									using (new UseClipping(e.Graphics, path))
									{
										using (new UseAntiAlias(e.Graphics))
										{
											e.Graphics.DrawPath(pen, path2);
											e.Graphics.DrawPath(pen2, path3);
										}
										e.Graphics.DrawLine(pen2, e.AffectedBounds.Right, e.AffectedBounds.Bottom, e.AffectedBounds.Right - 1, e.AffectedBounds.Bottom - 1);
										return;
									}
								}
							}
						}
					}
				}
			}
			if (e.ToolStrip is StatusStrip)
			{
				using (Pen pen3 = new Pen(_statusStripBorderDark))
				{
					using (Pen pen4 = new Pen(_statusStripBorderLight))
					{
						e.Graphics.DrawLine(pen3, 0, 0, e.ToolStrip.Width, 0);
						e.Graphics.DrawLine(pen4, 0, 1, e.ToolStrip.Width, 1);
						return;
					}
				}
			}
			base.OnRenderToolStripBorder(e);
		}

		private void RenderToolButtonBackground(Graphics g, ToolStripButton button, ToolStrip toolstrip)
		{
			if (button.Enabled)
			{
				if (button.Checked)
				{
					if (button.Pressed)
					{
						DrawGradientToolItem(g, button, _itemToolItemPressedColors);
					}
					else if (button.Selected)
					{
						DrawGradientToolItem(g, button, _itemToolItemCheckPressColors);
					}
					else
					{
						DrawGradientToolItem(g, button, _itemToolItemCheckedColors);
					}
				}
				else if (button.Pressed)
				{
					DrawGradientToolItem(g, button, _itemToolItemPressedColors);
				}
				else if (button.Selected)
				{
					DrawGradientToolItem(g, button, _itemToolItemSelectedColors);
				}
			}
			else if (button.Selected)
			{
				Point pt = toolstrip.PointToClient(Control.MousePosition);
				if (!button.Bounds.Contains(pt))
				{
					DrawGradientToolItem(g, button, _itemDisabledColors);
				}
			}
		}

		private void RenderToolDropButtonBackground(Graphics g, ToolStripItem item, ToolStrip toolstrip)
		{
			if (!item.Selected && !item.Pressed)
			{
				return;
			}
			if (item.Enabled)
			{
				if (item.Pressed)
				{
					DrawContextMenuHeader(g, item);
				}
				else
				{
					DrawGradientToolItem(g, item, _itemToolItemSelectedColors);
				}
				return;
			}
			Point pt = toolstrip.PointToClient(Control.MousePosition);
			if (!item.Bounds.Contains(pt))
			{
				DrawGradientToolItem(g, item, _itemDisabledColors);
			}
		}

		private void RenderToolSplitButtonBackground(Graphics g, ToolStripSplitButton splitButton, ToolStrip toolstrip)
		{
			if (!splitButton.Selected && !splitButton.Pressed)
			{
				return;
			}
			if (splitButton.Enabled)
			{
				if (!splitButton.Pressed && splitButton.ButtonPressed)
				{
					DrawGradientToolSplitItem(g, splitButton, _itemToolItemPressedColors, _itemToolItemSelectedColors, _itemContextItemEnabledColors);
				}
				else if (splitButton.Pressed && !splitButton.ButtonPressed)
				{
					DrawContextMenuHeader(g, splitButton);
				}
				else
				{
					DrawGradientToolSplitItem(g, splitButton, _itemToolItemSelectedColors, _itemToolItemSelectedColors, _itemContextItemEnabledColors);
				}
			}
			else
			{
				Point pt = toolstrip.PointToClient(Control.MousePosition);
				if (!splitButton.Bounds.Contains(pt))
				{
					DrawGradientToolItem(g, splitButton, _itemDisabledColors);
				}
			}
		}

		private void DrawGradientToolItem(Graphics g, ToolStripItem item, GradientItemColors colors)
		{
			DrawGradientItem(g, new Rectangle(Point.Empty, item.Bounds.Size), colors);
		}

		private void DrawGradientToolSplitItem(Graphics g, ToolStripSplitButton splitButton, GradientItemColors colorsButton, GradientItemColors colorsDrop, GradientItemColors colorsSplit)
		{
			Rectangle rectangle = new Rectangle(Point.Empty, splitButton.Bounds.Size);
			Rectangle dropDownButtonBounds = splitButton.DropDownButtonBounds;
			if (rectangle.Width <= 0 || dropDownButtonBounds.Width <= 0 || rectangle.Height <= 0 || dropDownButtonBounds.Height <= 0)
			{
				return;
			}
			Rectangle backRect = rectangle;
			int num;
			if (dropDownButtonBounds.X > 0)
			{
				backRect.Width = dropDownButtonBounds.Left;
				dropDownButtonBounds.X--;
				dropDownButtonBounds.Width++;
				num = dropDownButtonBounds.X;
			}
			else
			{
				backRect.Width -= dropDownButtonBounds.Width - 2;
				backRect.X = dropDownButtonBounds.Right - 1;
				dropDownButtonBounds.Width++;
				num = dropDownButtonBounds.Right - 1;
			}
			using (CreateBorderPath(rectangle, _cutMenuItemBack))
			{
				DrawGradientBack(g, backRect, colorsButton);
				DrawGradientBack(g, dropDownButtonBounds, colorsDrop);
				using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new Rectangle(rectangle.X + num, rectangle.Top, 1, rectangle.Height + 1), colorsSplit.Border1, colorsSplit.Border2, 90f))
				{
					linearGradientBrush.SetSigmaBellShape(0.5f);
					using (Pen pen = new Pen(linearGradientBrush))
					{
						g.DrawLine(pen, rectangle.X + num, rectangle.Top + 1, rectangle.X + num, rectangle.Bottom - 1);
					}
				}
				DrawGradientBorder(g, rectangle, colorsButton);
			}
		}

		private void DrawContextMenuHeader(Graphics g, ToolStripItem item)
		{
			Rectangle rect = new Rectangle(Point.Empty, item.Bounds.Size);
			using (GraphicsPath path2 = CreateBorderPath(rect, _cutToolItemMenu))
			{
				using (CreateInsideBorderPath(rect, _cutToolItemMenu))
				{
					using (GraphicsPath path = CreateClipBorderPath(rect, _cutToolItemMenu))
					{
						using (new UseClipping(g, path))
						{
							using (SolidBrush brush = new SolidBrush(_separatorMenuLight))
							{
								g.FillPath(brush, path2);
							}
							using (Pen pen = new Pen(base.ColorTable.MenuBorder))
							{
								g.DrawPath(pen, path2);
							}
						}
					}
				}
			}
		}

		private void DrawGradientContextMenuItem(Graphics g, ToolStripItem item, GradientItemColors colors)
		{
			Rectangle backRect = new Rectangle(2, 0, item.Bounds.Width - 3, item.Bounds.Height);
			DrawGradientItem(g, backRect, colors);
		}

		private void DrawGradientItem(Graphics g, Rectangle backRect, GradientItemColors colors)
		{
			if (backRect.Width > 0 && backRect.Height > 0)
			{
				DrawGradientBack(g, backRect, colors);
				DrawGradientBorder(g, backRect, colors);
			}
		}

		private void DrawGradientBack(Graphics g, Rectangle backRect, GradientItemColors colors)
		{
			backRect.Inflate(-1, -1);
			int num = backRect.Height / 2;
			Rectangle rectangle = new Rectangle(backRect.X, backRect.Y, backRect.Width, num);
			Rectangle rectangle2 = new Rectangle(backRect.X, backRect.Y + num, backRect.Width, backRect.Height - num);
			Rectangle rect = rectangle;
			Rectangle rect2 = rectangle2;
			rect.Inflate(1, 1);
			rect2.Inflate(1, 1);
			using (LinearGradientBrush brush = new LinearGradientBrush(rect, colors.InsideTop1, colors.InsideTop2, 90f))
			{
				using (LinearGradientBrush brush2 = new LinearGradientBrush(rect2, colors.InsideBottom1, colors.InsideBottom2, 90f))
				{
					g.FillRectangle(brush, rectangle);
					g.FillRectangle(brush2, rectangle2);
				}
			}
			num = backRect.Height / 2;
			rectangle = new Rectangle(backRect.X, backRect.Y, backRect.Width, num);
			rectangle2 = new Rectangle(backRect.X, backRect.Y + num, backRect.Width, backRect.Height - num);
			rect = rectangle;
			rect2 = rectangle2;
			rect.Inflate(1, 1);
			rect2.Inflate(1, 1);
			using (LinearGradientBrush brush3 = new LinearGradientBrush(rect, colors.FillTop1, colors.FillTop2, 90f))
			{
				using (LinearGradientBrush brush4 = new LinearGradientBrush(rect2, colors.FillBottom1, colors.FillBottom2, 90f))
				{
					backRect.Inflate(-1, -1);
					num = backRect.Height / 2;
					rectangle = new Rectangle(backRect.X, backRect.Y, backRect.Width, num);
					rectangle2 = new Rectangle(backRect.X, backRect.Y + num, backRect.Width, backRect.Height - num);
					g.FillRectangle(brush3, rectangle);
					g.FillRectangle(brush4, rectangle2);
				}
			}
		}

		private void DrawGradientBorder(Graphics g, Rectangle backRect, GradientItemColors colors)
		{
			using (new UseAntiAlias(g))
			{
				Rectangle rect = backRect;
				rect.Inflate(1, 1);
				using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, colors.Border1, colors.Border2, 90f))
				{
					linearGradientBrush.SetSigmaBellShape(0.5f);
					using (Pen pen = new Pen(linearGradientBrush))
					{
						using (GraphicsPath path = CreateBorderPath(backRect, _cutMenuItemBack))
						{
							g.DrawPath(pen, path);
						}
					}
				}
			}
		}

		private void DrawGripGlyph(Graphics g, int x, int y, Brush darkBrush, Brush lightBrush)
		{
			g.FillRectangle(lightBrush, x + _gripOffset, y + _gripOffset, _gripSquare, _gripSquare);
			g.FillRectangle(darkBrush, x, y, _gripSquare, _gripSquare);
		}

		private void DrawSeparator(Graphics g, bool vertical, Rectangle rect, Pen lightPen, Pen darkPen, int horizontalInset, bool rtl)
		{
			if (vertical)
			{
				int num = rect.Width / 2;
				int y = rect.Y;
				int bottom = rect.Bottom;
				g.DrawLine(darkPen, num, y, num, bottom);
				g.DrawLine(lightPen, num + 1, y, num + 1, bottom);
			}
			else
			{
				int num2 = rect.Height / 2;
				int x = rect.X + ((!rtl) ? horizontalInset : 0);
				int x2 = rect.Right - (rtl ? horizontalInset : 0);
				g.DrawLine(darkPen, x, num2, x2, num2);
				g.DrawLine(lightPen, x, num2 + 1, x2, num2 + 1);
			}
		}

		private GraphicsPath CreateBorderPath(Rectangle rect, Rectangle exclude, float cut)
		{
			if (exclude.IsEmpty)
			{
				return CreateBorderPath(rect, cut);
			}
			rect.Width--;
			rect.Height--;
			List<PointF> list = new List<PointF>();
			float x = rect.X;
			float num = rect.Y;
			float x2 = rect.Right;
			float y = rect.Bottom;
			float num2 = (float)rect.X + cut;
			float num3 = (float)rect.Right - cut;
			float y2 = (float)rect.Y + cut;
			float y3 = (float)rect.Bottom - cut;
			float num4 = ((cut == 0f) ? 1f : cut);
			if (rect.Y >= exclude.Top && rect.Y <= exclude.Bottom)
			{
				float num5 = (float)(exclude.X - 1) - cut;
				float num6 = (float)exclude.Right + cut;
				if (num2 <= num5)
				{
					list.Add(new PointF(num2, num));
					list.Add(new PointF(num5, num));
					list.Add(new PointF(num5 + cut, num - num4));
				}
				else
				{
					num5 = exclude.X - 1;
					list.Add(new PointF(num5, num));
					list.Add(new PointF(num5, num - num4));
				}
				if (num3 > num6)
				{
					list.Add(new PointF(num6 - cut, num - num4));
					list.Add(new PointF(num6, num));
					list.Add(new PointF(num3, num));
				}
				else
				{
					num6 = exclude.Right;
					list.Add(new PointF(num6, num - num4));
					list.Add(new PointF(num6, num));
				}
			}
			else
			{
				list.Add(new PointF(num2, num));
				list.Add(new PointF(num3, num));
			}
			list.Add(new PointF(x2, y2));
			list.Add(new PointF(x2, y3));
			list.Add(new PointF(num3, y));
			list.Add(new PointF(num2, y));
			list.Add(new PointF(x, y3));
			list.Add(new PointF(x, y2));
			GraphicsPath graphicsPath = new GraphicsPath();
			for (int i = 1; i < list.Count; i++)
			{
				graphicsPath.AddLine(list[i - 1], list[i]);
			}
			graphicsPath.AddLine(list[list.Count - 1], list[0]);
			return graphicsPath;
		}

		private GraphicsPath CreateBorderPath(Rectangle rect, float cut)
		{
			rect.Width--;
			rect.Height--;
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddLine((float)rect.Left + cut, rect.Top, (float)rect.Right - cut, rect.Top);
			graphicsPath.AddLine((float)rect.Right - cut, rect.Top, rect.Right, (float)rect.Top + cut);
			graphicsPath.AddLine(rect.Right, (float)rect.Top + cut, rect.Right, (float)rect.Bottom - cut);
			graphicsPath.AddLine(rect.Right, (float)rect.Bottom - cut, (float)rect.Right - cut, rect.Bottom);
			graphicsPath.AddLine((float)rect.Right - cut, rect.Bottom, (float)rect.Left + cut, rect.Bottom);
			graphicsPath.AddLine((float)rect.Left + cut, rect.Bottom, rect.Left, (float)rect.Bottom - cut);
			graphicsPath.AddLine(rect.Left, (float)rect.Bottom - cut, rect.Left, (float)rect.Top + cut);
			graphicsPath.AddLine(rect.Left, (float)rect.Top + cut, (float)rect.Left + cut, rect.Top);
			return graphicsPath;
		}

		private GraphicsPath CreateInsideBorderPath(Rectangle rect, float cut)
		{
			rect.Inflate(-1, -1);
			return CreateBorderPath(rect, cut);
		}

		private GraphicsPath CreateInsideBorderPath(Rectangle rect, Rectangle exclude, float cut)
		{
			rect.Inflate(-1, -1);
			return CreateBorderPath(rect, exclude, cut);
		}

		private GraphicsPath CreateClipBorderPath(Rectangle rect, float cut)
		{
			rect.Width++;
			rect.Height++;
			return CreateBorderPath(rect, cut);
		}

		private GraphicsPath CreateClipBorderPath(Rectangle rect, Rectangle exclude, float cut)
		{
			rect.Width++;
			rect.Height++;
			return CreateBorderPath(rect, exclude, cut);
		}

		private GraphicsPath CreateArrowPath(ToolStripItem item, Rectangle rect, ArrowDirection direction)
		{
			int num;
			int num2;
			if (direction == ArrowDirection.Left || direction == ArrowDirection.Right)
			{
				num = rect.Right - (rect.Width - 4) / 2;
				num2 = rect.Y + rect.Height / 2;
			}
			else
			{
				num = rect.X + rect.Width / 2;
				num2 = rect.Bottom - (rect.Height - 3) / 2;
				if (item is ToolStripDropDownButton && item.RightToLeft == RightToLeft.Yes)
				{
					num++;
				}
			}
			GraphicsPath graphicsPath = new GraphicsPath();
			switch (direction)
			{
			case ArrowDirection.Right:
				graphicsPath.AddLine(num, num2, num - 4, num2 - 4);
				graphicsPath.AddLine(num - 4, num2 - 4, num - 4, num2 + 4);
				graphicsPath.AddLine(num - 4, num2 + 4, num, num2);
				break;
			case ArrowDirection.Left:
				graphicsPath.AddLine(num - 4, num2, num, num2 - 4);
				graphicsPath.AddLine(num, num2 - 4, num, num2 + 4);
				graphicsPath.AddLine(num, num2 + 4, num - 4, num2);
				break;
			case ArrowDirection.Down:
				graphicsPath.AddLine((float)num + 3f, (float)num2 - 3f, (float)num - 2f, (float)num2 - 3f);
				graphicsPath.AddLine((float)num - 2f, (float)num2 - 3f, num, num2);
				graphicsPath.AddLine(num, num2, (float)num + 3f, (float)num2 - 3f);
				break;
			case ArrowDirection.Up:
				graphicsPath.AddLine((float)num + 3f, num2, (float)num - 3f, num2);
				graphicsPath.AddLine((float)num - 3f, num2, num, (float)num2 - 4f);
				graphicsPath.AddLine(num, (float)num2 - 4f, (float)num + 3f, num2);
				break;
			}
			return graphicsPath;
		}

		private GraphicsPath CreateTickPath(Rectangle rect)
		{
			int num = rect.X + rect.Width / 2;
			int num2 = rect.Y + rect.Height / 2;
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddLine(num - 4, num2, num - 2, num2 + 4);
			graphicsPath.AddLine(num - 2, num2 + 4, num + 3, num2 - 5);
			return graphicsPath;
		}

		private GraphicsPath CreateIndeterminatePath(Rectangle rect)
		{
			int num = rect.X + rect.Width / 2;
			int num2 = rect.Y + rect.Height / 2;
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddLine(num - 3, num2, num, num2 - 3);
			graphicsPath.AddLine(num, num2 - 3, num + 3, num2);
			graphicsPath.AddLine(num + 3, num2, num, num2 + 3);
			graphicsPath.AddLine(num, num2 + 3, num - 3, num2);
			return graphicsPath;
		}
	}
}
