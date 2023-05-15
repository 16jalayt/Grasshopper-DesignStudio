using System.Drawing;
using System.Drawing.Drawing2D;

namespace Cricut_Design_Studio
{
	public class SceneUtils
	{
		public class GlyphTransformHandle
		{
			public Region region;

			public int tag;

			public PointF pnt;

			public GlyphTransformHandle(int t)
			{
				tag = t;
			}
		}

		public static GlyphTransformHandle[] transformHandles = null;

		public static GlyphTransformHandle thisHandle = null;

		public static bool transforming = false;

		public static int dragHandle = -1;

		public static void drawEmptySelectionIcon(Graphics g, float x, float y, bool dragging, PointF mpnt, Matrix modelingTransform, bool tentativeSelection)
		{
			SolidBrush solidBrush = new SolidBrush(tentativeSelection ? Color.FromArgb(160, 255, 222, 196) : Color.FromArgb(160, 196, 222, 255));
			Pen pen = new Pen(Color.FromArgb(255, 0, 0, 0), 2f * Canvas.penScale);
			float num = 4f * Canvas.penScale;
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddEllipse(x - num, y - num, 2f * num, 2f * num);
			PointF[] array = new PointF[4];
			array[0].X = x - num;
			array[0].Y = y - num;
			array[1].X = x + num;
			array[1].Y = y - num;
			array[2].X = x + num;
			array[2].Y = y + num;
			array[3].X = x - num;
			array[3].Y = y + num;
			g.FillPath(solidBrush, graphicsPath);
			g.DrawEllipse(pen, x - num, y - num, 2f * num, 2f * num);
			pen.Dispose();
			solidBrush.Dispose();
		}

		public static void drawRotateHandleIcon(Graphics g, float x, float y, bool dragging, PointF mpnt, GlyphTransformHandle th, Matrix modelingTransform, bool hot)
		{
			SolidBrush solidBrush = new SolidBrush(Color.FromArgb(160, 196, 222, 255));
			Pen pen = new Pen(Color.FromArgb(255, 0, 0, 0), 2f * Canvas.penScale);
			float num = 15f * Canvas.penScale;
			float num2 = 5f * Canvas.penScale;
			float num3 = 8f * Canvas.penScale;
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddEllipse(x - num, y - num, 2f * num, 2f * num);
			PointF[] array = new PointF[4];
			array[0].X = x - num;
			array[0].Y = y - num;
			array[1].X = x + num;
			array[1].Y = y - num;
			array[2].X = x + num;
			array[2].Y = y + num;
			array[3].X = x - num;
			array[3].Y = y + num;
			if (th != null)
			{
				if (hot)
				{
					solidBrush.Color = Color.FromArgb(160, 255, 255, 0);
				}
				th.pnt.X = x;
				th.pnt.Y = y;
				th.region = new Region(graphicsPath);
				Region region = th.region.Clone();
				region.Transform(modelingTransform);
				if (dragging && region.IsVisible(mpnt, g))
				{
					transforming = true;
					thisHandle = th;
				}
				g.FillRegion(solidBrush, th.region);
			}
			else
			{
				g.FillPath(solidBrush, graphicsPath);
			}
			g.DrawEllipse(pen, x - num, y - num, 2f * num, 2f * num);
			num = 12f * Canvas.penScale;
			solidBrush.Color = pen.Color;
			g.DrawArc(pen, x - (num - num3), y - (2f * num - num2 + Canvas.penScale), 2f * num, 2f * num, 90f, 90f);
			g.FillPolygon(solidBrush, new PointF[3]
			{
				new PointF(x - (num - num3), y - num),
				new PointF(x - (num - num3) + num2, y - (num - num2)),
				new PointF(x - (num - num3) - num2, y - (num - num2))
			});
			g.FillPolygon(solidBrush, new PointF[3]
			{
				new PointF(x + num, y + (num - num3)),
				new PointF(x + (num - num2), y + (num - num3) + num2),
				new PointF(x + (num - num2), y + (num - num3) - num2)
			});
			pen.Dispose();
			solidBrush.Dispose();
		}

