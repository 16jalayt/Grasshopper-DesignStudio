using System;
using System.Collections;
using Cricut_Design_Studio;

namespace Cricut_Cartridge_Font_Converter
{
	public class Header
	{
		public class CartHeader
		{
			public string fontName;

			public int fontId;

			public int fontVersion;

			public int firmwareVersion;

			public int keyHeightChar;

			public int keyHeightCode;

			public int spaceKeyCode;

			public string getChunk()
			{
				string stringChunk = Utils.getStringChunk(fontName);
				ushort num = 32;
				string text = "";
				num = (ushort)(num + (ushort)stringChunk.Length);
				text += Utils.ushort_to_hex(num);
				text += Utils.ushort_to_hex(17224);
				text += Utils.ushort_to_hex((ushort)fontId);
				text += Utils.ushort_to_hex((ushort)fontVersion);
				text += Utils.ushort_to_hex(0);
				text += Utils.ushort_to_hex((ushort)keyHeightChar);
				text += Utils.ushort_to_hex((ushort)keyHeightCode);
				text += Utils.ushort_to_hex((ushort)spaceKeyCode);
				return text + stringChunk;
			}
		}

		public class GlyphNames
		{
			public string[] names = new string[70];

			public string getChunk(ushort id)
			{
				int[] array = new int[560];
				int num = 0;
				string[] array2 = names;
				foreach (string text in array2)
				{
					for (int j = 0; j < 8; j++)
					{
						array[num + j] = 0;
					}
					if (text != null)
					{
						for (int k = 0; k < Math.Min(text.Length, 8); k++)
						{
							array[num + k] = text[k];
						}
					}
					num += 8;
				}
				string byteArrayChunk = Utils.getByteArrayChunk(array);
				ushort u = (ushort)(8 + byteArrayChunk.Length);
				string text2 = "";
				text2 += Utils.ushort_to_hex(u);
				text2 += Utils.ushort_to_hex(id);
				return text2 + byteArrayChunk;
			}
		}

		private const int NoneCode = -1;

		public FeatureDef[] featureDefs = new FeatureDef[23]
		{
			new FeatureDef("KEY_FontOption0", "Font 0", 128),
			new FeatureDef("KEY_FontOption1", "Font 1", 129),
			new FeatureDef("KEY_FontOption2", "Font 2", 130),
			new FeatureDef("KEY_FontOption3", "Font 3", 131),
			new FeatureDef("KEY_FontOption4", "Font 4", 132),
			new FeatureDef("KEY_FontOption5", "Font 5", 133),
			new FeatureDef("KEY_FontOption6", "Font 6", 134),
			new FeatureDef("KEY_FontOption7", "Font 7", 135),
			new FeatureDef("KEY_FontOption8", "Font 8", 136),
			new FeatureDef("KEY_FontOption9", "Font 9", 137),
			new FeatureDef("KEY_PaperSaver", "PaprSav", 138),
			new FeatureDef("KEY_RealSizing", "RealSize", 139),
			new FeatureDef("KEY_Shift", "Shift", 140),
			new FeatureDef("KEY_CapsLock", "CapsLock", 141),
			new FeatureDef("KEY_Backspace", "Bkspace", 142),
			new FeatureDef("KEY_Clear", "Clear", 143),
			new FeatureDef("KEY_RepeatLast", "RptLast", 144),
			new FeatureDef("KEY_SetPaperSize", "SetPrSiz", 145),
			new FeatureDef("KEY_UnloadPaper", "UnloadPr", 146),
			new FeatureDef("KEY_LoadPaper", "LoadPapr", 147),
			new FeatureDef("KEY_LoadLast", "LoadLast", 148),
			new FeatureDef("KEY_ResetAll", "ResetAll", 149),
			new FeatureDef("KEY_SoundOnOff", "Sound1/0", 150)
		};

