namespace Cricut_Cartridge_Font_Converter
{
	public class FontKeyMap
	{
		public string label;

		public int[] map = new int[70];

		public FontKeyMap()
		{
			for (int i = 0; i < map.Length; i++)
			{
				map[i] = -1;
			}
		}
	}
}
