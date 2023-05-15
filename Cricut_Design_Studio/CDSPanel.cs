using System.Drawing;
using System.Windows.Forms;

namespace Cricut_Design_Studio
{
	internal class CDSPanel : Panel
	{
		protected override Point ScrollToControl(Control activeControl)
		{
			return base.AutoScrollPosition;
		}
	}
}
