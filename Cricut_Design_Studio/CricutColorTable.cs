using System.Drawing;
using System.Windows.Forms;

namespace Cricut_Design_Studio
{
	public class CricutColorTable : ProfessionalColorTable
	{
		private static Color _contextMenuBack = Color.FromArgb(250, 250, 250);

		private static Color _buttonPressedBegin = Color.FromArgb(248, 181, 106);

		private static Color _buttonPressedEnd = Color.FromArgb(255, 208, 134);

		private static Color _buttonPressedMiddle = Color.FromArgb(251, 140, 60);

		private static Color _buttonSelectedBegin = Color.FromArgb(255, 255, 222);

		private static Color _buttonSelectedEnd = Color.FromArgb(255, 203, 136);

		private static Color _buttonSelectedMiddle = Color.FromArgb(255, 225, 172);

		private static Color _menuItemSelectedBegin = Color.FromArgb(255, 213, 103);

		private static Color _menuItemSelectedEnd = Color.FromArgb(255, 228, 145);

		private static Color _checkBack = Color.FromArgb(227, 213, 158);

		private static Color _gripDark = Color.FromArgb(111, 157, 217);

		private static Color _gripLight = Color.FromArgb(255, 255, 255);

		private static Color _imageMargin = Color.FromArgb(227, 213, 158);

		private static Color _menuBorder = Color.FromArgb(134, 134, 134);

		private static Color _overflowBegin = Color.FromArgb(167, 204, 251);

		private static Color _overflowEnd = Color.FromArgb(101, 147, 207);

		private static Color _overflowMiddle = Color.FromArgb(167, 204, 251);

		private static Color _menuToolBack = Color.FromArgb(182, 215, 154);

		private static Color _separatorDark = Color.FromArgb(154, 198, 255);

		private static Color _separatorLight = Color.FromArgb(255, 255, 255);

		private static Color _statusStripLight = Color.FromArgb(215, 229, 247);

		private static Color _statusStripDark = Color.FromArgb(172, 201, 238);

		private static Color _toolStripBorder = Color.FromArgb(111, 157, 217);

		private static Color _toolStripContentEnd = Color.FromArgb(164, 195, 235);

		private static Color _toolStripBegin = Color.FromArgb(227, 239, 255);

		private static Color _toolStripEnd = Color.FromArgb(152, 186, 230);

		private static Color _toolStripMiddle = Color.FromArgb(222, 236, 255);

		private static Color _buttonBorder = Color.FromArgb(121, 153, 194);

		public override Color ButtonPressedGradientBegin => _buttonPressedBegin;

		public override Color ButtonPressedGradientEnd => _buttonPressedEnd;

		public override Color ButtonPressedGradientMiddle => _buttonPressedMiddle;

		public override Color ButtonSelectedGradientBegin => _buttonSelectedBegin;

		public override Color ButtonSelectedGradientEnd => _buttonSelectedEnd;

		public override Color ButtonSelectedGradientMiddle => _buttonSelectedMiddle;

		public override Color ButtonSelectedHighlightBorder => _buttonBorder;

		public override Color CheckBackground => _checkBack;

		public override Color GripDark => _gripDark;

		public override Color GripLight => _gripLight;

		public override Color ImageMarginGradientBegin => _imageMargin;

		public override Color MenuBorder => _menuBorder;

		public override Color MenuItemPressedGradientBegin => _toolStripBegin;

		public override Color MenuItemPressedGradientEnd => _toolStripEnd;

		public override Color MenuItemPressedGradientMiddle => _toolStripMiddle;

		public override Color MenuItemSelectedGradientBegin => _menuItemSelectedBegin;

		public override Color MenuItemSelectedGradientEnd => _menuItemSelectedEnd;

		public override Color MenuStripGradientBegin => _menuToolBack;

		public override Color MenuStripGradientEnd => _menuToolBack;

		public override Color OverflowButtonGradientBegin => _overflowBegin;

		public override Color OverflowButtonGradientEnd => _overflowEnd;

		public override Color OverflowButtonGradientMiddle => _overflowMiddle;

		public override Color RaftingContainerGradientBegin => _menuToolBack;

		public override Color RaftingContainerGradientEnd => _menuToolBack;

		public override Color SeparatorDark => _separatorDark;

		public override Color SeparatorLight => _separatorLight;

		public override Color StatusStripGradientBegin => _statusStripLight;

		public override Color StatusStripGradientEnd => _statusStripDark;

		public override Color ToolStripBorder => _toolStripBorder;

		public override Color ToolStripContentPanelGradientBegin => _toolStripContentEnd;

		public override Color ToolStripContentPanelGradientEnd => _menuToolBack;

		public override Color ToolStripDropDownBackground => _contextMenuBack;

		public override Color ToolStripGradientBegin => _toolStripBegin;

		public override Color ToolStripGradientEnd => _toolStripEnd;

		public override Color ToolStripGradientMiddle => _toolStripMiddle;

		public override Color ToolStripPanelGradientBegin => _menuToolBack;

		public override Color ToolStripPanelGradientEnd => _menuToolBack;
	}
}
