using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace Cricut_Design_Studio
{
	public class FontLoading
	{
		public enum FontLibraryMode
		{
			Favorites,
			All_by_Category,
			My_Cartridges,
			Keywords,
			This_Project
		}

		public const int fontIconSize = 20;

		public ArrayList shapes;

		public Shape selectedShape;

		public int FontId;

		public int GlyphIndex = -1;

		public TagDictionary tagDictionary = new TagDictionary();

		public FontMetadata fontMetadata;

		public Bitmap matteImage;

		public Bitmap whiteKeyBitmap;

		public Bitmap whiteKeyBitmap_hot;

		public Bitmap grayKeyBitmap;

		public Bitmap grayKeyBitmap_hot;

		public static Matrix worldToCanvas = new Matrix();

		public static Matrix canvasToWorld = new Matrix();

		public static float penScale;

		private float glyphInFontListFitSize = 0.8f;

		private float glyphInMenuFitSize = 0.9f;

		public float glyphOnKeyFitSize = 0.85f;

		private int glyphOnKeyRasterSize = 140;

		private static Hashtable fontDisplayNames;

		private bool onlyOnce;

		private Pen blackPen = new Pen(Color.Black, 1f);

		public FontLoading()
		{
			whiteKeyBitmap = MarcusResources.btn_32x32_white;
			whiteKeyBitmap_hot = MarcusResources.btn_32x32_white_hot;
			grayKeyBitmap = MarcusResources.btn_32x32_gray;
			grayKeyBitmap_hot = MarcusResources.btn_32x32_gray_hot;
		}

		public void showFontList(FontLibraryMode mode, ArrayList fromtags, ArrayList shapes)
		{
			Form1.myRootForm.fontTreeView.Nodes.Clear();
			if (shapes == null)
			{
				return;
			}
			if (FontLibraryMode.All_by_Category == mode)
			{
				string[] array = new string[4]
				{
					Shape.FontProperties.FontFamily.Font.ToString(),
					Shape.FontProperties.FontFamily.Image.ToString(),
					Shape.FontProperties.FontFamily.Educational.ToString(),
					Shape.FontProperties.FontFamily.Seasonal.ToString()
				};
				string[] array2 = array;
				foreach (string text in array2)
				{
					TreeNode treeNode = new TreeNode(text);
					treeNode.Name = text;
					treeNode.ImageKey = "blank";
					treeNode.ForeColor = SystemColors.ControlDarkDark;
					Form1.myRootForm.fontTreeView.Nodes.Add(treeNode);
				}
			}
			if (fromtags != null)
			{
				for (int j = 0; j < shapes.Count; j++)
				{
					for (int k = 0; k < fromtags.Count; k++)
					{
						int fontNo = 0;
						int featureId = 0;
						int glyphId = 0;
						TagDictionary.fromTagRef((int)fromtags[k], ref fontNo, ref featureId, ref glyphId);
						if (j == fontNo)
						{
							addFontToList((Shape)shapes[j], FontLibraryMode.All_by_Category == mode);
							break;
						}
					}
				}
				return;
			}
			foreach (Shape shape in shapes)
			{
				if (FontLibraryMode.Keywords != mode && (mode != 0 || shape.properties.favorite) && (FontLibraryMode.My_Cartridges != mode || shape.properties.owned))
				{
					addFontToList(shape, FontLibraryMode.All_by_Category == mode);
				}
			}
			Form1.myRootForm.fontTreeView.ExpandAll();
		}

		private static void AddFontDisplayName(string name, string shownAs)
		{
			if (name != shownAs)
			{
				fontDisplayNames.Add(name, shownAs);
			}
		}

		private static void InitFontDisplayNames()
		{
			fontDisplayNames = new Hashtable();
			AddFontDisplayName("50 States", "50 States");
			AddFontDisplayName("Cricut(R) A Childs Year", "A Child’s Year");
			AddFontDisplayName("Accent Essentials", "Accent Essentials®");
			AddFontDisplayName("All Mixed Up", "All Mixed Up®");
			AddFontDisplayName("Alphalicious", "Alphalicious");
			AddFontDisplayName("Animal Kingdom", "Animal Kingdom®");
			AddFontDisplayName("Cricut(R) Ashlyns Alphabet", "Ashlyn’s Alphabet");
			AddFontDisplayName("Base Camp", "Base Camp®");
			AddFontDisplayName("Baseball", "Baseball");
			AddFontDisplayName("Cricut(R) Basketball", "Basketball");
			AddFontDisplayName("Beyond Birthdays", "Beyond Birthdays®");
			AddFontDisplayName("Cricut(R) Blackletter", "Blackletter");
			AddFontDisplayName("Cricut(R) Calligraphy Collection", "Calligraphy Collection");
			AddFontDisplayName("Campout", "Camp Out");
			AddFontDisplayName("Celebrations", "Celebrations®");
			AddFontDisplayName("Cricut(R) Christmas", "Christmas");
			AddFontDisplayName("Christmas Cheer", "Christmas Cheer®");
			AddFontDisplayName("Cricut Sampler", "Cricut Sampler®");
			AddFontDisplayName("Office Help", "Cricut® Office Help");
			AddFontDisplayName("Cursive 101", "Cursive 101");
			AddFontDisplayName("Cuttin Up", "Cuttin’ Up");
			AddFontDisplayName("Daisy Chain", "Daisy Chain");
			AddFontDisplayName("Cricut(R) Daisy Chain INTL", "Daisy Chain Intl");
			AddFontDisplayName("Cricut(R) Batman", "DC Comics - Batman:  The Brave and the Bold");
			AddFontDisplayName("Cricut(R) Superman", "DC Comics - Superman");
			AddFontDisplayName("Cricut(R) Designers Calendar", "Designer's Calendar");
			AddFontDisplayName("Cricut (R) Destinations", "Destinations");
			AddFontDisplayName("Cricut(R) Dinosaur Tracks", "Dinosaur Tracks");
			AddFontDisplayName("Cricut(R) Hannah Montana Font", "Disney - Hannah Montana");
			AddFontDisplayName("Mickey and Friends", "Disney - Mickey and Friends");
			AddFontDisplayName("Mickey Font", "Disney - Mickey Font");
			AddFontDisplayName("Pooh and Friends", "Disney - Pooh and Friends");
			AddFontDisplayName("Pooh Font Set", "Disney - Pooh Font Set");
			AddFontDisplayName("Dreams Come True", "Disney - Princesses Dreams Come True");
			AddFontDisplayName("Happily Ever After", "Disney - Princesses Happily Ever After");
			AddFontDisplayName("Cricut(R) TinkerBell & Friends", "Disney - Tinker Bell & Friends");
			AddFontDisplayName("Cricut(R) Cars", "Disney/Pixar - Cars");
			AddFontDisplayName("Don Juan", "DonJuan");
			AddFontDisplayName("Doodlecharms", "Doodlecharms®");
			AddFontDisplayName("Doodletype", "Doodletype®");
			AddFontDisplayName("Cricut(R) European Decor", "European Décor");
			AddFontDisplayName("Fabulous Finds", "Fabulous Finds");
			AddFontDisplayName("Cricut(R) From My Kitchen", "From My Kitchen");
			AddFontDisplayName("George and Basic Shapes", "George and Basic Shapes®");
			AddFontDisplayName("Going Places", "Going Places");
			AddFontDisplayName("Cricut(R) Graphically Speaking", "Graphically Speaking");
			AddFontDisplayName("Cricut(R) Gypsy Font", "Gypsy Font");
			AddFontDisplayName("Cricut(R) Gypsy Wanderings", "Gypsy Wanderings");
			AddFontDisplayName("Home Accents", "Home Accents");
			AddFontDisplayName("Cricut(R) Home Décor", "Home Décor");
			AddFontDisplayName("Indie Art", "Indie Art");
			AddFontDisplayName("Jasmine", "Jasmine®");
			AddFontDisplayName("Joys of the Season", "Joys of the Season");
			AddFontDisplayName("Cricut(R) Jubilee", "Jubilee");
			AddFontDisplayName("Cricut Keystone", "Keystone");
			AddFontDisplayName("Cricut(R) Keystone INTL", "Keystone International");
			AddFontDisplayName("Cricut(R) Kitchen and Bath Decor", "Kitchen and Bath Décor");
			AddFontDisplayName("Learning Curve", "Learning Curve");
			AddFontDisplayName("Cricut(R) Life is a Beach", "Life is a Beach");
			AddFontDisplayName("Locker Talk", "Locker Talk");
			AddFontDisplayName("Cricut(R) Lyrical Letters", "Lyrical Letters");
			AddFontDisplayName("Makin' the Grade", "Makin’ the Grade");
			AddFontDisplayName("Mini Monograms", "Mini Monograms®");
			AddFontDisplayName("My Community", "My Community®");
			AddFontDisplayName("My World", "My World®");
			AddFontDisplayName("New Arrival", "New Arrival®");
			AddFontDisplayName("Cricut(R) SpongeBob SquarePants", "Nickelodeon - SpongeBob SquarePants");
			AddFontDisplayName("Cricut(R) Old West", "Old West");
			AddFontDisplayName("Opposites Attract", "Opposites Attract®");
			AddFontDisplayName("Cricut(R) Pagoda Passion", "Pagoda");
			AddFontDisplayName("Cricut(R) Paisley", "Paisley");
			AddFontDisplayName("Paper Dolls Dress Up", "Paper Doll Dress Up®");
			AddFontDisplayName("Cricut(R) Everyday Paper Dolls", "Paper Dolls for Everyday");
			AddFontDisplayName("Paper Pups", "Paper Pups®");
			AddFontDisplayName("Cricut(R) Picturesque", "Picturesque");
			AddFontDisplayName("Cricut(R) Pink Journey", "Pink Journey");
			AddFontDisplayName("Plantin Schoolbook", "Plantin SchoolBook");
			AddFontDisplayName("Cricut(R) Potpourri Basket", "Potpourri Basket");
			AddFontDisplayName("Printing 101", "Printing 101");
			AddFontDisplayName("Printing Press", "Printing Press®");
			AddFontDisplayName("Cricut(R) Hello Kitty Font", "Sanrio - Hello Kitty® Font");
			AddFontDisplayName("Cricut(R) Hello Kitty Greetings", "Sanrio - Hello Kitty® Greetings");
			AddFontDisplayName("Cricut(R) Sans Serif", "Sans Serif");
			AddFontDisplayName("Cricut(R) Serenade", "Serenade");
			AddFontDisplayName("Cricut(R) Elmo Party", "Sesame Street - Elmo's Party");
			AddFontDisplayName("Cricut(R) Sesame Street Font", "Sesame Street Font");
			AddFontDisplayName("Cricut(R) Sesame Street Friends", "Sesame Street Friends");
			AddFontDisplayName("Cricut(R) Simply Sweet", "Simply Sweet");
			AddFontDisplayName("Soccer", "Soccer");
			AddFontDisplayName("Speaking of Fall", "Speaking of Fall");
			AddFontDisplayName("Speaking of School", "Speaking of School");
			AddFontDisplayName("Speaking of Winter", "Speaking of Winter");
			AddFontDisplayName("Cricut(R) Game On", "Sports Mania");
			AddFontDisplayName("Stamped", "Stamped®");
			AddFontDisplayName("Cricut(R) Stamping", "Stamping");
			AddFontDisplayName("Cricut(R) Stand and Salute", "Stand and Salute");
			AddFontDisplayName("Stone Script", "Stone Script");
			AddFontDisplayName("StoreFront", "StoreFront");
			AddFontDisplayName("Cricut(R) Storybook", "Storybook");
			AddFontDisplayName("Street Sign", "Street Sign");
			AddFontDisplayName("Stretch your Imagination", "Stretch your Imagination");
			AddFontDisplayName("Cricut(R) Sweet Treats", "Sweet Treats");
			AddFontDisplayName("Cricut(R) Sweethearts", "Sweethearts");
			AddFontDisplayName("Tags, Bags, Boxes and More", "Tags, Bags, Boxes & More®");
			AddFontDisplayName("Tear Drop", "Tear Drop®");
			AddFontDisplayName("Walk in my Garden", "Walk in My Garden®");
			AddFontDisplayName("Wedding", "Wedding");
			AddFontDisplayName("Wild Card", "Wild Card");
			AddFontDisplayName("Cricut(R) Winter Woodland", "Winter Woodland");
			AddFontDisplayName("Word Builders 1", "Word Builders 1® A Word Party®");
			AddFontDisplayName("Word Builders 2", "Word Builders 2® A Garden of Words®");
			AddFontDisplayName("Word Builders 3", "Word Builders 3® An Ocean of Words®");
			AddFontDisplayName("ZooBallo", "ZooBalloo®");
		}

		public static string GetFontDisplayName(string fontName)
		{
			if (fontDisplayNames.ContainsKey(fontName))
			{
				return (string)fontDisplayNames[fontName];
			}
			char[] trimChars = new char[1] { ' ' };
			fontName = fontName.Replace("Cricut(R)", "");
			fontName = fontName.TrimStart(trimChars);
			return fontName;
		}

		private void addFontToList(Shape shape, bool useFamily)
		{
			string fontDisplayName = GetFontDisplayName(shape.getFontName());
			TreeNode treeNode = new TreeNode(fontDisplayName, shape.fontImageId, shape.fontImageId);
			treeNode.Tag = shape;
			if (useFamily)
			{
				string text = shape.properties.family.ToString();
				TreeNode treeNode2 = Form1.myRootForm.fontTreeView.Nodes[text];
				if (treeNode2 == null)
				{
					treeNode2 = new TreeNode(text);
					treeNode2.Name = text;
					treeNode2.ImageKey = "blank";
					treeNode2.ForeColor = SystemColors.GrayText;
					Form1.myRootForm.fontTreeView.Nodes.Add(treeNode2);
				}
				treeNode2.Nodes.Add(treeNode);
			}
			else
			{
				Form1.myRootForm.fontTreeView.Nodes.Add(treeNode);
			}
		}

		private void loadFont(string filename, ref int oy)
		{
			int num = 20;
			Shape shape = Shape.loadFont(filename);
			if (shape != null)
			{
				string fontName = shape.getFontName();
				SplashScreen.SetStatus("Loading font\n\"" + GetFontDisplayName(fontName) + "\"...", setReference: true);
				shapes.Add(shape);
				fontMetadata.getData(fontName, shape.properties);
				shape.backStore = new Bitmap(20, 20);
				Graphics graphics = Graphics.FromImage(shape.backStore);
				Matrix matrix = new Matrix();
				shape.properties.selectedFeatureId = 0;
				shape.properties.selectedKeyId = 2;
				drawScaledGlyph(null, shape, shape.properties.selectedFeatureId, 30, 0f, 0f, highlight: false, useScaleArg: false, 0f, drawingFeatures: false, glyphInFontListFitSize, 80);
				matrix.Reset();
				graphics.Transform = matrix;
				graphics.InterpolationMode = InterpolationMode.High;
				if (matteImage != null)
				{
					graphics.DrawImage(matteImage, 0, 0, 20, 20);
				}
				shape.fontImageId = Form1.myRootForm.fontImageList.Images.Count;
				Form1.myRootForm.fontImageList.Images.Add(shape.getFontName(), shape.backStore);
				oy += num + 1;
			}
		}

		private void loadPTCFile(string filename, ref int oy)
		{
			int num = 20;
			Shape shape = Shape.loadPTC(filename);
			if (shape != null)
			{
				string text = shape.getFontName();
				if (text == null)
				{
					text = filename;
				}
				SplashScreen.SetStatus("Loading font\n\"" + GetFontDisplayName(text) + "\"...", setReference: true);
				shapes.Add(shape);
				fontMetadata.getData(text, shape.properties);
				shape.backStore = new Bitmap(20, 20);
				Graphics graphics = Graphics.FromImage(shape.backStore);
				Matrix matrix = new Matrix();
				shape.properties.selectedFeatureId = 0;
				shape.properties.selectedKeyId = 2;
				drawScaledGlyph(null, shape, shape.properties.selectedFeatureId, 30, 0f, 0f, highlight: false, useScaleArg: false, 0f, drawingFeatures: false, glyphInFontListFitSize, 80);
				matrix.Reset();
				graphics.Transform = matrix;
				graphics.InterpolationMode = InterpolationMode.High;
				if (matteImage != null)
				{
					graphics.DrawImage(matteImage, 0, 0, 20, 20);
				}
				shape.fontImageId = Form1.myRootForm.fontImageList.Images.Count;
				Form1.myRootForm.fontImageList.Images.Add(shape.getFontName(), shape.backStore);
				oy += num + 1;
			}
		}

		private void searchFontFolder(bool zip, string parent, ref int oy)
		{
			ArrayList arrayList = null;
			arrayList = MyStream.searchFolder(top: true, parent);
			for (int i = 0; i < arrayList.Count; i++)
			{
				string filename = MyStream.getFilename(zip, arrayList, i);
				if (Path.GetExtension(filename).ToLower().CompareTo(".txt") == 0)
				{
					Form1.myRootForm.trace("CP 0003I " + filename);
					loadFont(filename, ref oy);
					Form1.myRootForm.trace("CP 0003J");
				}
				if (Path.GetExtension(filename).ToLower().CompareTo(".pcgf") == 0)
				{
					Form1.myRootForm.trace("CP 0003K " + filename);
					loadPTCFile(filename, ref oy);
					Form1.myRootForm.trace("CP 0003L");
				}
			}
		}

		public void loadFonts()
		{
			InitFontDisplayNames();
			Form1.myRootForm.fontImageList.ImageSize = new Size(20, 20);
			Form1.myRootForm.fontImageList.ColorDepth = ColorDepth.Depth32Bit;
			Form1.myRootForm.fontImageList.Images.Add("blank", new Bitmap(20, 20));
			Form1.myRootForm.fontTreeView.ImageList = Form1.myRootForm.fontImageList;
			Form1.myRootForm.fontTreeView.ItemHeight = 20;
			Form1.myRootForm.fontTreeView.Font = new Font("Microsoft Sans Serif", 10f, FontStyle.Regular, GraphicsUnit.Point, 0);
			Form1.myRootForm.fontTreeView.DoubleClick += panel_DoubleClick;
			fontMetadata = new FontMetadata();
			string text = Form1.myRootForm.exeFolderPath + "\\CricutFontsMetadata.xml";
			string text2 = Form1.myRootForm.userDataFolderPath + "\\CricutFontsMetadata.xml";
			DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(text);
			DateTime lastWriteTimeUtc2 = File.GetLastWriteTimeUtc(text2);
			if (!fontMetadata.read(text2))
			{
				Form1.myRootForm.trace("CP 0003C");
				MessageBox.Show("Replacing bad font metadata file.");
				File.Copy(text, text2, overwrite: true);
				if (!fontMetadata.read(text2))
				{
					Form1.myRootForm.trace("CP 0003D");
					MessageBox.Show("Font metadata unrecoverable error.");
					Application.Exit();
				}
			}
			Form1.myRootForm.trace("CP 0003E");
			try
			{
				if ((lastWriteTimeUtc2 - lastWriteTimeUtc).TotalSeconds < 0.0)
				{
					fontMetadata.append(text);
					File.SetLastWriteTime(text2, lastWriteTimeUtc);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			shapes = new ArrayList();
			if (Path.GetExtension(Form1.myRootForm.fontFolderPath).ToLower().CompareTo(".zip") == 0)
			{
				int oy = 0;
				Form1.myRootForm.trace("CP 0003F");
				searchFontFolder(zip: true, Form1.myRootForm.fontFolderPath, ref oy);
			}
			else if (Directory.Exists(Form1.myRootForm.fontFolderPath))
			{
				int oy2 = 0;
				Form1.myRootForm.trace("CP 0003G");
				searchFontFolder(zip: false, Form1.myRootForm.fontFolderPath, ref oy2);
			}
			Form1.myRootForm.trace("CP 0003H");
			string registryValue = Form1.getRegistryValue(Form1.userKey, "currentFont");
			if (registryValue != null)
			{
				foreach (Shape shape in shapes)
				{
					string fontName = shape.getFontName();
					if (registryValue.CompareTo(fontName) == 0)
					{
						selectFont(shape, 0);
						break;
					}
				}
			}
			SplashScreen.SetReferencePoint();
			tagDictionary.load(shapes, fontMetadata);
			Form1.myRootForm.fontsTag1ComboBox.Items.AddRange(tagDictionary.getKeywordsStringList());
			Form1.myRootForm.fontsTag2ComboBox.Items.AddRange(tagDictionary.getKeywordsStringList());
			Form1.myRootForm.glyphKeywordComboBox.Items.AddRange(tagDictionary.getKeywordsStringList());
		}

		private void panel_DoubleClick(object sender, EventArgs e)
		{
			TreeView treeView = (TreeView)sender;
			if (treeView.SelectedNode != null)
			{
				GlyphIndex = -1;
				selectFont((Shape)treeView.SelectedNode.Tag, 0);
			}
		}

		public int getFontIndex(Shape shape)
		{
			for (int i = 0; i < shapes.Count; i++)
			{
				if (shape == shapes[i])
				{
					return i;
				}
			}
			return -1;
		}

		public void selectFont(Shape shape, int fontId)
		{
			if (shape != null)
			{
				string fontDisplayName = GetFontDisplayName(shape.getFontName());
				Form1.myRootForm.fontNameLabel.Text = fontDisplayName;
				Form1.setRegistryValue(Form1.userKey, "currentFont", shape.getFontName());
				FontId = fontId;
				Form1.setRegistryValue(Form1.userKey, "currentFontFeature", FontId.ToString());
				selectedShape = shape;
				Form1.myRootForm.myCartridgeMenuItem.Checked = selectedShape.properties.owned;
				Form1.myRootForm.favoriteCartridgeMenuItem.Checked = selectedShape.properties.favorite;
				showFontFeature(FontId);
			}
		}

		public bool drawScaledGlyph(Graphics g, Shape shape, int fontId, int keyId, float locX, float locY, bool highlight, bool useScaleArg, float scaleArg, bool drawingFeatures, float fitSize, int rasterSize)
		{
			return drawScaledGlyph(g, shape, fontId, keyId, locX, locY, highlight, useScaleArg, scaleArg, drawingFeatures, fitSize, rasterSize, 32, 32);
		}

		public bool drawScaledGlyph(Graphics g, Shape shape, int fontId, int keyId, float locX, float locY, bool highlight, bool useScaleArg, float scaleArg, bool drawingFeatures, float fitSize, int rasterSize, int glyphWidthInPixels, int glyphHeightInPixels)
		{
			int num = keyIdToGlyphIndex(keyId);
			Glyph glyph = shape.getGlyph(fontId, keyId);
			if (glyph == null)
			{
				return false;
			}
			Matrix matrix = new Matrix();
			matrix.Reset();
			matrix.Multiply(Canvas.glyphToWorld(0f, 0f, 1f), MatrixOrder.Append);
			RectangleF transformedBBox = Shape.getTransformedBBox(matrix, glyph);
			float num2 = 1f;
			num2 = ((!(transformedBBox.Width > transformedBBox.Height)) ? transformedBBox.Height : transformedBBox.Width);
			if (useScaleArg)
			{
				num2 = scaleArg;
			}
			matrix.Reset();
			matrix.Multiply(Canvas.glyphToWorld(0f, 0f, fitSize * (1f / num2)), MatrixOrder.Append);
			transformedBBox = Shape.getTransformedBBox(matrix, glyph);
			float locX2 = locX + 0.5f - (transformedBBox.X + transformedBBox.Width / 2f);
			float locY2 = locY - 0.5f - (transformedBBox.Y + transformedBBox.Height / 2f);
			if (!onlyOnce)
			{
				float locX3 = 0.5f - (transformedBBox.X + transformedBBox.Width / 2f);
				float locY3 = 0.5f - (transformedBBox.Y + transformedBBox.Height / 2f);
				shape.setGraphics(null, null, flipShape: false, welding: false, kerning: false);
				matrix.Reset();
				matrix.Multiply(Canvas.glyphToWorld(locX3, locY3, fitSize * (1f / num2)), MatrixOrder.Prepend);
				shape.cutMatrix = matrix;
				matteImage = (Bitmap)shape.renderGlyph(glyph, rasterSize);
				shape.cutMatrix.Reset();
			}
			if (g == null)
			{
				return true;
			}
			matrix.Reset();
			matrix.Multiply(Canvas.glyphToWorld(locX2, locY2, fitSize * (1f / num2)), MatrixOrder.Append);
			matrix.Multiply(worldToCanvas, MatrixOrder.Append);
			g.Transform = matrix;
			shape.setGraphics(g, null, flipShape: false, welding: false, kerning: false);
			matrix.Reset();
			g.Transform = matrix;
			g.CompositingQuality = CompositingQuality.HighQuality;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.InterpolationMode = InterpolationMode.High;
			if (drawingFeatures)
			{
				int num3 = ((1 == FontId % 2) ? (FontId - 1) : FontId);
				if (highlight && fontId == num3)
				{
					g.DrawImage(grayKeyBitmap_hot, (int)(locX * (float)glyphWidthInPixels), (int)(locY - 1f) * glyphHeightInPixels, glyphWidthInPixels, glyphHeightInPixels);
				}
				else
				{
					g.DrawImage(grayKeyBitmap, (int)(locX * (float)glyphWidthInPixels), (int)(locY - 1f) * glyphHeightInPixels, glyphWidthInPixels, glyphHeightInPixels);
				}
			}
			else if (highlight && num == GlyphIndex)
			{
				g.DrawImage(whiteKeyBitmap_hot, (int)(locX * (float)glyphWidthInPixels), (int)(locY - 1f) * glyphHeightInPixels, glyphWidthInPixels, glyphHeightInPixels);
			}
			else
			{
				g.DrawImage(whiteKeyBitmap, (int)(locX * (float)glyphWidthInPixels), (int)(locY - 1f) * glyphHeightInPixels, glyphWidthInPixels, glyphHeightInPixels);
			}
			Bitmap image = matteImage;
			g.CompositingMode = CompositingMode.SourceOver;
			g.DrawImage(image, locX * (float)glyphWidthInPixels + 1f, (locY - 1f) * (float)glyphHeightInPixels + 1f, glyphWidthInPixels - 4, glyphHeightInPixels - 4);
			matrix.Reset();
			matrix.Multiply(worldToCanvas, MatrixOrder.Append);
			g.Transform = matrix;
			return true;
		}

		public void setWorldToCanvas(int pbWidth, int pbHeight, RectangleF canvasR)
		{
			float num = (float)pbWidth / canvasR.Width;
			float num2 = (float)pbHeight / canvasR.Height;
			worldToCanvas.Reset();
			worldToCanvas.Scale(num, num2, MatrixOrder.Prepend);
			worldToCanvas.Translate(canvasR.Left, canvasR.Top, MatrixOrder.Prepend);
			canvasToWorld = worldToCanvas.Clone();
			canvasToWorld.Invert();
			penScale = 1f / (float)Math.Sqrt(num * num + num2 * num2);
		}

		public void showFeatures(Graphics g, int characterWidth, int characterHeight)
		{
			Form1.myRootForm.fontFeaturesMenuItem.DropDownItems.Clear();
			Shape shape = selectedShape;
			if (shape == null)
			{
				return;
			}
			string text = shape.header.fontDefs[0].fontFilename.Replace("NS.PFB", "");
			text = text.Replace("NS.pfb", "");
			text = text.Replace("ns.pfb", "");
			text = text.Replace(".PFB", "");
			text = text.Replace(".pfb", "");
			setWorldToCanvas(canvasR: new RectangleF(0f, 0f, 2f, 3f), pbWidth: Form1.myRootForm.featuresPicBox.Width, pbHeight: Form1.myRootForm.featuresPicBox.Height);
			new Matrix();
			int num = 30;
			int num2 = 0;
			for (int i = 0; i < 14; i++)
			{
				num2 = i;
				num = 30;
				while (shape.getGlyph(num2, num) == null && ++num < 70)
				{
				}
				if (i >= 2 && i % 2 == 0)
				{
					float locX = (i - 2) / 2 % 2;
					float locY = (i - 2) / 2 / 2 + 1;
					drawScaledGlyph(g, shape, num2, num, locX, locY, highlight: true, useScaleArg: false, 0f, drawingFeatures: true, glyphOnKeyFitSize, glyphOnKeyRasterSize, characterWidth, characterHeight);
				}
				drawScaledGlyph(null, shape, num2, num, 0f, 0f, highlight: true, useScaleArg: false, 0f, drawingFeatures: true, glyphInMenuFitSize, 96, characterWidth, characterHeight);
				if (shape.header.fontDefs[i] != null && shape.header.fontDefs[i].fontFilename != null && shape.glyphTables[i] != null)
				{
					char[] trimChars = new char[1] { ' ' };
					string fontFilename = shape.header.fontDefs[i].fontFilename;
					fontFilename = fontFilename.Replace("NS.PFB", "");
					fontFilename = fontFilename.Replace("SH.PFB", " (shift)");
					fontFilename = fontFilename.Replace("NS.pfb", "");
					fontFilename = fontFilename.Replace("SH.pfb", " (shift)");
					fontFilename = fontFilename.Replace("ns.pfb", "");
					fontFilename = fontFilename.Replace("sh.pfb", " (shift)");
					fontFilename = fontFilename.Replace(".PFB", "");
					fontFilename = fontFilename.Replace(".pfb", "");
					fontFilename = ((i <= 1) ? fontFilename.Replace(text, "Base") : fontFilename.Replace(text, ""));
					fontFilename = fontFilename.Replace("_", " ").TrimStart(trimChars);
					ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(fontFilename, matteImage);
					toolStripMenuItem.Tag = i;
					toolStripMenuItem.Click += featureMenuItem_Click;
					Form1.myRootForm.fontFeaturesMenuItem.DropDownItems.Add(toolStripMenuItem);
				}
			}
		}

		private void showFontFeature(int fontId)
		{
			if (fontId % 2 == 0)
			{
				Form1.myRootForm.shiftLock = false;
			}
			else
			{
				Form1.myRootForm.shiftLock = true;
			}
			if (!Form1.myRootForm.shiftLock)
			{
				Form1.myRootForm.shiftKeyLabel.Image = MarcusResources.shift_lock;
			}
			else
			{
				Form1.myRootForm.shiftKeyLabel.Image = MarcusResources.shift_lock_hot;
			}
			FontId = fontId;
			Form1.myRootForm.shiftKeyLabel.Refresh();
			Form1.setRegistryValue(Form1.userKey, "currentFontFeature", FontId.ToString());
			selectedShape.properties.selectedFeatureId = FontId;
			selectedShape.properties.selectedKeyId = -1;
			Form1.myRootForm.featuresPicBox_backStore = null;
			Form1.myRootForm.fontPicBox_backStore = null;
			Form1.myRootForm.featuresPicBox.Refresh();
			Form1.myRootForm.fontPicBox.Refresh();
		}

		private void featureMenuItem_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
			int fontId = (int)toolStripMenuItem.Tag;
			GlyphIndex = -1;
			showFontFeature(fontId);
		}

		public void dspyFont(Graphics g, int fontId, int glyphWidthInPixels, int glyphHeightInPixels)
		{
			Shape shape = selectedShape;
			if (shape == null)
			{
				return;
			}
			shape.getFontName();
			bool flag = false;
			if (shape.properties.isTextFont)
			{
				flag = true;
			}
			setWorldToCanvas(canvasR: new RectangleF(0f, 0f, 10f, 5f), pbWidth: Form1.myRootForm.fontPicBox.Width, pbHeight: Form1.myRootForm.fontPicBox.Height);
			new Matrix();
			int num = 30;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = float.MinValue;
			if (flag)
			{
				for (int i = 0; i < 40; i++)
				{
					if (29 != i && 37 != i && 38 != i && 39 != i)
					{
						int num5 = i / 10;
						int num6 = i - num5 * 10;
						float scale = 0f;
						float wide = 0f;
						float high = 0f;
						float top = 0f;
						float bottom = 0f;
						num = num5 * 14 + (num6 + 2);
						num2 = num6;
						num3 = num5 + 1;
						if (Shape.measureGlyph(shape, fontId, num, ref scale, ref wide, ref high, ref top, ref bottom) && scale > num4)
						{
							num4 = scale;
						}
					}
				}
			}
			for (int j = 0; j < 50; j++)
			{
				int num7 = j / 10;
				int num8 = j - num7 * 10;
				float scaleArg = num4;
				num = num7 * 14 + (num8 + 2);
				num2 = num8;
				num3 = num7 + 1;
				if (flag && j < 40 && 29 != j && 37 != j && 38 != j && 39 != j)
				{
					drawScaledGlyph(g, shape, fontId, num, num2, num3, highlight: true, useScaleArg: true, scaleArg, drawingFeatures: false, glyphOnKeyFitSize, glyphOnKeyRasterSize, glyphWidthInPixels, glyphHeightInPixels);
				}
				else
				{
					drawScaledGlyph(g, shape, fontId, num, num2, num3, highlight: true, useScaleArg: false, 0f, drawingFeatures: false, glyphOnKeyFitSize, glyphOnKeyRasterSize, glyphWidthInPixels, glyphHeightInPixels);
				}
			}
		}

		public static int keyIdToGlyphIndex(int keyId)
		{
			int num = keyId / 14;
			int num2 = keyId - num * 14 - 2;
			return num * 10 + num2;
		}

		public static int glyphIndexToKeyId(int glyphIndex)
		{
			int num = glyphIndex / 10;
			int num2 = glyphIndex - num * 10;
			return num * 14 + num2 + 2;
		}

		public void addGlyphToCanvas(int keyId, float size)
		{
			GlyphIndex = -1;
			Shape shape = selectedShape;
			float scale = 0f;
			float wide = 0f;
			float high = 0f;
			float top = 0f;
			float bottom = 0f;
			Canvas canvas = Form1.myRootForm.getCanvas();
			if (canvas == null)
			{
				return;
			}
			_ = canvas.drawR;
			if (shape == null)
			{
				return;
			}
			Shape.measureGlyph(shape, FontId, keyId, ref scale, ref wide, ref high, ref top, ref bottom);
			if (0f == wide && 0f == high)
			{
				return;
			}
			float num = size;
			if (Form1.myRootForm.realSizeCheckBox.Checked)
			{
				num = size / high;
			}
			wide *= num;
			high *= num;
			canvas.getCursor(out var _, out var _, out var _, out var _);
			if (canvas.selectedGroup == null)
			{
				float cursorX = canvas.cursorX;
				float num2 = canvas.cursorY;
				if (Form1.myRootForm.paperSaverCheckBox.Checked || Form1.myRootForm.realSizeCheckBox.Checked)
				{
					num2 += num2 - size - (num2 + top * num);
				}
				SceneGroup sceneGroup = canvas.newGroup();
				Form1.myRootForm.getCanvas().addGlyph(shape, FontId, keyId, cursorX, num2, num, sceneGroup);
				canvas.adjCursor(wide + num / 16f, 0f, size);
				sceneGroup.selected = true;
				canvas.selectedGroup = sceneGroup;
			}
			else
			{
				SceneGroup selectedGroup = canvas.selectedGroup;
				_ = canvas.cursorX;
				float num3 = selectedGroup.baseline;
				if (Form1.myRootForm.paperSaverCheckBox.Checked || Form1.myRootForm.realSizeCheckBox.Checked)
				{
					num3 += num3 - size - (num3 + top * num);
				}
				canvas.addGlyph(shape, FontId, keyId, canvas.cursorX, num3, num, selectedGroup);
				canvas.adjCursor(wide + num / 16f, 0f, size);
			}
			shape.properties.selectedFeatureId = FontId;
			shape.properties.selectedKeyId = keyId;
			Form1.myRootForm.refreshMattePicBox();
		}

		private void sharpen(Bitmap img)
		{
			for (int i = 1; i < img.Height - 1; i++)
			{
				Color color = img.GetPixel(0, i);
				Color color2 = img.GetPixel(1, i);
				for (int j = 1; j < img.Width - 1; j++)
				{
					Color pixel = img.GetPixel(j + 1, i);
					int num = (-2 * color.A + 11 * color2.A + -2 * pixel.A) / 7;
					num = ((num >= 0) ? ((num > 255) ? 255 : num) : 0);
					img.SetPixel(j, i, Color.FromArgb(num, 0, 0, 0));
					color = color2;
					color2 = pixel;
				}
			}
			for (int k = 1; k < img.Width - 1; k++)
			{
				Color color3 = img.GetPixel(k, 0);
				Color color4 = img.GetPixel(k, 1);
				for (int l = 1; l < img.Height - 1; l++)
				{
					Color pixel2 = img.GetPixel(k, l + 1);
					int num2 = (-2 * color3.A + 11 * color4.A + -2 * pixel2.A) / 7;
					num2 = ((num2 >= 0) ? ((num2 > 255) ? 255 : num2) : 0);
					img.SetPixel(k, l, Color.FromArgb(num2, 0, 0, 0));
					color3 = color4;
					color4 = pixel2;
				}
			}
		}
	}
}
