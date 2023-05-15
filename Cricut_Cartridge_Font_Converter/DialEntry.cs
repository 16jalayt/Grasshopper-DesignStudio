using System;

namespace Cricut_Cartridge_Font_Converter
{
	public class DialEntry
	{
		public string label;

		public int pos;

		public int ival;

		public float fval;

		public string getChunk(bool isFloat)
		{
			string stringChunk = Utils.getStringChunk(label);
			ushort num = 16;
			string text = "";
			num = (ushort)(num + (ushort)stringChunk.Length);
			text += Utils.ushort_to_hex(num);
			text += Utils.ushort_to_hex(17477);
			text += Utils.ushort_to_hex((ushort)pos);
			if (isFloat)
			{
				int num2 = (int)Math.Floor(fval);
				int num3 = (int)(((double)fval - Math.Floor(fval)) * 255.0);
				text += Utils.ushort_to_hex((ushort)((num2 << 8) | num3));
			}
			else
			{
				text += Utils.ushort_to_hex((ushort)ival);
			}
			return text + stringChunk;
		}
	}
}