		public static void drawShearHandleIcon(Graphics g, float x, float y, bool dragging, PointF mpnt, GlyphTransformHandle th, Matrix modelingTransform, bool hot)
		{
			SolidBrush solidBrush = new SolidBrush(Color.FromArgb(160, 196, 222, 255));
			Pen pen = new Pen(Color.FromArgb(255, 0, 0, 0), 2f * Canvas.penScale);
			float num = 16f * Canvas.penScale;
			float num2 = 15f * Canvas.penScale * 0.5f;
			float num3 = 5f * Canvas.penScale;
			float num4 = 10f * Canvas.penScale;
			RectangleF rect = new RectangleF(x - num, y - num, 2f * num, 2f * num);
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddEllipse(rect);
			PointF[] array = new PointF[4];
			if (th != null)
			{
				if (hot)
				{
					solidBrush.Color = Color.FromArgb(160, 255, 255, 0);
				}
				th.pnt.X = x;
				th.pnt.Y = y;
				th.region = new Region(graphicsPath);
				Region region = th.region.Clone();
				region.Transform(modelingTransform);
				if (dragging && region.IsVisible(mpnt, g))
				{
					transforming = true;
					thisHandle = th;
				}
				g.FillRegion(solidBrush, th.region);
			}
			else
			{
				g.FillPath(solidBrush, graphicsPath);
			}
			g.DrawEllipse(pen, rect);
			num2 = 12f * Canvas.penScale;
			solidBrush.Color = pen.Color;
			g.DrawLine(pen, x - (num2 - num3), y, x + (num2 - num4), y);
			array = new PointF[3]
			{
				new PointF(x - num2, y),
				new PointF(x - (num2 - num3), y + num3),
				new PointF(x - (num2 - num3), y - num3)
			};
			g.FillPolygon(solidBrush, array);
			ref PointF reference = ref array[0];
			reference = new PointF(x + (num2 - num3), y);
			ref PointF reference2 = ref array[1];
			reference2 = new PointF(x + (num2 - num4), y + num3);
			ref PointF reference3 = ref array[2];
			reference3 = new PointF(x + (num2 - num4), y - num3);
			g.FillPolygon(solidBrush, array);
			g.DrawLine(pen, x - num2, y - num4, x - num2, y + num4);
			g.DrawLine(pen, x + num2, y - num4, x + (num2 - num4), y + num4);
			pen.Dispose();
			solidBrush.Dispose();
		}

		public static void drawMoveHandleIcon(Graphics g, float x, float y, bool showHorz, bool showVert, bool dragging, PointF mpnt, GlyphTransformHandle th, Matrix modelingTransform, bool hot)
		{
			SolidBrush solidBrush = new SolidBrush(Color.FromArgb(160, 196, 222, 255));
			Pen pen = new Pen(Color.FromArgb(255, 0, 0, 0), 2f * Canvas.penScale);
			float num = 15f * Canvas.penScale;
			float num2 = 5f * Canvas.penScale;
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddEllipse(x - num, y - num, 2f * num, 2f * num);
			PointF[] array = new PointF[4];
			if (th != null)
			{
				if (hot)
				{
					solidBrush.Color = Color.FromArgb(160, 255, 255, 0);
				}
				th.pnt.X = x;
				th.pnt.Y = y;
				th.region = new Region(graphicsPath);
				Region region = th.region.Clone();
				region.Transform(modelingTransform);
				if (dragging && region.IsVisible(mpnt, g))
				{
					transforming = true;
					thisHandle = th;
				}
				g.FillRegion(solidBrush, th.region);
			}
			else
			{
				g.FillPath(solidBrush, graphicsPath);
			}
			g.DrawEllipse(pen, x - num, y - num, 2f * num, 2f * num);
			num = 12f * Canvas.penScale;
			solidBrush.Color = pen.Color;
			if (showHorz)
			{
				g.DrawLine(pen, x - (num - num2), y, x + (num - num2), y);
				g.FillPolygon(solidBrush, new PointF[3]
				{
					new PointF(x - num, y),
					new PointF(x - (num - num2), y + num2),
					new PointF(x - (num - num2), y - num2)
				});
				g.FillPolygon(solidBrush, new PointF[3]
				{
					new PointF(x + num, y),
					new PointF(x + (num - num2), y + num2),
					new PointF(x + (num - num2), y - num2)
				});
			}
			if (showVert)
			{
				g.DrawLine(pen, x, y - (num - num2), x, y + (num - num2));
				g.FillPolygon(solidBrush, new PointF[3]
				{
					new PointF(x, y - num),
					new PointF(x + num2, y - (num - num2)),
					new PointF(x - num2, y - (num - num2))
				});
				g.FillPolygon(solidBrush, new PointF[3]
				{
					new PointF(x, y + num),
					new PointF(x + num2, y + (num - num2)),
					new PointF(x - num2, y + (num - num2))
				});
			}
			pen.Dispose();
			solidBrush.Dispose();
		}