		public FontDef[] fontDefs = new FontDef[22]
		{
			new FontDef("FONT_Standard", 0, "FONT_StandardMap"),
			new FontDef("FONT_StandardShift", 1, "FONT_StandardShiftMap"),
			new FontDef("FONT_Option_0", 2, "FONT_Option_0_Map"),
			new FontDef("FONT_Option_0_Shift", 3, "FONT_Option_0_ShiftMap"),
			new FontDef("FONT_Option_1", 4, "FONT_Option_1_Map"),
			new FontDef("FONT_Option_1_Shift", 5, "FONT_Option_1_ShiftMap"),
			new FontDef("FONT_Option_2", 6, "FONT_Option_2_Map"),
			new FontDef("FONT_Option_2_Shift", 7, "FONT_Option_2_ShiftMap"),
			new FontDef("FONT_Option_3", 8, "FONT_Option_3_Map"),
			new FontDef("FONT_Option_3_Shift", 9, "FONT_Option_3_ShiftMap"),
			new FontDef("FONT_Option_4", 10, "FONT_Option_4_Map"),
			new FontDef("FONT_Option_4_Shift", 11, "FONT_Option_4_ShiftMap"),
			new FontDef("FONT_Option_5", 12, "FONT_Option_5_Map"),
			new FontDef("FONT_Option_5_Shift", 13, "FONT_Option_5_ShiftMap"),
			new FontDef("FONT_Option_6", 14, "FONT_Option_6_Map"),
			new FontDef("FONT_Option_6_Shift", 15, "FONT_Option_6_ShiftMap"),
			new FontDef("FONT_Option_7", 16, "FONT_Option_7_Map"),
			new FontDef("FONT_Option_7_Shift", 17, "FONT_Option_7_ShiftMap"),
			new FontDef("FONT_Option_8", 18, "FONT_Option_8_Map"),
			new FontDef("FONT_Option_8_Shift", 19, "FONT_Option_8_ShiftMap"),
			new FontDef("FONT_Option_9", 20, "FONT_Option_9_Map"),
			new FontDef("FONT_Option_9_Shift", 21, "FONT_Option_9_ShiftMap")
		};

		public PressureSettings pressureSettings = new PressureSettings();

		public SpeedSettings speedSettings = new SpeedSettings();

		public FontSizeSettings fontSizes = new FontSizeSettings();

		public char[] whitespace = new char[5] { ',', ' ', '\t', '\r', '\n' };

		public CartHeader cartHeader = new CartHeader();

		public GlyphNames glyphNames = new GlyphNames();

		public GlyphNames glyphNamesShift = new GlyphNames();

		public string nextArg(ref string s)
		{
			s = s.Trim(whitespace);
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			char[] array = s.ToCharArray();
			string text = "";
			string text2 = "";
			char[] array2 = array;
			foreach (char c in array2)
			{
				bool flag5 = false;
				if (!flag3 && !flag2)
				{
					if (' ' == c || '\t' == c || '\r' == c || '\n' == c || ',' == c)
					{
						flag = true;
					}
				}
				else if ('\\' == c)
				{
					flag5 = true;
				}
				if (flag)
				{
					text2 += c;
					continue;
				}
				if (!flag4 && !flag3 && '"' == c)
				{
					flag2 = !flag2;
				}
				if (!flag4 && !flag2 && '\'' == c)
				{
					flag3 = !flag3;
				}
				text += c;
				flag4 = flag5;
			}
			s = text2;
			return text;
		}

		public string nextArg(string s)
		{
			return nextArg(ref s);
		}

		public ArrayList readSection(string s, MyStream sr)
		{
			int num = 0;
			char[] array = s.ToCharArray(0, s.Length);
			bool flag = false;
			char[] array2 = array;
			foreach (char c in array2)
			{
				if ('\'' == c)
				{
					flag = !flag;
				}
				if ('{' == c && !flag)
				{
					num++;
					break;
				}
			}
			if (num == 0)
			{
				return null;
			}
			ArrayList arrayList = new ArrayList();
			string text = "";
			string text2;
			while ((text2 = sr.ReadLine()) != null && num > 0)
			{
				array = text2.ToCharArray(0, text2.Length);
				char[] array3 = array;
				foreach (char c2 in array3)
				{
					if ('\'' == c2)
					{
						flag = !flag;
					}
					if ('{' == c2 && !flag)
					{
						num++;
					}
					else if ('}' == c2 && !flag)
					{
						if (text.Length > 0)
						{
							arrayList.Add(text);
						}
						text = "";
						num--;
					}
					else if (num > 1)
					{
						text += c2;
					}
				}
			}
			return arrayList;
		}

