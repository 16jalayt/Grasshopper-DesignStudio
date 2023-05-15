using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Cricut_Design_Studio
{
	public class GypsyWriter
	{
		public TextWriter tw;

		public RectangleF bbox = new RectangleF(0f, 0f, 0f, 0f);

		public SceneGroup sg;

		public Matrix transform;

		public float shear;

		private bool flipShapes;

		private float matTop;

		public string lastFontName;

		public RectangleF drawR;

		private FileStream fs;

		public float angle;

		private static bool writeDebugPNG;

		private Bitmap thumbnail;

		public GypsyWriter(FileStream fs)
		{
			this.fs = fs;
			tw = new StreamWriter(fs);
			transform = new Matrix();
			matTop = 0f;
		}

		internal void WriteHeader(string filename, int matSize)
		{
			tw.WriteLine('"' + filename + '"');
			tw.WriteLine("3");
			switch (matSize)
			{
			case 0:
				tw.WriteLine("12 6");
				matTop = 6f;
				break;
			case 1:
				tw.WriteLine("12 12");
				matTop = 12f;
				break;
			case 2:
				tw.WriteLine("24 12");
				matTop = 12f;
				break;
			default:
				throw new Exception("unknown mat size ");
			}
			tw.WriteLine();
		}

		internal void WriteGroupHeader(float kerning, float xshear, float yshear, float rotation, int layer, bool flipShapes)
		{
			this.flipShapes = flipShapes;
			rotation = 0f - rotation;
			tw.WriteLine(kerning + " " + xshear + " " + yshear + " " + rotation + " " + layer);
		}

		internal void EndGroupHeader()
		{
			tw.WriteLine();
		}

		private void WriteFontName(string fontName)
		{
			fontName = fontName.Replace("Cricut(R) ", "");
			if (lastFontName != fontName)
			{
				tw.WriteLine('"' + fontName + '"');
				lastFontName = fontName;
			}
		}

		public void getTransformValues(ref float tx, ref float ty, ref float len1, ref float len2, ref float angle)
		{
			transform.Clone();
			PointF[] array = new PointF[4]
			{
				new PointF(bbox.X, bbox.Y),
				new PointF(bbox.X + bbox.Width, bbox.Y),
				new PointF(bbox.X, bbox.Y + bbox.Height),
				new PointF(bbox.X + bbox.Width / 2f, bbox.Y + bbox.Height / 2f)
			};
			transform.TransformPoints(array);
			float num = array[1].X - array[0].X;
			float num2 = array[1].Y - array[0].Y;
			float num3 = array[2].X - array[0].X;
			float num4 = array[2].Y - array[0].Y;
			float num5 = 0f;
			tx = array[0].X;
			ty = array[0].Y;
			len1 = (float)Math.Sqrt(num * num + num2 * num2);
			len2 = (float)Math.Sqrt(num3 * num3 + num4 * num4);
			angle = 0f;
			if (Math.Abs(len1) > float.Epsilon)
			{
				num5 = num / len1;
				if (num5 < -1f || num5 > 1f)
				{
					Console.WriteLine("dot error");
					if (num5 < -1f)
					{
						num5 = -1f;
					}
					if (num5 > 1f)
					{
						num5 = 1f;
					}
				}
				angle = (float)Math.Acos(num5) * 180f / (float)Math.PI;
				if (num2 < 0f)
				{
					angle = 0f - angle;
				}
			}
			else if (Math.Abs(len2) > float.Epsilon)
			{
				num5 = num4 / len2;
				if (num5 < -1f || num5 > 1f)
				{
					Console.WriteLine("dot error");
					if (num5 < -1f)
					{
						num5 = -1f;
					}
					if (num5 > 1f)
					{
						num5 = 1f;
					}
				}
				angle = (float)Math.Acos(num5) * 180f / (float)Math.PI;
				if (num3 < 0f)
				{
					angle = 0f - angle;
				}
			}
			tx = array[3].X - len1 / 2f;
			ty = array[3].Y + len2 / 2f;
		}

		private void AddShearOffset(float angle, float shear, ref float x, ref float y)
		{
			PointF[] array = new PointF[1]
			{
				new PointF(shear, 0f)
			};
			Matrix matrix = new Matrix();
			matrix.Reset();
			matrix.Rotate(angle);
			matrix.TransformPoints(array);
			x += array[0].X;
			y += array[0].Y;
		}

		internal void WriteGlyph(string fontName, int shiftState, int column, int row, float ox, float oy, bool welding, SceneGlyph sceneGlyph, Matrix glyphToWorld, Glyph glyph)
		{
			float tx = 0f;
			float ty = 0f;
			float len = 1f;
			float len2 = 1f;
			float num = 0f;
			float num2 = shear;
			bool flag = flipShapes;
			bool flag2 = false;
			WriteFontName(fontName);
			PointF[] array = new PointF[4]
			{
				new PointF(glyph.xMin, glyph.yMin),
				new PointF(glyph.xMin, glyph.yMax),
				new PointF(glyph.xMax, glyph.yMin),
				new PointF(glyph.xMax, glyph.yMax)
			};
			sceneGlyph.glyphToWorld.TransformPoints(array);
			bbox.Location = array[0];
			bbox.Size = new SizeF(array[3].X - array[0].X, array[3].Y - array[0].Y);
			getTransformValues(ref tx, ref ty, ref len, ref len2, ref num);
			tx -= drawR.X;
			ty -= drawR.Y;
			num2 *= (float)(glyph.yMax - glyph.yMin) / (float)(glyph.xMax - glyph.xMin);
			num2 *= len;
			AddShearOffset(num, num2 / 2f, ref tx, ref ty);
			num2 /= len2;
			num = 0f - num;
			ty = matTop - ty;
			string text = "";
			int num3 = sceneGlyph.contourInvis.Length - 1;
			while (num3 >= 0 && !sceneGlyph.contourInvis[num3])
			{
				num3--;
			}
			if (num3 >= 0)
			{
				text = " ";
				for (int i = 0; i <= num3; i++)
				{
					text += (sceneGlyph.contourInvis[i] ? 'H' : 'S');
				}
			}
			tw.WriteLine("\t" + shiftState + " " + column + " " + row + " " + tx + " " + ty + " " + len + " " + len2 + " " + num2 + " 0 " + num + " " + (welding ? "1" : "0") + " " + (flag ? "1" : "0") + " " + (flag2 ? "1" : "0") + text);
		}

		private static float Length(PointF p0, PointF p1)
		{
			float num = p1.X - p0.X;
			float num2 = p1.Y - p0.Y;
			return (float)Math.Sqrt(num * num + num2 * num2);
		}

		internal void WriteGlyphDan(string fontName, int shiftState, int column, int row, double x, double y, double xpos, double ypos, double xsize, double ysize, bool welding, Matrix glyphToWorld)
		{
			PointF pointF = new PointF((float)(x + xpos + xsize / 2.0), (float)(y + ypos + ysize / 2.0));
			float num = shear;
			PointF[] array = new PointF[5]
			{
				new PointF((float)(x + xpos), (float)(y + ypos)),
				new PointF((float)x, (float)y),
				new PointF((float)(x + xsize), (float)y),
				new PointF((float)x, (float)(y + ysize)),
				new PointF(pointF.X, pointF.Y)
			};
			glyphToWorld.TransformPoints(array);
			transform.TransformPoints(array);
			double num2 = Length(array[1], array[2]);
			double num3 = Length(array[1], array[3]);
			PointF pointF2 = new PointF(array[4].X, array[4].Y);
			pointF2.X -= drawR.X;
			pointF2.Y -= drawR.Y;
			double num4 = (double)angle * Math.PI / 180.0;
			pointF2.X += (float)((double)num / 2.0 * Math.Cos(num4));
			pointF2.Y += (float)((double)num / 2.0 * Math.Sin(num4));
			pointF2.X -= (float)(num2 / 2.0);
			pointF2.Y += (float)(num3 / 2.0);
			pointF2.Y = matTop - pointF2.Y;
			double num5 = 0.0;
			WriteFontName(fontName);
			tw.WriteLine("\t" + shiftState + " " + column + " " + row + " " + pointF2.X + " " + pointF2.Y + " " + num2 + " " + num3 + " " + num + " " + num5 + " " + (0f - angle) + " " + (welding ? "1" : "0") + " " + (flipShapes ? "1" : "0") + " 0");
		}

		internal void WriteEOF()
		{
			tw.WriteLine("EOF");
			tw.Flush();
			long num = 0L;
			if (thumbnail != null)
			{
				num = fs.Position;
				MemoryStream memoryStream = new MemoryStream();
				thumbnail.Save(memoryStream, ImageFormat.Png);
				int num2 = (int)memoryStream.Length;
				byte[] buffer = new byte[num2];
				memoryStream.Position = 0L;
				memoryStream.Read(buffer, 0, num2);
				fs.Write(buffer, 0, num2);
				if (writeDebugPNG)
				{
					FileStream fileStream = new FileStream(fs.Name + ".png", FileMode.Create);
					fileStream.Write(buffer, 0, num2);
					fileStream.Close();
				}
				memoryStream.Close();
				memoryStream.Dispose();
			}
			byte[] array = new byte[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = (byte)(num % 256);
				num /= 256;
			}
			fs.Write(array, 0, 4);
		}

		internal void Close()
		{
			tw.Close();
		}

		internal void SetThumbnail(Bitmap previewThumbnail)
		{
			thumbnail = previewThumbnail;
		}

		internal void WriteLayersStart()
		{
			tw.WriteLine();
			tw.WriteLine("layers");
		}

		internal void WriteLayersEnd()
		{
			tw.WriteLine();
		}

		internal void WriteLayer(string layerName, int layerId, bool visible)
		{
			string value = "\"" + layerName + "\" " + layerId + " " + (visible ? "1" : "0");
			tw.WriteLine(value);
		}
	}
}
