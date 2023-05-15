namespace Cricut_Cartridge_Font_Converter
{
	public class FontSizeSettings
	{
		public string label = "DIAL_FontSize";

		public DialEntry[] entries;

		public string getChunk()
		{
			string[] array = new string[entries.Length];
			ushort num = 12;
			for (int i = 0; i < entries.Length; i++)
			{
				array[i] = entries[i].getChunk(isFloat: true);
				num = (ushort)(num + (ushort)array[i].Length);
			}
			string text = "";
			text += Utils.ushort_to_hex(num);
			text += Utils.ushort_to_hex(21338);
			text += Utils.ushort_to_hex((ushort)entries.Length);
			for (int j = 0; j < array.Length; j++)
			{
				text += array[j];
			}
			return text;
		}
	}
}
