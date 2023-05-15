using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace Cricut_Design_Studio
{
	public class SceneNode
	{
		public SceneNode parent;

		public ArrayList children = new ArrayList();

		public virtual void draw(Graphics g)
		{
			foreach (SceneNode child in children)
			{
				child.parent = this;
				child.draw(g);
			}
		}

		public virtual bool wouldSelect(PointF mpnt, Matrix t)
		{
			bool flag = false;
			foreach (SceneNode child in children)
			{
				flag = flag || child.wouldSelect(mpnt, t);
			}
			return flag;
		}

		public virtual bool select(PointF mpnt, Matrix t)
		{
			bool flag = false;
			foreach (SceneNode child in children)
			{
				flag = flag || child.select(mpnt, t);
			}
			return flag;
		}

		public virtual void save(BinaryWriter bw)
		{
			foreach (SceneNode child in children)
			{
				child.parent = this;
				child.save(bw);
			}
		}

		public virtual void listCarts(ArrayList fontNames, ArrayList shapes)
		{
			foreach (SceneNode child in children)
			{
				child.listCarts(fontNames, shapes);
			}
		}

		public virtual void saveGypsy(GypsyWriter gw, int layerNo)
		{
			foreach (SceneNode child in children)
			{
				child.saveGypsy(gw, layerNo);
			}
		}
	}
}
