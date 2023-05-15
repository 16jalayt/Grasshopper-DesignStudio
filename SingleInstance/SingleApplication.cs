using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace SingleInstance
{
	public class SingleApplication
	{
		private const int SW_RESTORE = 9;

		private static bool alreadyChecked;

		private static bool alreadyRunning;

		private static Mutex mutex;

		[DllImport("user32.dll")]
		private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		private static extern int SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern int IsIconic(IntPtr hWnd);

		private static IntPtr GetCurrentInstanceWindowHandle()
		{
			IntPtr result = IntPtr.Zero;
			Process currentProcess = Process.GetCurrentProcess();
			Process[] processesByName = Process.GetProcessesByName(currentProcess.ProcessName);
			Process[] array = processesByName;
			foreach (Process process in array)
			{
				if (process.Id != currentProcess.Id && process.MainModule.FileName == currentProcess.MainModule.FileName && process.MainWindowHandle != IntPtr.Zero)
				{
					result = process.MainWindowHandle;
					break;
				}
			}
			return result;
		}

		public static void SwitchToCurrentInstance()
		{
			IntPtr currentInstanceWindowHandle = GetCurrentInstanceWindowHandle();
			if (currentInstanceWindowHandle != IntPtr.Zero)
			{
				if (IsIconic(currentInstanceWindowHandle) != 0)
				{
					ShowWindow(currentInstanceWindowHandle, 9);
				}
				SetForegroundWindow(currentInstanceWindowHandle);
			}
		}

		public static bool Run(Form frmMain)
		{
			if ((alreadyChecked && alreadyRunning) || (!alreadyChecked && IsAlreadyRunning()))
			{
				SwitchToCurrentInstance();
				return false;
			}
			Application.Run(frmMain);
			return true;
		}

		public static bool Run()
		{
			if (IsAlreadyRunning())
			{
				return false;
			}
			return true;
		}

		public static bool IsAlreadyRunning()
		{
			string location = Assembly.GetExecutingAssembly().Location;
			FileSystemInfo fileSystemInfo = new FileInfo(location);
			string name = fileSystemInfo.Name;
			mutex = new Mutex(initiallyOwned: true, "Global\\" + name, out var createdNew);
			if (createdNew)
			{
				mutex.ReleaseMutex();
			}
			alreadyChecked = true;
			alreadyRunning = !createdNew;
			return !createdNew;
		}
	}
}
