using System;
using Cricut_Cartridge_Font_Converter;

namespace ChunkComm
{
	public class ChunkPkg
	{
		public static bool sendPrefix(Comm comm, ushort pkgid, ushort siz)
		{
			comm.sendUShort(siz);
			comm.sendUShort(pkgid);
			ushort num = comm.receiveUShort();
			if (num != siz)
			{
				return false;
			}
			return true;
		}

		public static bool waitForAck(Comm comm, ushort pkgid)
		{
			ushort num = comm.receiveUShort();
			if (num != pkgid)
			{
				return false;
			}
			return true;
		}

		public static bool sendHexPkg(Comm comm, string chunk)
		{
			char[] array = chunk.ToCharArray();
			int num = array.Length;
			int num2 = (num + 15) / 16;
			sendPrefix(comm, 18512, (ushort)num);
			for (int i = 0; i < num2; i++)
			{
				int size = Math.Min(16, num - i * 16);
				comm.sendPage(array, size, i * 16);
			}
			waitForAck(comm, 18512);
			return true;
		}

		public static int sendBinPkg(Comm comm, string chunk, bool fwd)
		{
			int num = 0;
			char[] array = new char[16];
			int num2 = chunk.Length / 2;
			int num3 = (num2 + 15) / 16;
			if (!((!fwd) ? sendPrefix(comm, 16976, (ushort)num2) : sendPrefix(comm, 16978, (ushort)num2)))
			{
				return -1;
			}
			for (int i = 0; i < num3; i++)
			{
				int num4 = Math.Min(16, num2 - i * 16);
				for (int j = 0; j < num4; j++)
				{
					string text = chunk.Substring((i * 16 + j) * 2, 2);
					array[j] = (char)Utils.hex_to_byte(text.ToCharArray(), 0);
				}
				comm.sendPage(array, num4, 0);
				num += num4;
			}
			waitForAck(comm, 16976);
			return num;
		}

		public static char[] receiveBinPkg(Comm comm)
		{
			ushort num = comm.receiveUShort();
			if (ushort.MaxValue == num)
			{
				return null;
			}
			ushort u = comm.receiveUShort();
			comm.sendUShort(num);
			char[] data = new char[16];
			char[] array = new char[num];
			int num2 = (num + 15) / 16;
			int i = 0;
			int num3 = 0;
			for (; i < num2; i++)
			{
				int num4 = Math.Min(16, num - i * 16);
				comm.receivePage(ref data);
				for (int j = 0; j < num4; j++)
				{
					array[num3++] = data[j];
				}
			}
			comm.sendUShort(u);
			return array;
		}
	}
}