		public static void drawSizeHandleIcon(Graphics g, float x, float y, bool showHorz, bool showVert, bool dragging, PointF mpnt, GlyphTransformHandle th, Matrix modelingTransform, bool hot)
		{
			SolidBrush solidBrush = new SolidBrush(Color.FromArgb(160, 196, 222, 255));
			Pen pen = new Pen(Color.FromArgb(255, 0, 0, 0), 2f * Canvas.penScale);
			float num = 15f * Canvas.penScale;
			float num2 = 4f * Canvas.penScale;
			float num3 = 6f * Canvas.penScale;
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddEllipse(x - num, y - num, 2f * num, 2f * num);
			PointF[] array = new PointF[4];
			if (th != null)
			{
				if (hot)
				{
					solidBrush.Color = Color.FromArgb(160, 255, 255, 0);
				}
				th.pnt.X = x;
				th.pnt.Y = y;
				th.region = new Region(graphicsPath);
				Region region = th.region.Clone();
				region.Transform(modelingTransform);
				if (dragging && region.IsVisible(mpnt, g))
				{
					transforming = true;
					thisHandle = th;
				}
				g.FillRegion(solidBrush, th.region);
			}
			else
			{
				g.FillPath(solidBrush, graphicsPath);
			}
			g.DrawEllipse(pen, x - num, y - num, 2f * num, 2f * num);
			num = 12f * Canvas.penScale;
			solidBrush.Color = pen.Color;
			if (showHorz)
			{
				g.DrawLine(pen, x - (num - num2), y, x + (num - num3), y);
				g.FillPolygon(solidBrush, new PointF[3]
				{
					new PointF(x - num, y),
					new PointF(x - (num - num2), y + num2),
					new PointF(x - (num - num2), y - num2)
				});
				g.FillPolygon(solidBrush, new PointF[3]
				{
					new PointF(x + num, y),
					new PointF(x + (num - num3), y + num3),
					new PointF(x + (num - num3), y - num3)
				});
			}
			if (showVert)
			{
				g.DrawLine(pen, x, y - (num - num2), x, y + (num - num3));
				g.FillPolygon(solidBrush, new PointF[3]
				{
					new PointF(x, y - num),
					new PointF(x + num2, y - (num - num2)),
					new PointF(x - num2, y - (num - num2))
				});
				g.FillPolygon(solidBrush, new PointF[3]
				{
					new PointF(x, y + num),
					new PointF(x + num3, y + (num - num3)),
					new PointF(x - num3, y + (num - num3))
				});
			}
			pen.Dispose();
			solidBrush.Dispose();
		}

