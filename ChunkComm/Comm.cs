using System;
using Cricut_Design_Studio;

namespace ChunkComm
{
	public class Comm : USB_Interface
	{
		public const int pageSize = 16;

		public Comm(int baud)
		{
			if (driverOkay)
			{
				Form1.myRootForm.openCricut(this, baud);
			}
		}

		public void dump()
		{
			while (getLength() > 0)
			{
				receiveChar();
			}
		}

		public void sendUShort(ushort u)
		{
			sendChar((byte)((uint)(u >> 8) & 0xFFu));
			sendChar((byte)(u & 0xFFu));
		}

		public ushort receiveUShort()
		{
			int num = receiveChar();
			int num2 = receiveChar();
			return (ushort)((num << 8) | num2);
		}

		public int receivePage(ref char[] data)
		{
			for (int i = 0; i < 16; i++)
			{
				data[i] = (char)receiveChar();
			}
			sendUShort(16);
			return 16;
		}

		public void sendPage(char[] data, int size, int start)
		{
			ushort num = 0;
			do
			{
				for (int i = 0; i < Math.Min(size, 16); i++)
				{
					sendChar((byte)data[start + i]);
				}
				for (int j = size; j < 16; j++)
				{
					sendChar(0);
				}
				num = receiveUShort();
			}
			while (num != 16);
		}
	}
}