		public bool parseLine(string line, MyStream sr)
		{
			line = line.Trim();
			char[] array = line.ToCharArray(0, line.Length);
			if (array.Length < 1 || '#' == array[0])
			{
				return false;
			}
			int num = line.IndexOfAny(whitespace);
			if (-1 == num)
			{
				return true;
			}
			string text = line.Substring(0, num);
			string s = line.Substring(num);
			if ("CART_FontName" == text)
			{
				cartHeader.fontName = nextArg(s);
				cartHeader.fontName = Utils.fixupString(cartHeader.fontName);
				return true;
			}
			if ("CART_FontID" == text)
			{
				cartHeader.fontId = int.Parse(nextArg(s));
				return true;
			}
			if ("CART_FontVersion" == text)
			{
				cartHeader.fontVersion = int.Parse(nextArg(s));
				return true;
			}
			if ("CART_KeyHeightChar" == text)
			{
				string text2 = nextArg(s);
				if ('\'' == text2[0])
				{
					cartHeader.keyHeightChar = text2[1];
				}
				else
				{
					cartHeader.keyHeightChar = int.Parse(text2);
				}
				return true;
			}
			if ("GLYPH_NameMap" == text || "GLYPH_NameShiftMap" == text)
			{
				ArrayList arrayList = readSection(s, sr);
				if (arrayList != null)
				{
					for (int i = 0; i < arrayList.Count; i++)
					{
						string s2 = (string)arrayList[i];
						int num2 = int.Parse(nextArg(ref s2)) - 1;
						string s3 = nextArg(ref s2);
						if (num2 != -1)
						{
							if ("GLYPH_NameMap" == text)
							{
								glyphNames.names[num2] = Utils.fixupString(s3);
							}
							else
							{
								glyphNamesShift.names[num2] = Utils.fixupString(s3);
							}
						}
					}
				}
			}
			FeatureDef[] array2 = featureDefs;
			foreach (FeatureDef featureDef in array2)
			{
				if (featureDef.label == text)
				{
					string text3 = nextArg(s);
					int num3 = -255;
					num3 = ((!("NONE" == text3)) ? int.Parse(text3) : (-1));
					if (featureDef.keycode != -1)
					{
						featureDef.keycode2 = num3;
					}
					else
					{
						featureDef.keycode = num3;
					}
					break;
				}
			}
			FontDef[] array3 = fontDefs;
			foreach (FontDef fontDef in array3)
			{
				if (fontDef.label == text)
				{
					fontDef.fontFilename = nextArg(s);
					fontDef.fontFilename = Utils.fixupString(fontDef.fontFilename);
					break;
				}
				if (!(fontDef.keymap.label == text))
				{
					continue;
				}
				string text4 = "";
				ArrayList arrayList2 = readSection(s, sr);
				if (arrayList2 == null)
				{
					text4 = nextArg(s);
					FontDef[] array4 = fontDefs;
					foreach (FontDef fontDef2 in array4)
					{
						if (fontDef2.keymap.label == text4)
						{
							fontDef.keymap.map = fontDef2.keymap.map;
							break;
						}
					}
					break;
				}
				for (int m = 0; m < arrayList2.Count; m++)
				{
					string s4 = (string)arrayList2[m];
					int num4 = int.Parse(nextArg(ref s4)) - 1;
					string text5 = nextArg(ref s4);
					if (int.TryParse(text5, out var result))
					{
						fontDef.keymap.map[num4] = result;
						continue;
					}
					char[] array5 = text5.ToCharArray();
					result = array5[1];
					fontDef.keymap.map[num4] = result;
				}
				break;
			}
			if (speedSettings.label == text)
			{
				ArrayList arrayList3 = readSection(s, sr);
				speedSettings.entries = new DialEntry[arrayList3.Count];
				for (int n = 0; n < arrayList3.Count; n++)
				{
					string s5 = (string)arrayList3[n];
					speedSettings.entries[n] = new DialEntry();
					speedSettings.entries[n].pos = int.Parse(nextArg(ref s5));
					speedSettings.entries[n].label = nextArg(ref s5);
					speedSettings.entries[n].label = Utils.fixupString(speedSettings.entries[n].label);
					speedSettings.entries[n].ival = int.Parse(nextArg(ref s5));
				}
			}
			else if (pressureSettings.label == text)
			{
				ArrayList arrayList4 = readSection(s, sr);
				pressureSettings.entries = new DialEntry[arrayList4.Count];
				for (int num5 = 0; num5 < arrayList4.Count; num5++)
				{
					string s6 = (string)arrayList4[num5];
					pressureSettings.entries[num5] = new DialEntry();
					pressureSettings.entries[num5].pos = int.Parse(nextArg(ref s6));
					pressureSettings.entries[num5].label = nextArg(ref s6);
					pressureSettings.entries[num5].label = Utils.fixupString(pressureSettings.entries[num5].label);
					pressureSettings.entries[num5].ival = int.Parse(nextArg(ref s6));
				}
			}
			else if (fontSizes.label == text)
			{
				ArrayList arrayList5 = readSection(s, sr);
				fontSizes.entries = new DialEntry[arrayList5.Count];
				for (int num6 = 0; num6 < arrayList5.Count; num6++)
				{
					string s7 = (string)arrayList5[num6];
					fontSizes.entries[num6] = new DialEntry();
					fontSizes.entries[num6].pos = int.Parse(nextArg(ref s7));
					fontSizes.entries[num6].label = nextArg(ref s7);
					fontSizes.entries[num6].label = Utils.fixupString(fontSizes.entries[num6].label);
					fontSizes.entries[num6].fval = float.Parse(nextArg(ref s7));
				}
			}
			return true;
		}

