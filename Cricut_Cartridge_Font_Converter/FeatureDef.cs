namespace Cricut_Cartridge_Font_Converter
{
	public class FeatureDef
	{
		public string label;

		public string name;

		public int featureno;

		public int keycode;

		public int keycode2;

		public FeatureDef(string label, string name, int featureno)
		{
			this.label = label;
			this.name = name;
			this.featureno = featureno;
			keycode = -1;
			keycode2 = -1;
		}
	}
}