		public static void drawHandles(Graphics g, PointF[] bbox, PointF cm, float siz, bool dragging, PointF mpnt, Matrix modelingTransform, int enableBits)
		{
			if (transformHandles == null)
			{
				transformHandles = new GlyphTransformHandle[9];
				for (int i = 0; i < 9; i++)
				{
					transformHandles[i] = new GlyphTransformHandle(i);
				}
			}
			GlyphTransformHandle[] array = new GlyphTransformHandle[9];
			for (int j = 0; j < 9; j++)
			{
				if ((enableBits & (1 << j)) == 0)
				{
					array[j] = null;
				}
				else
				{
					array[j] = transformHandles[j];
				}
			}
			new Pen(Color.FromArgb(64, 0, 0, 0), 1f * Canvas.penScale);
			new SolidBrush(Color.FromArgb(64, 0, 0, 0));
			drawShearHandleIcon(g, bbox[2].X, bbox[2].Y, dragging, mpnt, array[3], modelingTransform, 3 == dragHandle);
			drawRotateHandleIcon(g, bbox[0].X, bbox[0].Y, dragging, mpnt, array[7], modelingTransform, 7 == dragHandle);
			drawMoveHandleIcon(g, (bbox[3].X + bbox[0].X) / 2f, (bbox[3].Y + bbox[0].Y) / 2f, showHorz: true, showVert: false, dragging, mpnt, array[8], modelingTransform, 8 == dragHandle);
			drawMoveHandleIcon(g, (bbox[3].X + bbox[2].X) / 2f, (bbox[3].Y + bbox[2].Y) / 2f, showHorz: false, showVert: true, dragging, mpnt, array[6], modelingTransform, 6 == dragHandle);
			drawSizeHandleIcon(g, (bbox[0].X + bbox[1].X) / 2f, (bbox[0].Y + bbox[1].Y) / 2f, showHorz: false, showVert: true, dragging, mpnt, array[2], modelingTransform, 2 == dragHandle);
			drawSizeHandleIcon(g, (bbox[2].X + bbox[1].X) / 2f, (bbox[2].Y + bbox[1].Y) / 2f, showHorz: true, showVert: false, dragging, mpnt, array[4], modelingTransform, 4 == dragHandle);
			drawMoveHandleIcon(g, bbox[3].X, bbox[3].Y, showHorz: true, showVert: true, dragging, mpnt, array[5], modelingTransform, 5 == dragHandle);
			drawSizeHandleIcon(g, bbox[1].X, bbox[1].Y, showHorz: true, showVert: true, dragging, mpnt, array[1], modelingTransform, 1 == dragHandle);
		}

		public static void drawSelected(Graphics g, PointF[] bbox, PointF cm, float siz, bool dragging, PointF mpnt, Matrix modelingTransform, int enableBits, bool tentativeSelection)
		{
			if (transformHandles == null)
			{
				transformHandles = new GlyphTransformHandle[9];
				for (int i = 0; i < 9; i++)
				{
					transformHandles[i] = new GlyphTransformHandle(i);
				}
			}
			GlyphTransformHandle[] array = new GlyphTransformHandle[9];
			for (int j = 0; j < 9; j++)
			{
				if ((enableBits & (1 << j)) == 0)
				{
					array[j] = null;
				}
				else
				{
					array[j] = transformHandles[j];
				}
			}
			new Pen(Color.FromArgb(64, 0, 0, 0), 1f * Canvas.penScale);
			new SolidBrush(Color.FromArgb(64, 0, 0, 0));
			drawEmptySelectionIcon(g, bbox[2].X, bbox[2].Y, dragging, mpnt, modelingTransform, tentativeSelection);
			drawEmptySelectionIcon(g, bbox[0].X, bbox[0].Y, dragging, mpnt, modelingTransform, tentativeSelection);
			drawEmptySelectionIcon(g, (bbox[3].X + bbox[0].X) / 2f, (bbox[3].Y + bbox[0].Y) / 2f, dragging, mpnt, modelingTransform, tentativeSelection);
			drawEmptySelectionIcon(g, (bbox[3].X + bbox[2].X) / 2f, (bbox[3].Y + bbox[2].Y) / 2f, dragging, mpnt, modelingTransform, tentativeSelection);
			drawEmptySelectionIcon(g, (bbox[0].X + bbox[1].X) / 2f, (bbox[0].Y + bbox[1].Y) / 2f, dragging, mpnt, modelingTransform, tentativeSelection);
			drawEmptySelectionIcon(g, (bbox[2].X + bbox[1].X) / 2f, (bbox[2].Y + bbox[1].Y) / 2f, dragging, mpnt, modelingTransform, tentativeSelection);
			drawEmptySelectionIcon(g, bbox[3].X, bbox[3].Y, dragging, mpnt, modelingTransform, tentativeSelection);
			drawEmptySelectionIcon(g, bbox[1].X, bbox[1].Y, dragging, mpnt, modelingTransform, tentativeSelection);
		}
	}
}
