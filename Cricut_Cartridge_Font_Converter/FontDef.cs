using System;

namespace Cricut_Cartridge_Font_Converter
{
	public class FontDef
	{
		public string label;

		public int featureno;

		public string fontFilename;

		public FontKeyMap keymap;

		public string hexFilename;

		public FontDef(string label, int featureno, string keymapLabel)
		{
			this.label = label;
			this.featureno = featureno;
			keymap = new FontKeyMap();
			keymap.label = keymapLabel;
		}

		public string getChunk()
		{
			string stringChunk = Utils.getStringChunk(fontFilename);
			string byteArrayChunk = Utils.getByteArrayChunk(keymap.map);
			if (stringChunk == null)
			{
				Console.WriteLine("Font file name missing!");
				return null;
			}
			ushort num = 12;
			string text = "";
			num = (ushort)(num + (ushort)stringChunk.Length);
			num = (ushort)(num + (ushort)byteArrayChunk.Length);
			text += Utils.ushort_to_hex(num);
			text += Utils.ushort_to_hex(17990);
			text += Utils.ushort_to_hex((ushort)featureno);
			text += stringChunk;
			return text + byteArrayChunk;
		}
	}
}
