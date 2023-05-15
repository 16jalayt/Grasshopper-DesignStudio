namespace Cricut_Design_Studio
{
	public class GlyphTable
	{
		public string fontName;

		public float scaling;

		public int nGlyphs;

		public Glyph[] glyphs;

		public GlyphTable(int n)
		{
			glyphs = new Glyph[n];
		}
	}
}
