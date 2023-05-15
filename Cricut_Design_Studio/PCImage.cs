using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace Cricut_Design_Studio
{
	public class PCImage
	{
		public Image image;

		public Image thumbnail;

		public string name;

		public bool selected;

		public bool stretch;

		public float aspectRatio;

		public FloatParam positionX = new FloatParam(0f, 0);

		public FloatParam positionY = new FloatParam(0f, 0);

		public FloatParam width = new FloatParam(0f, 0);

		public FloatParam height = new FloatParam(0f, 0);

		public FloatParam angle = new FloatParam(0f, 0);

		public float oldX;

		public float oldY;

		public float oldWidth;

		public float oldHeight;

		public PCImage()
		{
			positionX.parent = this;
			positionY.parent = this;
			width.parent = this;
			height.parent = this;
			angle.parent = this;
		}

		public void save(BinaryWriter bw)
		{
			bw.Write("PCImage");
			bw.Write(name);
			bw.Write(selected);
			bw.Write(stretch);
			bw.Write(aspectRatio);
			bw.Write(positionX.f);
			bw.Write(positionY.f);
			bw.Write(width.f);
			bw.Write(height.f);
			bw.Write(angle.f);
		}

		public static PCImage read(BinaryReader br)
		{
			PCImage pCImage = new PCImage();
			pCImage.name = br.ReadString();
			pCImage.selected = br.ReadBoolean();
			pCImage.stretch = br.ReadBoolean();
			pCImage.aspectRatio = br.ReadSingle();
			pCImage.positionX.f = br.ReadSingle();
			pCImage.positionY.f = br.ReadSingle();
			pCImage.width.f = br.ReadSingle();
			pCImage.height.f = br.ReadSingle();
			pCImage.angle.f = br.ReadSingle();
			return pCImage;
		}

		public void assign(float size)
		{
			stretch = false;
			width.f = image.Width;
			height.f = image.Height;
			if (width.f > height.f)
			{
				height.f = height.f / width.f * size;
				width.f = size;
			}
			else
			{
				width.f = width.f / height.f * size;
				height.f = size;
			}
			aspectRatio = width.f / height.f;
			angle.f = 0f;
		}

		public bool select(PointF p, Matrix t)
		{
			float f = positionX.f;
			float f2 = positionY.f;
			float num = f + width.f;
			float num2 = f2 + height.f;
			return selected = f < p.X && p.X < num && f2 < p.Y && p.Y < num2;
		}

		public void transform(int handleTag, float hratio, float vratio, Matrix canvasToWorld, PointF mouseDownPnt, PointF mouseMovePnt)
		{
			SceneGroup.Transformers transformers = new SceneGroup.Transformers();
			Matrix modelingTransform = new Matrix();
			float num = mouseMovePnt.X - mouseDownPnt.X;
			float num2 = mouseMovePnt.Y - mouseDownPnt.Y;
			PointF[] array = new PointF[2] { mouseDownPnt, mouseMovePnt };
			canvasToWorld.TransformPoints(array);
			switch (handleTag)
			{
			case 1:
				transformers.calc(new PointF(positionX.f, positionY.f), array[1], array[0], modelingTransform);
				width.f = (float)Math.Round(oldWidth * transformers.mlen / transformers.hlen, 2);
				height.f = (float)Math.Round(oldHeight * transformers.mlen / transformers.hlen, 2);
				((TextBox)width.control).Text = width.f.ToString();
				((TextBox)height.control).Text = height.f.ToString();
				break;
			case 2:
				if (stretch)
				{
					transformers.calc(new PointF(positionX.f, positionY.f), array[1], array[0], modelingTransform);
					height.f = (float)Math.Round(oldHeight * transformers.mlen / transformers.hlen, 2);
					((TextBox)height.control).Text = height.f.ToString();
				}
				break;
			case 4:
				if (stretch)
				{
					transformers.calc(new PointF(positionX.f, positionY.f), array[1], array[0], modelingTransform);
					width.f = (float)Math.Round(oldWidth * transformers.mlen / transformers.hlen, 2);
					((TextBox)width.control).Text = width.f.ToString();
				}
				break;
			case 0:
			case 5:
				positionX.f = (float)Math.Round(oldX + num / hratio, 2);
				positionY.f = (float)Math.Round(oldY + num2 / vratio, 2);
				((TextBox)positionX.control).Text = positionX.f.ToString();
				((TextBox)positionY.control).Text = positionY.f.ToString();
				break;
			case 8:
				positionX.f = (float)Math.Round(oldX + num / hratio, 2);
				((TextBox)positionX.control).Text = positionX.f.ToString();
				break;
			case 6:
				positionY.f = (float)Math.Round(oldY + num2 / vratio, 2);
				((TextBox)positionY.control).Text = positionY.f.ToString();
				break;
			}
			aspectRatio = width.f / height.f;
		}

		public void draw(Graphics g)
		{
			g.DrawImage(image, positionX.f, positionY.f, width.f, height.f);
			if (selected)
			{
				Matrix modelingTransform = new Matrix();
				float f = positionX.f;
				float f2 = positionY.f;
				float num = f + width.f;
				float num2 = f2 + height.f;
				Pen pen = new Pen(Color.Black, 2.5f * Canvas.penScale);
				g.DrawRectangle(pen, positionX.f, positionY.f, width.f, height.f);
				float siz = (float)Math.Sqrt(width.f * width.f + height.f * height.f) / 7f;
				PointF cm = new PointF((f + num) / 2f, (f2 + num2) / 2f);
				PointF[] bbox = new PointF[4]
				{
					new PointF(f, num2),
					new PointF(num, num2),
					new PointF(num, f2),
					new PointF(f, f2)
				};
				int num3 = 354;
				if (stretch)
				{
					num3 |= 0x14;
				}
				SceneUtils.drawHandles(g, bbox, cm, siz, SceneGroup.dragging, SceneGroup.wmousePnt, modelingTransform, num3);
			}
		}
	}
}
