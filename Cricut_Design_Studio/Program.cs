using System;
using System.Windows.Forms;
using SingleInstance;

namespace Cricut_Design_Studio
{
	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: false);
			SingleApplication.Run(new Form1());
		}
	}
}
