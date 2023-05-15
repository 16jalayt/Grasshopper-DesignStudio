using System.Windows.Forms;

namespace Cricut_Design_Studio
{
	public class FloatParam
	{
		public float f;

		public Control control;

		public object parent;

		public int mode;

		public FloatParam(float f, int mode)
		{
			this.f = f;
			this.mode = mode;
		}
	}
}
