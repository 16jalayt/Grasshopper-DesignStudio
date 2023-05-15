namespace Cricut_Cartridge_Font_Converter
{
	public class Utils
	{
		public const ushort ChunkId_String = 21332;

		public const ushort ChunkId_ByteArray = 16706;

		public const ushort ChunkId_UShortArray = 16725;

		public const ushort ChunkId_DialEntry = 17477;

		public const ushort ChunkId_Pressure = 20562;

		public const ushort ChunkId_Speed = 21328;

		public const ushort ChunkId_FontSize = 21338;

		public const ushort ChunkId_FontDef = 17990;

		public const ushort ChunkId_Features = 18004;

		public const ushort ChunkId_GlyphNames = 18254;

		public const ushort ChunkId_GlyphNamesShift = 18259;

		public const ushort ChunkId_Glyph = 18252;

		public const ushort ChunkId_GlyphTable = 18260;

		public const ushort ChunkId_Programmed = 20560;

		public const ushort ChunkId_CartHeader = 17224;

		public const ushort ChunkId_ClearChunks = 17219;

		public const ushort ChunkId_HexPkg = 18512;

		public const ushort ChunkId_HexPkgFwd = 18514;

		public const ushort ChunkId_BinPkg = 16976;

		public const ushort ChunkId_BinPkgFwd = 16978;

		public const ushort ChunkId_ReqPkg = 21072;

		public const ushort ChunkId_Okay = 20299;

		public const ushort ChunkId_GetGlyph = 18247;

		public const ushort ChunkId_PcControl = 20547;

		public static byte decodeHexByte(char c)
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

		public static string byte_to_hex(byte b)
		{
			return b.ToString("X").PadLeft(2, '0');
		}

		public static string ushort_to_hex(ushort u)
		{
			return u.ToString("X").PadLeft(4, '0');
		}

		public static string uint_to_hex(uint u)
		{
			return u.ToString("X").PadLeft(8, '0');
		}

		public static byte hex_to_byte(char[] cs, int start)
		{
			return (byte)((decodeHexByte(cs[start]) << 4) | decodeHexByte(cs[start + 1]));
		}

		public static ushort hex_to_ushort(string s)
		{
			return hex_to_ushort(s.ToCharArray());
		}

		public static ushort hex_to_ushort(char[] cs)
		{
			return hex_to_ushort(cs, 0);
		}

		public static ushort hex_to_ushort(char[] cs, int start)
		{
			return (ushort)((decodeHexByte(cs[start]) << 12) | (decodeHexByte(cs[start + 1]) << 8) | (decodeHexByte(cs[start + 2]) << 4) | decodeHexByte(cs[start + 3]));
		}

		public static short hex_to_short(char[] cs, int start)
		{
			return (short)((decodeHexByte(cs[start]) << 12) | (decodeHexByte(cs[start + 1]) << 8) | (decodeHexByte(cs[start + 2]) << 4) | decodeHexByte(cs[start + 3]));
		}

		public static string fixupString(string s)
		{
			if (s == null || "" == s)
			{
				return null;
			}
			if ('"' == s[0])
			{
				s = s.Remove(0, 1);
			}
			if ('"' == s[s.Length - 1])
			{
				s = s.Remove(s.Length - 1, 1);
			}
			char[] anyOf = new char[1] { '\\' };
			int startIndex;
			while ((startIndex = s.IndexOfAny(anyOf)) != -1)
			{
				s = s.Remove(startIndex, 1);
			}
			if ('\'' == s[0])
			{
				s = s.Remove(0, 1);
			}
			if ('\'' == s[s.Length - 1])
			{
				s = s.Remove(s.Length - 1, 1);
			}
			return s;
		}

		public static string getStringChunk(string s)
		{
			if (s == null)
			{
				return null;
			}
			char[] array = s.ToCharArray();
			ushort u = (ushort)(2 * array.Length + 4 + 4);
			string text = "";
			text += ushort_to_hex(u);
			text += ushort_to_hex(21332);
			char[] array2 = array;
			foreach (char c in array2)
			{
				text += byte_to_hex((byte)c);
			}
			return text;
		}

		public static string getByteArrayChunk(int[] array)
		{
			ushort u = (ushort)(2 * array.Length + 4 + 4);
			string text = "";
			text += ushort_to_hex(u);
			text += ushort_to_hex(16706);
			foreach (int num in array)
			{
				text += byte_to_hex((byte)num);
			}
			return text;
		}

		public static string getUShortArrayChunk(int[] array)
		{
			ushort u = (ushort)(4 * array.Length + 4 + 4);
			string text = "";
			text += ushort_to_hex(u);
			text += ushort_to_hex(16725);
			foreach (int num in array)
			{
				text += ushort_to_hex((ushort)num);
			}
			return text;
		}

		public static string assembleChunks(ushort id, string[] chunks)
		{
			string text = "";
			int num = 8;
			foreach (string text2 in chunks)
			{
				if (text2 != null)
				{
					num += text2.Length;
				}
			}
			text += ushort_to_hex((ushort)num);
			text += ushort_to_hex(id);
			foreach (string text3 in chunks)
			{
				if (text3 != null)
				{
					text += text3;
				}
			}
			return text;
		}

		public static ushort decode_ushort(char[] cs)
		{
			return decode_ushort(cs, 0);
		}

		public static ushort decode_ushort(char[] cs, int start)
		{
			int num = 0;
			for (int i = 0; i < 4; i++)
			{
				num = (num << 4) | decodeHexByte(cs[start + i]);
			}
			return (ushort)num;
		}

		public static string decode_string(char[] cs, int start)
		{
			int num = decode_ushort(cs, start);
			int num2 = decode_ushort(cs, start + 4);
			if (num2 != 21332)
			{
				return null;
			}
			char[] array = new char[(num - 8) / 2];
			for (int i = 0; i < (num - 8) / 2; i++)
			{
				array[i] = (char)hex_to_byte(cs, start + 8 + i * 2);
			}
			return new string(array);
		}
	}
}
