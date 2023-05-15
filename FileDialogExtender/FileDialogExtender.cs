using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FileDialogExtender
{
	public class FileDialogExtender
	{
		public enum DialogViewTypes
		{
			Icons = 28713,
			List = 28715,
			Details = 28716,
			Thumbnails = 28717,
			Tiles = 28718
		}

		private const uint WM_COMMAND = 273u;

		private uint _lastDialogHandle;

		private DialogViewTypes _viewType;

		private bool _enabled;

		public DialogViewTypes DialogViewType
		{
			get
			{
				return _viewType;
			}
			set
			{
				_viewType = value;
			}
		}

		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				_enabled = value;
			}
		}

		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "SendMessageA")]
		private static extern uint SendMessage(uint Hdc, uint Msg_Const, uint wParam, uint lParam);

		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "FindWindowExA")]
		private static extern uint FindWindowEx(uint hwndParent, uint hwndChildAfter, string lpszClass, string lpszWindow);

		public FileDialogExtender()
			: this(DialogViewTypes.List, enabled: false)
		{
		}

		public FileDialogExtender(DialogViewTypes viewType)
			: this(viewType, enabled: false)
		{
		}

		public FileDialogExtender(DialogViewTypes viewType, bool enabled)
		{
			_viewType = viewType;
			Enabled = enabled;
		}

		public void WndProc(ref Message m)
		{
			if (_enabled && m.Msg == 289)
			{
				uint num = (uint)(int)m.LParam;
				if (num != _lastDialogHandle)
				{
					uint hdc = FindWindowEx(num, 0u, "SHELLDLL_DefView", "");
					SendMessage(hdc, 273u, (uint)_viewType, 0u);
					_lastDialogHandle = num;
				}
			}
		}
	}
}
