using System;
using System.IO;
using System.Threading;

namespace Cricut_Design_Studio
{
	internal class UpdateCricutFirmware
	{
		public byte[] bytes;

		public byte[] read(string filename)
		{
			char[] array = new char[262144];
			int num = 0;
			try
			{
				StreamReader streamReader = new StreamReader(filename);
				num = streamReader.Read(array, 0, 262144);
				streamReader.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return null;
			}
			byte[] array2 = new byte[num / 2];
			for (int i = 0; i < num / 2; i++)
			{
				array2[i] = hex_to_char(array, i * 2);
			}
			bytes = array2;
			return array2;
		}

		public bool update(byte[] bytes)
		{
			int num = bytes.Length;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			USB_Interface uSB_Interface = new USB_Interface();
			if (!uSB_Interface.driverOkay || !Form1.myRootForm.openCricut(uSB_Interface, 14400))
			{
				return false;
			}
			USB_Interface.setTimeout(4000);
			for (int i = 0; i < num; i += num2)
			{
				int num5 = 0;
				num2 = ((bytes[i] << 8) | bytes[i + 1]) + 2;
				while ((num5 = uSB_Interface.getLength()) > 0)
				{
					for (int j = 0; j < num5; j++)
					{
						uSB_Interface.receiveChar();
					}
				}
				int percentProgress = (int)Math.Round((float)num3 / (float)num * 10000f);
				Form1.myRootForm.firmwareBackgroundWorker.ReportProgress(percentProgress);
				uSB_Interface.write(bytes, i, num2);
				int num6 = uSB_Interface.receiveChar();
				if (-1 == num6)
				{
					uSB_Interface.Close();
					return false;
				}
				switch (num6)
				{
				case 17:
					num3 += num2;
					num4 = 0;
					continue;
				}
				if (++num4 < 4)
				{
					i -= num2;
					continue;
				}
				uSB_Interface.Close();
				return false;
			}
			uSB_Interface.Close();
			Thread.Sleep(12000);
			return true;
		}

		private static byte decodeHexByte(char c)
		{
			switch (char.ToLower(c))
			{
			case '0':
				return 0;
			case '1':
				return 1;
			case '2':
				return 2;
			case '3':
				return 3;
			case '4':
				return 4;
			case '5':
				return 5;
			case '6':
				return 6;
			case '7':
				return 7;
			case '8':
				return 8;
			case '9':
				return 9;
			case 'a':
				return 10;
			case 'b':
				return 11;
			case 'c':
				return 12;
			case 'd':
				return 13;
			case 'e':
				return 14;
			case 'f':
				return 15;
			default:
				return 0;
			}
		}

		private static byte hex_to_char(char[] cs, int start)
		{
			return (byte)((decodeHexByte(cs[start]) << 4) | decodeHexByte(cs[start + 1]));
		}
	}
}
