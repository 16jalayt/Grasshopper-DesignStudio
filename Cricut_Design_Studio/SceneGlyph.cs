using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace Cricut_Design_Studio
{
	public class SceneGlyph : SceneNode
	{
		public class ClipArt
		{
			public PCImage clipArt;

			public float sx;

			public float sy;

			public float tx;

			public float ty;
		}

		public static SceneGlyph selectedGlyph = null;

		public Shape shape;

		public ClipArt clipArt;

		public int localFontId;

		public int fontId;

		public int keyId;

		public Matrix glyphToWorld;

		public RectangleF bbox;

		public bool bboxAssigned;

		public bool isSpace;

		public float ox;

		public float oy;

		public float size;

		public bool[] contourInvis;

		public int selectedContour = -1;

		public static Bitmap rasterImage = null;

		public static Color glyphColor = Color.Gray;

		private Pen selectedPen;

		public static bool saveAsPhil = true;

		public SceneGlyph()
		{
		}

		public SceneGlyph(float ox, float oy, float size)
		{
			this.ox = ox;
			this.oy = oy;
			this.size = size;
			isSpace = true;
			bbox.Location = new PointF(ox, oy);
			bbox.Size = new SizeF(size, size / 2f);
		}

		public SceneGlyph(Shape shape, int fontId, int keyId, float ox, float oy, float size)
		{
			this.shape = shape;
			this.fontId = fontId;
			this.keyId = keyId;
			this.ox = ox;
			this.oy = oy;
			this.size = size;
			glyphToWorld = Canvas.glyphToWorld(ox, oy, size);
			Glyph glyph = shape.getGlyph(fontId, keyId);
			if (glyph != null)
			{
				contourInvis = new bool[glyph.nContours];
				for (int i = 0; i < glyph.nContours; i++)
				{
					contourInvis[i] = false;
				}
			}
		}

		public override void draw(Graphics g)
		{
			if (isSpace)
			{
				return;
			}
			Matrix transform = g.Transform.Clone();
			Matrix matrix = g.Transform.Clone();
			g.Transform = new Matrix();
			int num = (int)Math.Round(g.VisibleClipBounds.Width);
			int num2 = (int)Math.Round(g.VisibleClipBounds.Height);
			matrix.Multiply(glyphToWorld, MatrixOrder.Prepend);
			try
			{
				g.Transform = matrix;
			}
			catch
			{
			}
			SceneGroup sceneGroup = (SceneGroup)parent;
			if (rasterImage != null)
			{
				Glyph glyph = shape.getGlyph(fontId, keyId);
				int num3 = 3;
				while (num3 > 1 && (num * num3 >= 3000 || num2 * num3 >= 3000))
				{
					num3--;
				}
				shape.setGraphics(null, null, sceneGroup.flipShapes, sceneGroup.welding, sceneGroup.kerning);
				shape.cutMatrix = matrix;
				Bitmap bitmap = (Bitmap)shape.renderFullSize(glyph, num * num3, num2 * num3, num3, (int)Math.Round(sceneGroup.erosion.f), glyphColor, this);
				shape.cutMatrix = new Matrix();
				Graphics graphics = Graphics.FromImage(rasterImage);
				Rectangle rect = new Rectangle(0, 0, bitmap.Width / num3, bitmap.Height / num3);
				if (glyph.imgPath != null)
				{
					Bitmap bitmap2 = (Bitmap)Image.FromFile(glyph.imgPath);
					float width = 800f * ((float)bitmap2.Width / (float)bitmap2.Height);
					float height = 800f;
					float x = 15f;
					float y = -800f;
					graphics.Transform = matrix.Clone();
					graphics.CompositingMode = CompositingMode.SourceCopy;
					graphics.DrawImage(bitmap2, x, y, width, height);
					sceneGroup.renderType = true;
				}
				else
				{
					graphics.InterpolationMode = InterpolationMode.High;
					graphics.SmoothingMode = SmoothingMode.AntiAlias;
					graphics.CompositingQuality = CompositingQuality.HighQuality;
					graphics.CompositingMode = CompositingMode.SourceOver;
					graphics.DrawImage(bitmap, rect);
					sceneGroup.renderType = false;
				}
				bitmap = null;
			}
			else
			{
				if (this == selectedGlyph)
				{
					if (selectedPen == null)
					{
						selectedPen = new Pen(Color.Black, 1f);
						selectedPen.DashStyle = DashStyle.Dash;
					}
					shape.setGraphics(g, selectedPen, sceneGroup.flipShapes, sceneGroup.welding, sceneGroup.kerning);
					if (sceneGroup.kerning)
					{
						shape.cutMatrix = glyphToWorld.Clone();
						shape.drawGlyph(fontId, keyId, this);
						shape.cutMatrix = null;
					}
					else
					{
						shape.drawGlyph(fontId, keyId, this);
					}
				}
				else
				{
					shape.setGraphics(g, null, sceneGroup.flipShapes, sceneGroup.welding, sceneGroup.kerning);
					if (sceneGroup.kerning)
					{
						shape.cutMatrix = glyphToWorld.Clone();
						shape.drawGlyph(fontId, keyId, this);
						shape.cutMatrix = null;
					}
					else
					{
						shape.drawGlyph(fontId, keyId, this);
					}
				}
				if (!bboxAssigned)
				{
					bbox = new RectangleF(shape.bboxMinX, shape.bboxMinY, shape.bboxMaxX - shape.bboxMinX, shape.bboxMaxY - shape.bboxMinY);
					bboxAssigned = true;
					sceneGroup.calcBbox(null);
				}
			}
			g.Transform = transform;
		}

		public void cut(PcControl pc, Matrix m)
		{
			if (isSpace)
			{
				return;
			}
			SceneGroup sceneGroup = (SceneGroup)parent;
			Matrix matrix = m.Clone();
			matrix.Multiply(glyphToWorld, MatrixOrder.Prepend);
			shape.setGraphics(null, null, sceneGroup.flipShapes, sceneGroup.welding, kerning: false);
			shape.setCricut(pc, matrix);
			if (sceneGroup.welding)
			{
				shape.drawGlyph(fontId, keyId, this);
				return;
			}
			for (int i = 0; i < Form1.myRootForm.multiCut + 1; i++)
			{
				shape.drawGlyph(fontId, keyId, this);
			}
		}

		public override bool wouldSelect(PointF mpnt, Matrix t)
		{
			if (isSpace || shape == null)
			{
				return false;
			}
			Matrix matrix = t.Clone();
			matrix.Multiply(glyphToWorld, MatrixOrder.Prepend);
			shape.setSelectionPoint(mpnt, matrix, 0.0625f);
			shape.flipShapes = ((SceneGroup)parent).flipShapes;
			shape.drawGlyph(fontId, keyId, this);
			shape.setSelectionOff();
			return shape.isSelected();
		}

		public override bool select(PointF mpnt, Matrix t)
		{
			if (isSpace || shape == null)
			{
				return false;
			}
			Matrix matrix = t.Clone();
			matrix.Multiply(glyphToWorld, MatrixOrder.Prepend);
			shape.setSelectionPoint(mpnt, matrix, 0.0625f);
			shape.flipShapes = ((SceneGroup)parent).flipShapes;
			shape.drawGlyph(fontId, keyId, this);
			shape.setSelectionOff();
			bool flag = shape.isSelected();
			if (flag)
			{
				selectedGlyph = this;
				Form1.myRootForm.EnableGlyphManipulationControls(en: true);
			}
			if (flag && SceneUtils.transformHandles != null && SceneUtils.transformHandles[5] != null)
			{
				SceneUtils.transforming = true;
				SceneUtils.thisHandle = SceneUtils.transformHandles[5];
				SceneGroup.dragging = true;
			}
			return flag;
		}

		public override void saveGypsy(GypsyWriter gw, int layerNo)
		{
			if (!Form1.myRootForm.trialMode)
			{
				if (saveAsPhil)
				{
					saveGypsyPhil(gw);
				}
				else
				{
					saveGypsyDan(gw);
				}
			}
		}

		public void saveGypsyPhil(GypsyWriter gw)
		{
			string fontName = shape.getFontName();
			Glyph glyph = shape.getGlyph(fontId, keyId);
			int shiftState = fontId;
			int num = keyId - 2;
			int column = num % 14;
			int row = num / 14;
			if (num >= 0)
			{
				gw.WriteGlyph(fontName, shiftState, column, row, ox, oy, shape.welding, this, glyphToWorld, glyph);
			}
		}

		public void saveGypsyDan(GypsyWriter gw)
		{
			string fontName = shape.getFontName();
			Glyph glyph = shape.getGlyph(fontId, keyId);
			double xpos = (float)glyph.xMin;
			double ypos = (float)glyph.yMin;
			double xsize = (float)(glyph.xMax - glyph.xMin);
			double ysize = (float)(glyph.yMax - glyph.yMin);
			PointF[] array = new PointF[1]
			{
				new PointF(ox, oy)
			};
			int num = keyId - 2;
			int shiftState = fontId;
			int column = num % 14;
			int row = num / 14;
			gw.WriteGlyphDan(fontName, shiftState, column, row, array[0].X, array[0].Y, xpos, ypos, xsize, ysize, shape.welding, glyphToWorld);
		}

		public override void save(BinaryWriter bw)
		{
			bw.Write("SceneGlyph");
			bw.Write(fontId);
			bw.Write(keyId);
			bw.Write(isSpace);
			bw.Write(ox);
			bw.Write(oy);
			bw.Write(size);
			bw.Write(localFontId);
			if (contourInvis == null || contourInvis.Length <= 0)
			{
				return;
			}
			int num = 0;
			bool[] array = contourInvis;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i])
				{
					num++;
				}
			}
			if (num > 1)
			{
				bw.Write("GlyphContours");
				bw.Write(contourInvis.Length);
				bool[] array2 = contourInvis;
				foreach (bool value in array2)
				{
					bw.Write(value);
				}
			}
		}

		public static SceneNode read(BinaryReader br)
		{
			SceneGlyph sceneGlyph = new SceneGlyph();
			sceneGlyph.bboxAssigned = false;
			sceneGlyph.fontId = br.ReadInt32();
			sceneGlyph.keyId = br.ReadInt32();
			sceneGlyph.isSpace = br.ReadBoolean();
			sceneGlyph.ox = br.ReadSingle();
			sceneGlyph.oy = br.ReadSingle();
			sceneGlyph.size = br.ReadSingle();
			sceneGlyph.localFontId = br.ReadInt32();
			string text = (string)SceneGroup.localFontNames[sceneGlyph.localFontId];
			foreach (Shape item in Shape.shapesById)
			{
				if (text.CompareTo(item.header.cartHeader.fontName) == 0)
				{
					sceneGlyph.shape = item;
				}
			}
			if (sceneGlyph.shape == null)
			{
				return null;
			}
			if (sceneGlyph.isSpace)
			{
				sceneGlyph.bbox.Location = new PointF(sceneGlyph.ox, sceneGlyph.oy);
				sceneGlyph.bbox.Size = new SizeF(sceneGlyph.size, sceneGlyph.size / 2f);
			}
			else
			{
				sceneGlyph.glyphToWorld = Canvas.glyphToWorld(sceneGlyph.ox, sceneGlyph.oy, sceneGlyph.size).Clone();
			}
			Glyph glyph = sceneGlyph.shape.getGlyph(sceneGlyph.fontId, sceneGlyph.keyId);
			if (glyph != null)
			{
				sceneGlyph.contourInvis = new bool[glyph.nContours];
				for (int i = 0; i < glyph.nContours; i++)
				{
					sceneGlyph.contourInvis[i] = false;
				}
			}
			string text2 = SceneGroup.readNextString(br, "GlyphContours");
			if (text2 != null)
			{
				int num = br.ReadInt32();
				_ = sceneGlyph.contourInvis.Length;
				for (int j = 0; j < num; j++)
				{
					sceneGlyph.contourInvis[j] = br.ReadBoolean();
				}
			}
			return sceneGlyph;
		}

		public SceneGlyph getCopy()
		{
			SceneGlyph sceneGlyph = new SceneGlyph();
			sceneGlyph.fontId = fontId;
			sceneGlyph.keyId = keyId;
			sceneGlyph.isSpace = isSpace;
			sceneGlyph.ox = ox;
			sceneGlyph.oy = oy;
			sceneGlyph.size = size;
			sceneGlyph.localFontId = localFontId;
			sceneGlyph.shape = shape;
			if (sceneGlyph.isSpace)
			{
				sceneGlyph.bbox.Location = new PointF(sceneGlyph.ox, sceneGlyph.oy);
				sceneGlyph.bbox.Size = new SizeF(sceneGlyph.size, sceneGlyph.size / 2f);
			}
			else
			{
				sceneGlyph.glyphToWorld = Canvas.glyphToWorld(sceneGlyph.ox, sceneGlyph.oy, sceneGlyph.size);
			}
			return sceneGlyph;
		}

		public RectangleF getBbox()
		{
			return bbox;
		}
	}
}