		public Header(string filename)
		{
			try
			{
				MyStream myStream = new MyStream(filename);
				string line;
				while ((line = myStream.ReadLine()) != null)
				{
					parseLine(line, myStream);
				}
				myStream.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine("The file could not be read:");
				Console.WriteLine(ex.Message);
			}
		}

		public Header()
		{
		}

		public string getFeaturesChunk()
		{
			int[] array = new int[70];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = -1;
			}
			FeatureDef[] array2 = featureDefs;
			foreach (FeatureDef featureDef in array2)
			{
				if (featureDef.keycode >= 0)
				{
					if (featureDef.keycode >= 0)
					{
						array[featureDef.keycode - 1] = featureDef.featureno;
					}
					if (featureDef.keycode2 >= 0)
					{
						array[featureDef.keycode2 - 1] = featureDef.featureno;
					}
				}
			}
			string byteArrayChunk = Utils.getByteArrayChunk(array);
			ushort num = 8;
			string text = "";
			num = (ushort)(num + (ushort)byteArrayChunk.Length);
			text += Utils.ushort_to_hex(num);
			text += Utils.ushort_to_hex(18004);
			return text + byteArrayChunk;
		}

		public string getChunk(ushort pkgid)
		{
			int num = fontDefs.Length + 7;
			string[] array = new string[num];
			num = 0;
			array[num++] = cartHeader.getChunk();
			array[num++] = getFeaturesChunk();
			array[num++] = glyphNames.getChunk(18254);
			array[num++] = glyphNamesShift.getChunk(18259);
			string chunk = pressureSettings.getChunk();
			string chunk2 = speedSettings.getChunk();
			string chunk3 = fontSizes.getChunk();
			if (chunk != null)
			{
				array[num++] = chunk;
			}
			if (chunk2 != null)
			{
				array[num++] = chunk2;
			}
			if (chunk3 != null)
			{
				array[num++] = chunk3;
			}
			return Utils.assembleChunks(pkgid, array);
		}
	}
}
