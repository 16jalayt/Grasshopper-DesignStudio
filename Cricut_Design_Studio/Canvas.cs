using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

namespace Cricut_Design_Studio
{
	public class Canvas
	{
		public class LayerProperties
		{
			public string layerName;

			public float paperWidth;

			public float paperHeight;

			public bool includeInPreview = true;

			public Color color;

			public object tag;

			public LayerProperties()
			{
				Random random = new Random();
				color = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255));
			}
		}

		public enum InitMode
		{
			INIT_NONE,
			INIT_NORMAL,
			INIT_PRINT,
			INIT_CUT
		}

		private class DraggableSelectionRegion
		{
			protected PointF dragBeginPoint;

			protected PointF dragPoint;

			internal virtual void dragBgn(PointF mpt)
			{
				dragBeginPoint = mpt;
				dragPoint = mpt;
			}

			internal virtual void drag(PointF mpt, Matrix canvasToWorld, ArrayList sceneGroups, bool invertSelection)
			{
				dragPoint = mpt;
			}

			internal virtual void draw(Graphics graphics)
			{
			}

			internal virtual ArrayList groupsInRegion(Matrix canvasToWorld, ArrayList sceneGroups)
			{
				return new ArrayList();
			}

			internal void dragEnd(Matrix canvasToWorld, ArrayList sceneGroups)
			{
				foreach (SceneGroup sceneGroup in sceneGroups)
				{
					sceneGroup.tentativelySelected = false;
				}
			}
		}

		private class DraggableSelectionRect : DraggableSelectionRegion
		{
			private RectangleF RectFromPoints(PointF p1, PointF p2)
			{
				float num = Math.Min(p1.X, p2.X);
				float num2 = Math.Min(p1.Y, p2.Y);
				float width = Math.Max(p1.X, p2.X) - num;
				float height = Math.Max(p1.Y, p2.Y) - num2;
				return new RectangleF(num, num2, width, height);
			}

			private RectangleF DragRect()
			{
				return RectFromPoints(dragBeginPoint, dragPoint);
			}

			private RectangleF DragRectInWorldSpace(Matrix canvasToWorld)
			{
				RectangleF rectangleF = DragRect();
				PointF[] array = new PointF[2]
				{
					new PointF(rectangleF.X, rectangleF.Y),
					new PointF(rectangleF.X + rectangleF.Width, rectangleF.Y + rectangleF.Height)
				};
				canvasToWorld.TransformPoints(array);
				return RectFromPoints(array[0], array[1]);
			}

			internal override void draw(Graphics graphics)
			{
				Pen pen = new Pen(Brushes.SaddleBrown, 1f);
				pen.DashStyle = DashStyle.Dash;
				RectangleF rectangleF = DragRect();
				graphics.DrawRectangle(pen, rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);
				base.draw(graphics);
			}

			private bool InSelectionRegion(RectangleF selectionRect, SceneGroup group)
			{
				PointF[] array = new PointF[4]
				{
					new PointF(group.bbox.Left, group.bbox.Top),
					new PointF(group.bbox.Right, group.bbox.Top),
					new PointF(group.bbox.Left, group.bbox.Bottom),
					new PointF(group.bbox.Right, group.bbox.Bottom)
				};
				group.getTransform().TransformPoints(array);
				PointF[] array2 = array;
				foreach (PointF pt in array2)
				{
					if (!selectionRect.Contains(pt))
					{
						return false;
					}
				}
				return true;
			}

			internal override void drag(PointF mpt, Matrix canvasToWorld, ArrayList sceneGroups, bool invertSelection)
			{
				base.drag(mpt, canvasToWorld, sceneGroups, invertSelection);
				RectangleF selectionRect = DragRectInWorldSpace(canvasToWorld);
				foreach (SceneGroup sceneGroup in sceneGroups)
				{
					if (InSelectionRegion(selectionRect, sceneGroup))
					{
						if (invertSelection)
						{
							sceneGroup.tentativelySelected = !sceneGroup.selected;
						}
						else
						{
							sceneGroup.tentativelySelected = true;
						}
					}
					else
					{
						sceneGroup.tentativelySelected = false;
					}
				}
			}

			internal override ArrayList groupsInRegion(Matrix canvasToWorld, ArrayList sceneGroups)
			{
				RectangleF selectionRect = DragRectInWorldSpace(canvasToWorld);
				ArrayList arrayList = new ArrayList();
				foreach (SceneGroup sceneGroup in sceneGroups)
				{
					if (InSelectionRegion(selectionRect, sceneGroup))
					{
						arrayList.Add(sceneGroup);
					}
				}
				return arrayList;
			}
		}

		private const int GlyphKeyHeight = 800;

		public const int DRAW_BKGROUND = 1;

		public const int DRAW_SHAPES = 2;

		public const int DRAW_IMAGES = 4;

		public const int DRAW_CURSOR = 8;

		public const int DRAW_HANDLES = 16;

		public const int DRAW_RASTER = 32;

		public const int DRAW_ALL = 31;

		public UndoRedo undoRedo;

		public LayerProperties layerProperties = new LayerProperties();

		public PictureBox canvasPicBox;

		public static Bitmap previewBitmap = null;

		public static int previewFreshness = 0;

		public float matteWidth;

		public float matteHeight;

		public RectangleF drawR = new RectangleF(0f, 0f, 1f, 1f);

		public Matrix worldToCanvas = new Matrix();

		public Matrix canvasToWorld = new Matrix();

		public static float penScale;

		public SceneGroup selectedGroup;

		public bool dirty;

		private Rectangle canvasR = new Rectangle(0, 0, 1, 1);

		public InitMode initMode;

		public object[] initNormalParams = new object[5];

		public object[] initPrintParams = new object[3];

		public object[] initCutParams = new object[2];

		private Color matBkgndColor = Color.FromArgb(255, 213, 228, 207);

		private Color matLightColor = Color.FromArgb(255, 242, 245, 241);

		private Color matDarkLineColor = Color.FromArgb(255, 126, 156, 116);

		public static int drawMode = 31;

		public bool previewRenderMode;

		private int dragHandleTag = -1;

		private DraggableSelectionRegion draggableSelectionRegion;

		public float cursorX = 0.125f;

		public float cursorY = 1f;

		public float cursorSize = 1f;

		public ArrayList sceneGroups = new ArrayList();

		public ArrayList images = new ArrayList();

		public void init(float matteWidth, float matteHeight, float drawWidth, float drawHeight, float zoom)
		{
			initMode = InitMode.INIT_NORMAL;
			initNormalParams[0] = matteWidth;
			initNormalParams[1] = matteHeight;
			initNormalParams[2] = drawWidth;
			initNormalParams[3] = drawHeight;
			initNormalParams[4] = zoom;
			float x = (matteWidth - drawWidth) / 2f;
			float y = (matteHeight - drawHeight) / 2f;
			this.matteWidth = matteWidth;
			this.matteHeight = matteHeight;
			drawR = new RectangleF(x, y, drawWidth, drawHeight);
			_ = (float)canvasPicBox.Width / (float)canvasPicBox.Height;
			_ = matteWidth / matteHeight;
			canvasR.Width = canvasPicBox.Width;
			canvasR.Height = canvasPicBox.Height;
			canvasR.Location = new Point(0, 0);
			canvasR.Location = new Point(0, 0);
			canvasPicBox.Location = canvasR.Location;
			canvasPicBox.Size = canvasR.Size;
			canvasPicBox.BackColor = Color.FromArgb(255, 213, 228, 207);
			canvasPicBox.AllowDrop = true;
			float num = (float)(canvasR.Width - 1) / matteWidth;
			float num2 = (float)(canvasR.Height - 1) / matteHeight;
			worldToCanvas.Reset();
			worldToCanvas.Scale(num, num2, MatrixOrder.Prepend);
			worldToCanvas.Translate(0f, 0f, MatrixOrder.Prepend);
			canvasToWorld = worldToCanvas.Clone();
			canvasToWorld.Invert();
			penScale = 1f / (float)Math.Sqrt(num * num + num2 * num2);
			canvasPicBox.Focus();
		}

		public void printInit(Graphics pg, float drawWidth, float drawHeight)
		{
			initMode = InitMode.INIT_PRINT;
			initPrintParams[0] = pg;
			initPrintParams[1] = drawWidth;
			initPrintParams[2] = drawHeight;
			float num = pg.VisibleClipBounds.Width / 100f;
			float num2 = pg.VisibleClipBounds.Height / 100f;
			Math.Min(drawWidth, num);
			Math.Min(drawHeight, num2);
			float num3 = pg.VisibleClipBounds.Width / num;
			float num4 = pg.VisibleClipBounds.Height / num2;
			worldToCanvas.Reset();
			worldToCanvas.Scale(num3, num4, MatrixOrder.Prepend);
			worldToCanvas.Translate(0f - drawR.Location.X, 0f - drawR.Location.Y, MatrixOrder.Prepend);
			canvasToWorld = worldToCanvas.Clone();
			canvasToWorld.Invert();
			penScale = 1f / (float)Math.Sqrt(num3 * num4);
		}

		public void cutInit(int ox, int oy)
		{
			initMode = InitMode.INIT_CUT;
			initCutParams[0] = ox;
			initCutParams[1] = oy;
			worldToCanvas.Reset();
			worldToCanvas.Translate(-ox, -oy, MatrixOrder.Prepend);
			worldToCanvas.Scale(404f, 404f, MatrixOrder.Prepend);
			canvasToWorld = worldToCanvas.Clone();
			canvasToWorld.Invert();
		}

		public bool isDirty()
		{
			int num = 0;
			bool result = dirty;
			foreach (SceneGroup sceneGroup in sceneGroups)
			{
				num++;
				if (sceneGroup.dirty)
				{
					result = true;
					break;
				}
			}
			if (num == 0)
			{
				dirty = false;
				result = false;
			}
			return result;
		}

		public void clean()
		{
			foreach (SceneGroup sceneGroup in sceneGroups)
			{
				sceneGroup.dirty = false;
			}
			dirty = false;
		}

		public bool cut(PcControl pc)
		{
			Matrix matrix = new Matrix();
			matrix.Multiply(worldToCanvas, MatrixOrder.Prepend);
			Clipper clipper = Shape.openClipper();
			foreach (SceneGroup sceneGroup in sceneGroups)
			{
				sceneGroup.cut(pc, matrix);
			}
			clipper.cut(pc, out var nerrors);
			Shape.closeClipper();
			if (nerrors > 0)
			{
				return false;
			}
			return true;
		}

		public Canvas(PictureBox pb, int sizeId)
		{
			canvasPicBox = pb;
			canvasPicBox.Paint += canvasPicBox_Paint;
			canvasPicBox.DragEnter += canvasPicBox_DragEnter;
			canvasPicBox.DragDrop += canvasPicBox_DragDrop;
			setupSize(canvasPicBox, sizeId);
			Form1.myRootForm.refreshMattePicBox();
			undoRedo = new UndoRedo(this);
		}

		public void setupSize(PictureBox pb, int sizeId)
		{
			switch (sizeId)
			{
			case 0:
				layerProperties.paperWidth = 12f;
				layerProperties.paperHeight = 6f;
				init(14f, 7f, 12f, 6f, 1f);
				break;
			case 1:
				layerProperties.paperWidth = 12f;
				layerProperties.paperHeight = 12f;
				init(14f, 13f, 12f, 12f, 1f);
				break;
			case 2:
				layerProperties.paperWidth = 24f;
				layerProperties.paperHeight = 12f;
				init(26f, 13f, 24f, 12f, 1f);
				break;
			}
		}

		public static void fillRoundBox(Graphics g, Color col, float ox, float oy, float cx, float cy)
		{
			float num = 0.5f;
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddArc(ox, oy, num, num, 180f, 90f);
			graphicsPath.AddArc(cx - num, oy, num, num, 270f, 90f);
			graphicsPath.AddArc(cx - num, cy - num, num, num, 0f, 90f);
			graphicsPath.AddArc(ox, cy - num, num, num, 90f, 90f);
			SolidBrush brush = new SolidBrush(col);
			g.FillPath(brush, graphicsPath);
		}

		public static void drawRoundBox(Graphics g, Color col, float ox, float oy, float cx, float cy)
		{
			float num = 0.5f;
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddLine(ox, oy + num, ox, (oy + cy) / 2f);
			graphicsPath.AddArc(ox, oy, num, num, 180f, 90f);
			graphicsPath.AddArc(cx - num, oy, num, num, 270f, 90f);
			graphicsPath.AddArc(cx - num, cy - num, num, num, 0f, 90f);
			graphicsPath.AddArc(ox, cy - num, num, num, 90f, 90f);
			graphicsPath.AddLine(ox, (oy + cy) / 2f, ox, cy - num);
			_ = SystemColors.Control.R * 14 / 16;
			_ = SystemColors.Control.G * 14 / 16;
			_ = SystemColors.Control.B * 14 / 16;
			Pen pen = new Pen(col, penScale);
			g.DrawPath(pen, graphicsPath);
		}

		private void drawRuler(Graphics g, float ox, float oy, float cx, float cy, float corner, int d)
		{
			PointF[] array = new PointF[8]
			{
				new PointF(ox, oy),
				new PointF(ox, oy),
				new PointF(ox, cy),
				new PointF(ox, cy),
				new PointF(cx, cy),
				new PointF(cx, cy),
				new PointF(cx, oy),
				new PointF(cx, oy)
			};
			switch (d)
			{
			case 0:
				array[2].Y += corner;
				array[3].X += corner;
				array[4].X -= corner;
				array[5].Y += corner;
				break;
			case 1:
				array[4].X += corner;
				array[5].Y += corner;
				array[6].Y -= corner;
				array[7].X += corner;
				break;
			case 2:
				array[2].Y -= corner;
				array[3].X -= corner;
				array[4].X += corner;
				array[5].Y -= corner;
				break;
			case 3:
				array[4].X -= corner;
				array[5].Y -= corner;
				array[6].Y += corner;
				array[7].X -= corner;
				break;
			}
			Pen pen = new Pen(matDarkLineColor, penScale * 0.1f);
			Pen pen2 = new Pen(matBkgndColor, penScale * 0.1f);
			Brush brush = new SolidBrush(matLightColor);
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddPolygon(array);
			g.FillPath(brush, graphicsPath);
			Font font = new Font("Arial", 0.125f);
			StringFormat stringFormat = new StringFormat();
			brush = new SolidBrush(matDarkLineColor);
			g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			switch (d)
			{
			case 0:
			{
				stringFormat.Alignment = StringAlignment.Center;
				stringFormat.LineAlignment = StringAlignment.Far;
				for (float num5 = ox; num5 <= cx; num5 += 0.25f)
				{
					if (num5 > ox + float.Epsilon && num5 < cx - float.Epsilon)
					{
						g.DrawLine(pen2, num5, oy - 0.03f, num5, cy + 0.03f);
					}
				}
				for (float num6 = ox; num6 <= cx; num6 += 1f)
				{
					if (num6 > ox + float.Epsilon && num6 < cx - float.Epsilon)
					{
						g.DrawLine(pen, num6, oy - 0.03f, num6, cy + 0.03f);
					}
					if (num6 > ox + float.Epsilon)
					{
						RectangleF layoutRectangle3 = new RectangleF(num6 - 0.25f, cy - 0.25f, 0.5f, 0.25f);
						string s3 = ((int)Math.Round(num6 - 1f)).ToString();
						g.DrawString(s3, font, brush, layoutRectangle3, stringFormat);
					}
				}
				break;
			}
			case 1:
			{
				stringFormat.Alignment = StringAlignment.Far;
				stringFormat.LineAlignment = StringAlignment.Center;
				for (float num7 = cy; num7 <= oy; num7 += 0.25f)
				{
					if (num7 > cy + float.Epsilon && num7 < oy - float.Epsilon)
					{
						g.DrawLine(pen2, cx + 0.03f, num7, ox - 0.03f, num7);
					}
				}
				for (float num8 = cy; num8 <= oy; num8 += 1f)
				{
					if (num8 > cy + float.Epsilon && num8 < oy - float.Epsilon)
					{
						g.DrawLine(pen, cx + 0.03f, num8, ox - 0.03f, num8);
					}
					if (num8 > cy + float.Epsilon)
					{
						RectangleF layoutRectangle4 = new RectangleF(cx - 0.5f, num8 - 0.125f, 0.5f, 0.25f);
						string s4 = ((int)Math.Round(num8 - 0.5f)).ToString();
						g.DrawString(s4, font, brush, layoutRectangle4, stringFormat);
					}
				}
				break;
			}
			case 2:
			{
				stringFormat.Alignment = StringAlignment.Center;
				stringFormat.LineAlignment = StringAlignment.Near;
				for (float num3 = cx; num3 <= ox; num3 += 0.25f)
				{
					if (num3 > cx + float.Epsilon && num3 < ox - float.Epsilon)
					{
						g.DrawLine(pen2, num3, oy + 0.03f, num3, cy - 0.03f);
					}
				}
				for (float num4 = cx; num4 <= ox; num4 += 1f)
				{
					if (num4 > cx + float.Epsilon && num4 < ox - float.Epsilon)
					{
						g.DrawLine(pen, num4, oy + 0.03f, num4, cy - 0.03f);
					}
					if (num4 > cx + float.Epsilon)
					{
						RectangleF layoutRectangle2 = new RectangleF(num4 - 0.25f, cy, 0.5f, 0.25f);
						string s2 = ((int)Math.Round(num4 - 1f)).ToString();
						g.DrawString(s2, font, brush, layoutRectangle2, stringFormat);
					}
				}
				break;
			}
			case 3:
			{
				stringFormat.Alignment = StringAlignment.Near;
				stringFormat.LineAlignment = StringAlignment.Center;
				for (float num = oy; num <= cy; num += 0.25f)
				{
					if (num > oy + float.Epsilon && num < cy - float.Epsilon)
					{
						g.DrawLine(pen2, cx - 0.03f, num, ox + 0.03f, num);
					}
				}
				for (float num2 = oy; num2 <= cy; num2 += 1f)
				{
					if (num2 > oy + float.Epsilon && num2 < cy - float.Epsilon)
					{
						g.DrawLine(pen, cx - 0.03f, num2, ox + 0.03f, num2);
					}
					if (num2 > oy + float.Epsilon)
					{
						RectangleF layoutRectangle = new RectangleF(cx, num2 - 0.125f, 0.5f, 0.25f);
						string s = ((int)Math.Round(num2 - 0.5f)).ToString();
						g.DrawString(s, font, brush, layoutRectangle, stringFormat);
					}
				}
				break;
			}
			}
		}

		public void drawRulers(Graphics g)
		{
			float num = 0.1875f;
			float corner = num / 4f;
			drawRuler(g, drawR.Location.X, drawR.Location.Y, drawR.Location.X + drawR.Width, drawR.Location.Y - num, corner, 0);
			drawRuler(g, drawR.Location.X, drawR.Location.Y + drawR.Height, drawR.Location.X - num, drawR.Location.Y, corner, 1);
			drawRuler(g, drawR.Location.X + drawR.Width, drawR.Location.Y + drawR.Height, drawR.Location.X, drawR.Location.Y + drawR.Height + num, corner, 2);
			drawRuler(g, drawR.Location.X + drawR.Width, drawR.Location.Y, drawR.Location.X + drawR.Width + num, drawR.Location.Y + drawR.Height, corner, 3);
		}

		public void drawBackground(Graphics g)
		{
			Color color = matBkgndColor;
			SolidBrush solidBrush = new SolidBrush(color);
			Matrix matrix = new Matrix();
			g.Clear(Color.FromArgb(255, 248, 219));
			matrix.Reset();
			matrix.Multiply(worldToCanvas, MatrixOrder.Append);
			g.Transform = matrix;
			fillRoundBox(g, color, 0f, 0f, matteWidth, matteHeight);
			drawRoundBox(g, Color.Black, 0f, 0f, matteWidth, matteHeight);
			solidBrush.Color = matLightColor;
			Pen pen = new Pen(matLightColor, penScale);
			g.FillRectangle(solidBrush, drawR.Location.X, drawR.Location.Y, drawR.Width, drawR.Height);
			solidBrush.Color = matBkgndColor;
			pen = new Pen(matBkgndColor, penScale);
			for (int i = 1; i < (int)Math.Floor(drawR.Right - float.Epsilon); i++)
			{
				g.DrawLine(pen, drawR.Left + (float)i, drawR.Top, drawR.Left + (float)i, drawR.Bottom);
			}
			for (int j = 1; j < (int)Math.Floor(drawR.Bottom - float.Epsilon); j++)
			{
				g.DrawLine(pen, drawR.Left, drawR.Top + (float)j, drawR.Right, drawR.Top + (float)j);
			}
			if (Form1.myRootForm.showRuler)
			{
				drawRulers(g);
			}
			solidBrush.Color = matDarkLineColor;
			pen = new Pen(matDarkLineColor, penScale);
			g.DrawRectangle(pen, drawR.Location.X, drawR.Location.Y, drawR.Width, drawR.Height);
			g.DrawLine(pen, (drawR.Left + drawR.Right) / 2f, drawR.Top, (drawR.Left + drawR.Right) / 2f, drawR.Bottom);
			g.DrawLine(pen, drawR.Left, (drawR.Top + drawR.Bottom) / 2f, drawR.Right, (drawR.Top + drawR.Bottom) / 2f);
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			pen = new Pen(matDarkLineColor, penScale);
			pen.DashStyle = DashStyle.Dot;
			switch (Form1.myRootForm.matSize)
			{
			case 0:
				num = 0.1875f;
				num2 = 0.25f;
				num3 = 0.125f;
				num4 = 0.1875f;
				break;
			case 1:
			case 2:
				num = 0.3125f;
				num2 = 0.125f;
				num3 = 0.25f;
				num4 = 0.375f;
				break;
			}
			g.DrawRectangle(pen, drawR.Location.X + num, drawR.Location.Y + num2, drawR.Width - (num + num3), drawR.Height - (num2 + num4));
			pen.DashStyle = DashStyle.Solid;
			PointF[] array = new PointF[3]
			{
				new PointF(drawR.Right - 0.5f, drawR.Bottom),
				new PointF(drawR.Right, drawR.Bottom),
				new PointF(drawR.Right, drawR.Bottom - 0.5f)
			};
			g.FillPolygon(solidBrush, array);
			array[0].X = drawR.Left - 0.75f;
			array[0].Y = (drawR.Top + drawR.Bottom) / 2f;
			array[1].X = drawR.Left - 0.5f;
			array[1].Y = (drawR.Top + drawR.Bottom) / 2f + 0.5f;
			array[2].X = drawR.Left - 0.5f;
			array[2].Y = (drawR.Top + drawR.Bottom) / 2f - 0.5f;
			g.FillPolygon(solidBrush, array);
			float num5 = 24f;
			float num6 = 12f;
			switch (Form1.myRootForm.matSize)
			{
			case 0:
				num5 = 12f;
				num6 = 6f;
				break;
			case 1:
				num5 = 12f;
				num6 = 12f;
				break;
			case 2:
				num5 = 24f;
				num6 = 12f;
				break;
			}
			float num7 = ((layerProperties.paperWidth > num5) ? num5 : layerProperties.paperWidth);
			float num8 = ((layerProperties.paperHeight > num6) ? num6 : layerProperties.paperHeight);
			if (num7 < num5 || num8 < num6)
			{
				solidBrush.Color = Color.FromArgb(64, 255, 210, 190);
				g.FillRectangle(solidBrush, drawR.Right - num7, drawR.Bottom - num8, num7, num8);
				pen.Color = Color.FromArgb(192, 255, 200, 180);
				g.DrawRectangle(pen, drawR.Right - num7, drawR.Bottom - num8, num7, num8);
			}
			if (previewBitmap != null)
			{
				Matrix matrix3 = (g.Transform = new Matrix());
				g.DrawImageUnscaled(previewBitmap, 0, 0);
			}
		}

		public void drawCursor(Graphics g)
		{
			Matrix matrix = new Matrix();
			matrix.Reset();
			matrix.Multiply(worldToCanvas, MatrixOrder.Append);
			g.Transform = matrix;
			Pen pen = new Pen(Color.DarkCyan, penScale);
			g.DrawLine(pen, cursorX, cursorY + 0.125f, cursorX, cursorY - cursorSize);
			g.DrawLine(pen, cursorX - 0.125f, cursorY, cursorX + 0.125f, cursorY);
			g.DrawLine(pen, cursorX - 0.125f, cursorY - cursorSize, cursorX + 0.125f, cursorY - cursorSize);
		}

		public void setDrawMode(int mode)
		{
			drawMode = mode;
		}

		public void draw(Graphics g)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.CompositingQuality = CompositingQuality.HighQuality;
			if (((uint)drawMode & 0x20u) != 0)
			{
				if (((uint)drawMode & 0x10u) != 0)
				{
					drawMode &= -17;
					if (((uint)drawMode & 4u) != 0)
					{
						drawImages(g);
					}
					if (((uint)drawMode & 2u) != 0)
					{
						drawGroups(g);
					}
					drawMode |= 16;
					drawMode &= -33;
					if (((uint)drawMode & 2u) != 0)
					{
						drawGroups(g);
					}
					drawMode |= 32;
				}
				else
				{
					if (((uint)drawMode & 4u) != 0)
					{
						drawImages(g);
					}
					if (((uint)drawMode & 2u) != 0)
					{
						drawGroups(g);
					}
				}
			}
			else
			{
				selectedGroup = null;
				if (((uint)drawMode & (true ? 1u : 0u)) != 0)
				{
					drawBackground(g);
				}
				if (((uint)drawMode & 4u) != 0)
				{
					drawImages(g);
				}
				if (((uint)drawMode & 2u) != 0)
				{
					selectedGroup = drawGroups(g);
				}
				if (((uint)drawMode & 8u) != 0 && selectedGroup == null)
				{
					drawCursor(g);
				}
			}
			if (draggableSelectionRegion != null)
			{
				Matrix matrix2 = (g.Transform = new Matrix());
				draggableSelectionRegion.draw(g);
			}
		}

		private void canvasPicBox_Paint(object sender, PaintEventArgs e)
		{
			if (!Form1.myRootForm.cricutCutButton.Enabled)
			{
				e.Graphics.Clear(Color.White);
				return;
			}
			switch (initMode)
			{
			case InitMode.INIT_PRINT:
			case InitMode.INIT_CUT:
				init((float)initNormalParams[0], (float)initNormalParams[1], (float)initNormalParams[2], (float)initNormalParams[3], (float)initNormalParams[4]);
				break;
			}
			if (previewRenderMode)
			{
				e.Graphics.Clear(Color.White);
				setDrawMode(36);
				draw(e.Graphics);
				setDrawMode(31);
				previewRenderMode = false;
			}
			else
			{
				draw(e.Graphics);
			}
			switch (initMode)
			{
			case InitMode.INIT_PRINT:
				printInit((Graphics)initPrintParams[0], (float)initPrintParams[1], (float)initPrintParams[2]);
				break;
			case InitMode.INIT_CUT:
				cutInit((int)initCutParams[0], (int)initCutParams[1]);
				break;
			}
			if (selectedGroup == null)
			{
				Form1.myRootForm.EnableMenuItemsForGroupSelected(enable: false);
			}
			else
			{
				Form1.myRootForm.EnableMenuItemsForGroupSelected(enable: true);
			}
			if (SceneGroup.copyBuffer == null)
			{
				Form1.myRootForm.pasteMenuItem.Enabled = false;
			}
			else
			{
				Form1.myRootForm.pasteMenuItem.Enabled = true;
			}
		}

		public void select(PointF mpnt, bool clearPreviousSelections)
		{
			PointF[] array = new PointF[1] { mpnt };
			canvasToWorld.TransformPoints(array);
			Matrix t = new Matrix();
			bool flag = false;
			SceneGlyph.selectedGlyph = null;
			Form1.myRootForm.EnableGlyphManipulationControls(en: false);
			if (clearPreviousSelections)
			{
				deselectAll();
			}
			bool flag2 = false;
			foreach (SceneGroup sceneGroup in sceneGroups)
			{
				if (!sceneGroup.wouldSelect(array[0], t))
				{
					continue;
				}
				if (sceneGroup.selected && !clearPreviousSelections)
				{
					if (sceneGroup.lastSelected)
					{
						flag2 = true;
					}
					sceneGroup.selected = false;
				}
				else if (sceneGroup.select(array[0], t))
				{
					flag = true;
					cursorX = sceneGroup.bbox.Right;
					cursorY = sceneGroup.baseline;
				}
			}
			if (flag2)
			{
				ResetSelectionControls();
			}
			if (!flag)
			{
				SceneGroup.clearNumBoxes();
			}
		}

		public void dragBgn(PointF mpnt)
		{
			PointF[] array = new PointF[1] { mpnt };
			canvasToWorld.TransformPoints(array);
			SceneGroup.mousePnt = mpnt;
			SceneGroup.wmousePnt = array[0];
			SceneGroup.dragging = true;
			dragHandleTag = -1;
			SceneUtils.dragHandle = dragHandleTag;
			SceneGroup[] selectedGroups = GetSelectedGroups();
			if (selectedGroups.Length > 0)
			{
				SceneGroup[] array2 = selectedGroups;
				foreach (SceneGroup sceneGroup in array2)
				{
					sceneGroup.saveTransform();
				}
				SceneGroup[] psg = SceneGroup.CopySceneGroups(selectedGroups);
				Form1.myRootForm.getCanvas().undoRedo.add(new UndoRedo.UndoTransformShapes(psg, selectedGroups));
			}
			GetSelectedImages();
			foreach (PCImage image in images)
			{
				if (image.selected)
				{
					image.oldX = image.positionX.f;
					image.oldY = image.positionY.f;
					image.oldWidth = image.width.f;
					image.oldHeight = image.height.f;
				}
			}
			if (dragHandleTag < 0 && SceneUtils.thisHandle == null)
			{
				draggableSelectionRegion = new DraggableSelectionRect();
				draggableSelectionRegion.dragBgn(mpnt);
			}
		}

		public void drag(PointF mpnt, SceneGroup.DragRelativeTo dragRelativeTo)
		{
			if (-1 == dragHandleTag && SceneUtils.thisHandle != null)
			{
				draggableSelectionRegion = null;
				dragHandleTag = SceneUtils.thisHandle.tag;
			}
			if (draggableSelectionRegion != null)
			{
				draggableSelectionRegion.drag(mpnt, canvasToWorld, sceneGroups, Control.ModifierKeys == Keys.Control);
			}
			float hratio = (float)(canvasR.Width - 1) / matteWidth;
			float vratio = (float)(canvasR.Height - 1) / matteHeight;
			bool flag = false;
			foreach (SceneGroup sceneGroup in sceneGroups)
			{
				if (sceneGroup.selected)
				{
					SceneUtils.dragHandle = dragHandleTag;
					sceneGroup.transformGroup(selectedGroup, dragRelativeTo, dragHandleTag, hratio, vratio, canvasToWorld, SceneGroup.mousePnt, mpnt);
				}
			}
			if (flag)
			{
				return;
			}
			foreach (PCImage image in images)
			{
				if (image.selected)
				{
					image.transform(dragHandleTag, hratio, vratio, canvasToWorld, SceneGroup.mousePnt, mpnt);
				}
			}
		}

		public void dragEnd()
		{
			if (draggableSelectionRegion != null)
			{
				UndoRedo.UndoSelect undoSelect = new UndoRedo.UndoSelect(this);
				ArrayList arrayList = draggableSelectionRegion.groupsInRegion(canvasToWorld, sceneGroups);
				if (Control.ModifierKeys != Keys.Control && Control.ModifierKeys != Keys.Shift)
				{
					deselectAll();
				}
				bool flag = false;
				foreach (SceneGroup item in arrayList)
				{
					if (Control.ModifierKeys == Keys.Control)
					{
						if (item.lastSelected)
						{
							flag = true;
						}
						item.selected = !item.selected;
					}
					else
					{
						item.selected = true;
					}
				}
				if (flag)
				{
					ResetSelectionControls();
				}
				undoSelect.SaveNewSelections(this);
				undoRedo.add(undoSelect);
				draggableSelectionRegion.dragEnd(canvasToWorld, sceneGroups);
			}
			draggableSelectionRegion = null;
			SceneGroup.dragging = false;
			SceneUtils.transforming = false;
			dragHandleTag = -1;
			SceneUtils.dragHandle = dragHandleTag;
		}

		private void ResetSelectionControls()
		{
			SceneGroup sceneGroup = null;
			ulong num = 0uL;
			foreach (SceneGroup sceneGroup2 in sceneGroups)
			{
				if (sceneGroup2.selectedCount > num)
				{
					sceneGroup = sceneGroup2;
					num = sceneGroup.selectedCount;
				}
			}
			if (sceneGroup != null)
			{
				sceneGroup.selected = true;
			}
		}

		public void setCursor(float x, float y, float size)
		{
			setCursor(x, y, size, ignoreMaxX: false);
		}

		public void setCursor(float x, float y, float size, bool ignoreMaxX)
		{
			float num = 0.25f;
			float num2 = 0.25f;
			float num3 = 11.75f;
			float num4 = 5.75f;
			switch (Form1.myRootForm.matSize)
			{
			case 0:
				num3 = 11.75f;
				num4 = 5.75f;
				break;
			case 1:
				num3 = 11.75f;
				num4 = 11.75f;
				break;
			case 2:
				num3 = 23.75f;
				num4 = 11.75f;
				break;
			}
			if (size > num4 - num2)
			{
				size = num4 - num2;
				Form1.myRootForm.sizeValue = size;
			}
			if (x < num)
			{
				x = num;
			}
			if (!ignoreMaxX && x > num3)
			{
				x = num3;
			}
			if (y > num4)
			{
				y = num4;
			}
			if (y < num2 + size)
			{
				y = num2 + size;
			}
			cursorX = drawR.Location.X + x;
			cursorY = drawR.Location.Y + y;
			cursorSize = size;
		}

		public void adjCursor(float dx, float dy, float size)
		{
			setCursor(cursorX - drawR.Location.X + dx, cursorY - drawR.Location.Y + dy, size, ignoreMaxX: true);
		}

		public void getCursor(out float x, out float y, out float maxX, out float maxY)
		{
			x = cursorX - drawR.Location.X;
			y = cursorY - drawR.Location.Y;
			switch (Form1.myRootForm.matSize)
			{
			case 1:
				maxX = 11.75f;
				maxY = 11.75f;
				break;
			case 2:
				maxX = 23.75f;
				maxY = 11.75f;
				break;
			default:
				maxX = 11.75f;
				maxY = 5.75f;
				break;
			}
		}

		public SceneGroup newGroup()
		{
			deselectAll();
			SceneGroup sceneGroup = new SceneGroup();
			sceneGroup.selected = true;
			sceneGroups.Add(sceneGroup);
			return sceneGroup;
		}

		public void delGlyph(SceneGroup group)
		{
			if (group != null && group.children.Count != 0)
			{
				SceneGlyph sceneGlyph = (SceneGlyph)group.children[group.children.Count - 1];
				setCursor(sceneGlyph.ox - drawR.Location.X, sceneGlyph.oy - drawR.Location.Y, Form1.myRootForm.sizeValue, ignoreMaxX: true);
				_ = (SceneGlyph)group.delLast();
			}
		}

		public void delGlyphAndAddUndo(SceneGroup group)
		{
			if (group != null && group.children.Count != 0)
			{
				SceneGlyph sceneGlyph = (SceneGlyph)group.children[group.children.Count - 1];
				Form1.myRootForm.getCanvas().undoRedo.addAndDo(new UndoRedo.UndoDelGlyph(group, Form1.myRootForm.fontLoading, sceneGlyph.keyId, sceneGlyph.size, sceneGlyph.ox, sceneGlyph.oy, drawR.Location.X, drawR.Location.Y));
			}
		}

		public void addSpace(float size, SceneGroup group)
		{
			if (group != null)
			{
				SceneGlyph child = new SceneGlyph(cursorX, cursorY, size);
				group.add(child, cursorY);
			}
		}

		public void addGlyph(Shape shape, int fontId, int keyId, float ox, float oy, float size, SceneGroup group)
		{
			SceneGlyph child = (SceneGlyph.selectedGlyph = new SceneGlyph(shape, fontId, keyId, ox, oy, size));
			Form1.myRootForm.EnableGlyphManipulationControls(en: true);
			group.add(child, cursorY);
		}

		public void removeGroup(SceneGroup sg)
		{
			dirty = true;
			sceneGroups.Remove(sg);
		}

		public void removeImage(PCImage img)
		{
			dirty = true;
			images.Remove(img);
		}

		public void removeAll()
		{
			undoRedo.clear();
			while (sceneGroups.Count > 0)
			{
				dirty = true;
				sceneGroups.RemoveAt(sceneGroups.Count - 1);
			}
			while (images.Count > 0)
			{
				dirty = true;
				images.RemoveAt(images.Count - 1);
			}
		}

		public static Matrix glyphToWorld(float locX, float locY, float size)
		{
			Matrix matrix = new Matrix();
			matrix.Translate(locX, locY, MatrixOrder.Prepend);
			matrix.Scale(size / 800f, size / 800f, MatrixOrder.Prepend);
			return matrix;
		}

		public static void clearPreview()
		{
			if (previewBitmap != null)
			{
				previewBitmap.Dispose();
				previewBitmap = null;
			}
		}

		public Bitmap drawPreview(Bitmap image, bool drawBkgnd)
		{
			Graphics graphics = null;
			if (image == null)
			{
				image = new Bitmap(canvasPicBox.Width, canvasPicBox.Height, PixelFormat.Format32bppArgb);
				graphics = Graphics.FromImage(image);
				if (drawBkgnd)
				{
					drawBackground(graphics);
				}
				else
				{
					graphics.Clear(Color.FromArgb(0, 0, 0, 0));
				}
			}
			else
			{
				graphics = Graphics.FromImage(image);
			}
			Clipper clipper = Shape.openClipper();
			Clipper.testG = Graphics.FromImage(image);
			setDrawMode(38);
			SceneGlyph.glyphColor = layerProperties.color;
			SceneGlyph.rasterImage = image;
			draw(graphics);
			SceneGlyph.rasterImage = null;
			setDrawMode(31);
			clipper.draw();
			Shape.closeClipper();
			return image;
		}

		public SceneGroup drawGroups(Graphics g)
		{
			ArrayList selectedGroups = new ArrayList();
			return drawGroups(g, selectedGroups);
		}

		public SceneGroup drawGroups(Graphics g, ArrayList selectedGroups)
		{
			SceneGroup result = null;
			Matrix transform = g.Transform.Clone();
			Matrix matrix = new Matrix();
			selectedGroups.Clear();
			if (sceneGroups.Count > 0 && ((uint)drawMode & 0x20u) != 0)
			{
				g.Transform = matrix;
				int width = (int)Math.Round(g.VisibleClipBounds.Width);
				int height = (int)Math.Round(g.VisibleClipBounds.Height);
				SceneGlyph.rasterImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);
				Graphics graphics = Graphics.FromImage(SceneGlyph.rasterImage);
				graphics.Clear(Color.FromArgb(0, 0, 0, 0));
			}
			matrix.Multiply(worldToCanvas, MatrixOrder.Prepend);
			g.Transform = matrix;
			bool flag = true;
			for (int num = sceneGroups.Count - 1; num >= 0; num--)
			{
				SceneGroup sceneGroup = (SceneGroup)sceneGroups[num];
				if (sceneGroup.children.Count < 1)
				{
					removeGroup(sceneGroup);
				}
			}
			if (Form1.myRootForm.weAreKerning)
			{
				foreach (SceneGroup sceneGroup4 in sceneGroups)
				{
					if (sceneGroup4.selected)
					{
						sceneGroup4.kerning = true;
						sceneGroup4.kern(g);
						sceneGroup4.kerning = false;
					}
				}
			}
			else
			{
				foreach (SceneGroup sceneGroup5 in sceneGroups)
				{
					sceneGroup5.draw(g);
					flag = flag && sceneGroup5.renderType;
					if (sceneGroup5.selected)
					{
						selectedGroups.Add(sceneGroup5);
						if (sceneGroup5.lastSelected)
						{
							result = sceneGroup5;
						}
					}
				}
			}
			if (SceneGlyph.rasterImage != null)
			{
				_ = SceneGlyph.rasterImage;
				g.Transform = new Matrix();
				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.InterpolationMode = InterpolationMode.High;
				g.CompositingMode = CompositingMode.SourceOver;
				g.CompositingQuality = CompositingQuality.HighQuality;
				g.DrawImage(SceneGlyph.rasterImage, 0, 0, SceneGlyph.rasterImage.Width, SceneGlyph.rasterImage.Height);
				SceneGlyph.rasterImage = null;
				g.Transform = transform;
			}
			return result;
		}

		public void drawImages(Graphics g)
		{
			Matrix matrix = new Matrix();
			matrix.Reset();
			matrix.Multiply(worldToCanvas, MatrixOrder.Prepend);
			g.Transform = matrix;
			foreach (PCImage image in images)
			{
				image.draw(g);
			}
		}

		public void deselectAll()
		{
			foreach (SceneGroup sceneGroup in sceneGroups)
			{
				sceneGroup.selected = false;
			}
			foreach (PCImage image in images)
			{
				image.selected = false;
			}
			selectedGroup = null;
		}

		private void canvasPicBox_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(PCImage)))
			{
				e.Effect = DragDropEffects.Copy;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}

		private void canvasPicBox_DragDrop(object sender, DragEventArgs e)
		{
			Point point = canvasPicBox.PointToClient(new Point(e.X, e.Y));
			PCImage pCImage = (PCImage)e.Data.GetData(typeof(PCImage));
			PointF[] array = new PointF[1]
			{
				new PointF(point.X, point.Y)
			};
			canvasToWorld.TransformPoints(array);
			pCImage.positionX.f = (float)Math.Round(array[0].X, 2);
			pCImage.positionY.f = (float)Math.Round(array[0].Y, 2);
			pCImage.assign(4.5f);
			deselectAll();
			pCImage.selected = true;
			images.Add(pCImage);
			Form1.myRootForm.refreshMattePicBox();
		}

		public bool ThumbnailCallback()
		{
			return false;
		}

		public bool saveGypsy(GypsyWriter gw, bool saveThumb, int layerNo)
		{
			gw.drawR = drawR;
			foreach (SceneGroup sceneGroup in sceneGroups)
			{
				sceneGroup.saveGypsy(gw, layerNo);
			}
			if (saveThumb && previewBitmap != null)
			{
				Form1.myRootForm.refreshMattePicBox();
				Image.GetThumbnailImageAbort callback = ThumbnailCallback;
				int num = 184;
				int num2 = 130;
				Bitmap bitmap = null;
				if (1 == Form1.myRootForm.matSize)
				{
					num = num2;
				}
				else
				{
					num2 = 100;
				}
				bitmap = (Bitmap)previewBitmap.GetThumbnailImage(num, num2, callback, IntPtr.Zero);
				Bitmap bitmap2 = new Bitmap(num, num2, PixelFormat.Format24bppRgb);
				Graphics graphics = Graphics.FromImage(bitmap2);
				graphics.FillRectangle(Brushes.White, 0, 0, bitmap2.Width, bitmap2.Height);
				graphics.DrawImage(bitmap, new Rectangle((bitmap2.Width - bitmap.Width) / 2, 0, bitmap.Width, bitmap.Height));
				graphics.Dispose();
				bitmap.Dispose();
				bitmap = bitmap2;
				gw.SetThumbnail(bitmap);
			}
			return true;
		}

		public unsafe bool save(BinaryWriter bw, string filename, bool saveThumb)
		{
			if (saveThumb && previewBitmap != null)
			{
				Form1.myRootForm.refreshMattePicBox();
				Image.GetThumbnailImageAbort callback = ThumbnailCallback;
				Bitmap bitmap = null;
				bitmap = ((1 != Form1.myRootForm.matSize) ? ((Bitmap)previewBitmap.GetThumbnailImage(96, 48, callback, IntPtr.Zero)) : ((Bitmap)previewBitmap.GetThumbnailImage(96, 96, callback, IntPtr.Zero)));
				Path.GetDirectoryName(filename);
				int width = bitmap.Width;
				int height = bitmap.Height;
				bw.Write("EmbeddedThumbnail");
				bw.Write(width);
				bw.Write(height);
				BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
				int stride = bitmapData.Stride;
				for (int num = height - 1; num >= 0; num--)
				{
					byte* ptr = (byte*)(void*)bitmapData.Scan0 + (long)num * (long)stride;
					for (int i = 0; i < width; i++)
					{
						bw.Write(*ptr);
						bw.Write(ptr[1]);
						bw.Write(ptr[2]);
						ptr += 3;
					}
				}
				bitmap.UnlockBits(bitmapData);
				bitmap.Dispose();
			}
			bw.Write("CanvasBegin");
			bw.Write("LayerPropertiesBegin");
			bw.Write("Name");
			bw.Write(layerProperties.layerName);
			bw.Write("MatteWidth");
			bw.Write(matteWidth.ToString());
			bw.Write("MatteHeight");
			bw.Write(matteHeight.ToString());
			bw.Write("PaperWidth");
			bw.Write(layerProperties.paperWidth.ToString());
			bw.Write("PaperHeight");
			bw.Write(layerProperties.paperHeight.ToString());
			bw.Write("IncludeInPreview");
			bw.Write(layerProperties.includeInPreview.ToString());
			bw.Write("Color");
			bw.Write(layerProperties.color.A.ToString());
			bw.Write(layerProperties.color.R.ToString());
			bw.Write(layerProperties.color.G.ToString());
			bw.Write(layerProperties.color.B.ToString());
			bw.Write("LayerPropertiesEnd");
			foreach (PCImage image in images)
			{
				image.save(bw);
			}
			foreach (SceneGroup sceneGroup in sceneGroups)
			{
				sceneGroup.save(bw);
			}
			bw.Write("CanvasEnd");
			dirty = false;
			return true;
		}

		public static bool checkNextCanvas(BinaryReader br)
		{
			string text = null;
			try
			{
				text = br.ReadString();
			}
			catch
			{
				return false;
			}
			if (string.Compare(text, "CanvasBegin") == 0)
			{
				return true;
			}
			return false;
		}

		public bool readLayerProperties(BinaryReader br)
		{
			bool flag = true;
			do
			{
				string strA = null;
				try
				{
					strA = br.ReadString();
				}
				catch
				{
					flag = false;
				}
				if (string.Compare(strA, "Name") == 0)
				{
					layerProperties.layerName = br.ReadString();
				}
				else if (string.Compare(strA, "PaperWidth") == 0)
				{
					layerProperties.paperWidth = float.Parse(br.ReadString());
				}
				else if (string.Compare(strA, "PaperHeight") == 0)
				{
					layerProperties.paperHeight = float.Parse(br.ReadString());
				}
				else if (string.Compare(strA, "MatteWidth") == 0)
				{
					matteWidth = float.Parse(br.ReadString());
				}
				else if (string.Compare(strA, "MatteHeight") == 0)
				{
					matteHeight = float.Parse(br.ReadString());
				}
				else if (string.Compare(strA, "IncludeInPreview") == 0)
				{
					layerProperties.includeInPreview = bool.Parse(br.ReadString());
				}
				else if (string.Compare(strA, "Color") == 0)
				{
					byte alpha = byte.Parse(br.ReadString());
					byte red = byte.Parse(br.ReadString());
					byte green = byte.Parse(br.ReadString());
					byte blue = byte.Parse(br.ReadString());
					layerProperties.color = Color.FromArgb(alpha, red, green, blue);
				}
				else if (string.Compare(strA, "LayerPropertiesEnd") == 0)
				{
					break;
				}
			}
			while (flag);
			return flag;
		}

		public bool read(BinaryReader br)
		{
			Form1 myRootForm = Form1.myRootForm;
			removeAll();
			bool flag = true;
			do
			{
				string strA = null;
				try
				{
					strA = SceneGroup.readNextString(br, null);
				}
				catch
				{
					flag = false;
				}
				if (string.Compare(strA, "PCImage") == 0)
				{
					PCImage value = PCImage.read(br);
					images.Add(value);
				}
				else if (string.Compare(strA, "SceneGroup") == 0)
				{
					SceneGroup value2 = (SceneGroup)SceneGroup.read(br);
					sceneGroups.Add(value2);
				}
				else if (string.Compare(strA, "PreviewThumbnail") == 0)
				{
					br.ReadString();
				}
				else if (string.Compare(strA, "EmbeddedThumbnail") == 0)
				{
					int num = br.ReadInt32();
					int num2 = br.ReadInt32();
					byte[] buffer = new byte[num * 3];
					for (int i = 0; i < num2; i++)
					{
						br.Read(buffer, 0, num * 3);
					}
					buffer = null;
				}
				else if (string.Compare(strA, "CanvasBegin") != 0)
				{
					if (string.Compare(strA, "CanvasEnd") == 0)
					{
						break;
					}
					if (string.Compare(strA, "ProjectPropertiesBegin") == 0)
					{
						myRootForm.projectProperties.read(br);
					}
					else if (string.Compare(strA, "LayerPropertiesBegin") == 0)
					{
						readLayerProperties(br);
						init(matteWidth, matteHeight, layerProperties.paperWidth, layerProperties.paperHeight, 1f);
						Panel panel = (Panel)layerProperties.tag;
						TabPage tabPage = (TabPage)panel.Tag;
						tabPage.Name = layerProperties.layerName;
						tabPage.Text = layerProperties.layerName;
					}
				}
			}
			while (flag);
			Form1.myRootForm.refreshMattePicBox();
			return flag;
		}

		internal void AddGlyphWithUndo(FontLoading fontLoading, int keyId, float size)
		{
			undoRedo.addAndDo(new UndoRedo.UndoAddGlyph(fontLoading, keyId, size));
		}

		internal void Select(SceneGroup sg)
		{
			sg.selected = true;
			selectedGroup = sg;
		}

		internal SceneGroup[] GetSelectedGroups()
		{
			int num = 0;
			foreach (SceneGroup sceneGroup3 in sceneGroups)
			{
				if (sceneGroup3.selected)
				{
					num++;
				}
			}
			SceneGroup[] array = new SceneGroup[num];
			num = 0;
			foreach (SceneGroup sceneGroup4 in sceneGroups)
			{
				if (sceneGroup4.selected)
				{
					array[num++] = sceneGroup4;
				}
			}
			return array;
		}

		internal PCImage[] GetSelectedImages()
		{
			int num = 0;
			foreach (PCImage image in images)
			{
				if (image.selected)
				{
					num++;
				}
			}
			PCImage[] array = new PCImage[num];
			num = 0;
			foreach (PCImage image2 in images)
			{
				if (image2.selected)
				{
					array[num++] = image2;
				}
			}
			return array;
		}

		internal bool IsAnythingSelected()
		{
			foreach (SceneGroup sceneGroup in sceneGroups)
			{
				if (sceneGroup.selected)
				{
					return true;
				}
			}
			foreach (PCImage image in images)
			{
				if (image.selected)
				{
					return true;
				}
			}
			return false;
		}
	}
}
