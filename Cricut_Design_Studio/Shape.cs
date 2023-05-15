using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Cricut_Cartridge_Font_Converter;
using PrintStudioFile;

namespace Cricut_Design_Studio
{
	public class Shape
	{
		public class FontProperties
		{
			public enum FontFamily
			{
				Font,
				Image,
				Seasonal,
				Educational
			}

			public string name;

			public string creatorString;

			public FontFamily family;

			public bool favorite;

			public bool owned;

			public bool isTextFont;

			public int selectedFeatureId = -1;

			public int selectedKeyId = -1;

			public FontProperties()
			{
				creatorString = "unknown";
				family = FontFamily.Font;
				favorite = false;
				owned = false;
				isTextFont = true;
			}
		}

		internal enum GFlags
		{
			ONCURVE = 1,
			XSHORT = 2,
			YSHORT = 4,
			REPEAT = 8,
			XSAME = 0x10,
			YSAME = 0x20,
			ISCUBIC = 0x40
		}

		public class RenderCanvas
		{
			private class Edge
			{
				public float x;

				public float dx;

				public float dy;

				public int y;

				public int sy;

				public int ey;

				private void swap(ref float a, ref float b)
				{
					float num = a;
					a = b;
					b = num;
				}

				public void assign(Edge e)
				{
					x = e.x;
					dx = e.dx;
					dy = e.dy;
					y = e.y;
					sy = e.sy;
					ey = e.ey;
				}

				public Edge(Edge e)
				{
					assign(e);
				}

				public Edge(float ox, float oy, float cx, float cy)
				{
					if (oy > cy)
					{
						swap(ref ox, ref cx);
						swap(ref oy, ref cy);
					}
					dy = cy - oy;
					dx = (cx - ox) / dy;
					sy = (int)Math.Floor(oy);
					ey = (int)Math.Floor(cy);
					if ((float)sy + 0.5f < oy)
					{
						sy++;
					}
					if ((float)ey + 0.5f > cy)
					{
						ey--;
					}
					if (sy < 0)
					{
						sy = 0;
					}
					if (ey >= canvasHeight)
					{
						ey = canvasHeight - 1;
					}
					float num = (float)sy - oy;
					x = ox + dx * num;
					y = sy;
				}

				public void increment()
				{
					y++;
					x += dx;
				}
			}

			private class EdgeList
			{
				public ArrayList edges = new ArrayList();
			}

			public static int widthMultiplier = 256;

			public static int heightMultiplier = 256;

			public static int canvasWidth = 256;

			public static int canvasHeight = 256;

			private EdgeList[] edgeList;

			private ArrayList tmpEdges = new ArrayList();

			public int[,] raster;

			public void addEdge(float ox, float oy, float cx, float cy)
			{
				if (!(ox < 0f) && !(ox > 1f) && !(oy < 0f) && !(oy > 1f) && !(cx < 0f) && !(cx > 1f) && !(cy < 0f))
				{
					_ = 1f;
				}
				ox = (float)Math.Round(ox * (float)widthMultiplier * 3f) / 3f;
				oy = (float)Math.Round(oy * (float)heightMultiplier * 3f) / 3f;
				cx = (float)Math.Round(cx * (float)widthMultiplier * 3f) / 3f;
				cy = (float)Math.Round(cy * (float)heightMultiplier * 3f) / 3f;
				Edge edge = new Edge(ox, oy, cx, cy);
				if (edge.sy < canvasHeight && edge.ey >= 0 && edge.sy <= edge.ey)
				{
					tmpEdges.Add(edge);
				}
			}

			public void endContour()
			{
				foreach (Edge tmpEdge in tmpEdges)
				{
					if (edgeList[tmpEdge.sy] == null)
					{
						edgeList[tmpEdge.sy] = new EdgeList();
					}
					edgeList[tmpEdge.sy].edges.Add(tmpEdge);
				}
				discardEdges();
			}

			public void discardEdges()
			{
				tmpEdges.Clear();
			}

			public RenderCanvas(int width, int height)
			{
				canvasWidth = width;
				canvasHeight = height;
				raster = new int[canvasHeight, canvasWidth];
				edgeList = new EdgeList[canvasHeight];
			}

			private int[,] invert(int[,] image)
			{
				for (int i = 0; i < image.GetLength(0); i++)
				{
					for (int j = 0; j < image.GetLength(1); j++)
					{
						image[i, j] = ((1 != image[i, j]) ? 1 : 0);
					}
				}
				return image;
			}

			private int[,] manhattan(int[,] image)
			{
				for (int i = 0; i < image.GetLength(0); i++)
				{
					for (int j = 0; j < image.GetLength(1); j++)
					{
						if (image[i, j] == 1)
						{
							image[i, j] = 0;
							continue;
						}
						image[i, j] = image.GetLength(0) + image.GetLength(1);
						if (i > 0)
						{
							image[i, j] = Math.Min(image[i, j], image[i - 1, j] + 1);
						}
						if (j > 0)
						{
							image[i, j] = Math.Min(image[i, j], image[i, j - 1] + 1);
						}
					}
				}
				for (int num = image.GetLength(0) - 1; num >= 0; num--)
				{
					for (int num2 = image.GetLength(1) - 1; num2 >= 0; num2--)
					{
						if (num + 1 < image.GetLength(0))
						{
							image[num, num2] = Math.Min(image[num, num2], image[num + 1, num2] + 1);
						}
						if (num2 + 1 < image.GetLength(1))
						{
							image[num, num2] = Math.Min(image[num, num2], image[num, num2 + 1] + 1);
						}
					}
				}
				return image;
			}

			private int[,] dilate(int[,] image, int k)
			{
				image = manhattan(image);
				for (int i = 0; i < image.GetLength(0); i++)
				{
					for (int j = 0; j < image.GetLength(1); j++)
					{
						image[i, j] = ((image[i, j] <= k) ? 1 : 0);
					}
				}
				return image;
			}

			public void dilate(int n)
			{
				raster = dilate(raster, n);
			}

			public void erode(int n)
			{
				raster = invert(raster);
				raster = dilate(raster, n);
				raster = invert(raster);
			}

			public static void swap(ArrayList list, int i, int j)
			{
				Edge e = new Edge((Edge)list[i]);
				((Edge)list[i]).assign((Edge)list[j]);
				((Edge)list[j]).assign(e);
			}

			public static void sort(ArrayList list, int left, int right)
			{
				if (left >= right)
				{
					return;
				}
				swap(list, left, (left + right) / 2);
				int num = left;
				for (int i = left + 1; i <= right; i++)
				{
					if (((Edge)list[i]).x < ((Edge)list[left]).x)
					{
						swap(list, ++num, i);
					}
				}
				swap(list, left, num);
				sort(list, left, num - 1);
				sort(list, num + 1, right);
			}

			public void _render()
			{
				try
				{
					for (int i = 0; i < canvasHeight; i++)
					{
						if (edgeList[i] == null || edgeList[i].edges == null)
						{
							continue;
						}
						foreach (Edge edge in edgeList[i].edges)
						{
							for (int j = i; j <= edge.ey && j < raster.GetLength(0); j++)
							{
								float x = edge.x;
								int num = Math.Max(0, (int)Math.Round(x));
								for (int k = num; k < raster.GetLength(1); k++)
								{
									if (raster[j, k] == 0)
									{
										raster[j, k] = 1;
									}
									else
									{
										raster[j, k] = 0;
									}
								}
								edge.increment();
							}
						}
						edgeList[i].edges.Clear();
						edgeList[i].edges = null;
						edgeList[i] = null;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}

			public void render()
			{
				ArrayList arrayList = new ArrayList();
				for (int i = 0; i < canvasHeight; i++)
				{
					if (edgeList[i] != null)
					{
						foreach (Edge edge2 in edgeList[i].edges)
						{
							arrayList.Add(edge2);
						}
					}
					for (int num = arrayList.Count - 1; num >= 0; num--)
					{
						if (((Edge)arrayList[num]).ey < i)
						{
							arrayList.RemoveAt(num);
						}
					}
					if (arrayList.Count == 0)
					{
						continue;
					}
					if (arrayList.Count % 2 != 0)
					{
						Console.WriteLine("activeEdges problem");
						continue;
					}
					sort(arrayList, 0, arrayList.Count - 1);
					for (int j = 0; j < arrayList.Count; j += 2)
					{
						float x = ((Edge)arrayList[j]).x;
						float x2 = ((Edge)arrayList[j + 1]).x;
						int num2 = (int)Math.Floor(x);
						int num3 = (int)Math.Floor(x2);
						if ((float)num2 + 0.5f < x)
						{
							num2++;
						}
						if ((float)num3 + 0.5f > x2)
						{
							num3--;
						}
						if (num2 <= num3)
						{
							if (num2 < 0)
							{
								num2 = 0;
							}
							if (num3 >= canvasWidth)
							{
								num3 = canvasWidth - 1;
							}
							for (int k = num2; k <= num3; k++)
							{
								raster[i, k] = 1;
							}
						}
					}
					foreach (Edge item in arrayList)
					{
						item.increment();
					}
				}
			}
		}

		private const int cartMaxSize = 1081344;

		private const int pageSize = 16;

		public FontProperties properties = new FontProperties();

		public Header header;

		public GlyphTable[] glyphTables;

		public Bitmap backStore;

		public int fontImageId;

		public int myId;

		public static ArrayList shapesById = new ArrayList();

		public static Shape copyBuffer = null;

		public bool glyphsAddedSinceRead;

		private static string[] charNames = new string[95]
		{
			"/space", "/exclam", "/quotedbl", "/numbersign", "/dollar", "/percent", "/ampersand", "/quoteright", "/parenleft", "/parenright",
			"/asterisk", "/plus", "/comma", "/hyphen", "/period", "/slash", "/zero", "/one", "/two", "/three",
			"/four", "/five", "/six", "/seven", "/eight", "/nine", "/colon", "/semicolon", "/less", "/equal",
			"/greater", "/question", "/at", "/A", "/B", "/C", "/D", "/E", "/F", "/G",
			"/H", "/I", "/J", "/K", "/L", "/M", "/N", "/O", "/P", "/Q",
			"/R", "/S", "/T", "/U", "/V", "/W", "/X", "/Y", "/Z", "/bracketleft",
			"/backslash", "/bracketright", "/asciicircum", "/underscore", "/grave", "/a", "/b", "/c", "/d", "/e",
			"/f", "/g", "/h", "/i", "/j", "/k", "/l", "/m", "/n", "/o",
			"/p", "/q", "/r", "/s", "/t", "/u", "/v", "/w", "/x", "/y",
			"/z", "/braceleft", "/bar", "/braceright", "/asciitilde"
		};

		private PcControl pc;

		private Graphics graphics;

		private Pen pen;

		private Pen blackPen = new Pen(Color.Black, 1f);

		public Matrix cutMatrix;

		private bool rendering;

		private bool selecting;

		private bool selected;

		private float selectionDist;

		private PointF selectionPoint = new PointF(0f, 0f);

		private PointF lastPoint = default(PointF);

		private int contourId;

		private int selectedContour = -1;

		private static Clipper clipper = null;

		public float subdivDist = 0.25f;

		public bool flipShapes;

		public bool welding;

		public bool kerning;

		public int flipMinX;

		public int flipMaxX;

		public float bboxMinX = float.MaxValue;

		public float bboxMinY = float.MaxValue;

		public float bboxMaxX = float.MinValue;

		public float bboxMaxY = float.MinValue;

		private Pen contourPen;

		private Pen invisPen;

		private Pen invisContourPen;

		private RenderCanvas renderCanvas;

		public Shape()
		{
			myId = shapesById.Count;
			shapesById.Add(this);
			glyphTables = new GlyphTable[22];
		}

		~Shape()
		{
		}

		public string getFontName()
		{
			properties.name = header.cartHeader.fontName;
			return properties.name;
		}

		private Glyph loadGlyphFromPTC(PTCFile reader, long stop, ref int glyphPosition)
		{
			Glyph glyph = new Glyph();
			string type = "";
			int length = 0;
			int num = 0;
			while (reader.Position < stop && reader.ReadHeader(ref type, ref length))
			{
				long position = reader.Position;
				switch (type)
				{
				case "img ":
				{
					EncryptedStream encryptedStream = new EncryptedStream(reader, reader.Position, length);
					Image image = Image.FromStream(encryptedStream, useEmbeddedColorManagement: false, validateImageData: true);
					encryptedStream.Dispose();
					image.Dispose();
					break;
				}
				case "vshp":
				{
					byte[] buffer = new byte[length];
					long position2 = reader.Position;
					reader.Read(buffer, 0, length);
					reader.Position = position2;
					int width = 0;
					int xmin = 0;
					int ymin = 0;
					int xmax = 0;
					int ymax = 0;
					int nContours = 0;
					int nFlags = 0;
					int xLength = 0;
					int yLength = 0;
					if (reader.ReadGlyphCounts(ref width, ref xmin, ref ymin, ref xmax, ref ymax, ref nContours, ref nFlags, ref xLength, ref yLength) == 0)
					{
						throw new Exception("Invalid font format");
					}
					ushort[] endContours = new ushort[nContours];
					byte[] flags = new byte[nFlags];
					byte[] xCoords = new byte[xLength];
					byte[] yCoords = new byte[yLength];
					reader.ReadGlyphData(endContours, flags, xCoords, yCoords);
					glyph.width = (short)width;
					glyph.xMin = (short)xmin;
					glyph.yMin = (short)ymin;
					glyph.xMax = (short)xmax;
					glyph.yMax = (short)ymax;
					glyph.nContours = (byte)nContours;
					glyph.nFlags = (byte)nFlags;
					glyph.xLength = (byte)xLength;
					glyph.yLength = (byte)yLength;
					glyph.endContours = endContours;
					glyph.flags = flags;
					glyph.xCoords = xCoords;
					glyph.yCoords = yCoords;
					break;
				}
				case "nvp ":
				{
					string name2 = null;
					string value = null;
					reader.ReadNameValuePair(ref name2, ref value);
					if (name2 == "Glyph Position")
					{
						glyphPosition = Convert.ToInt32(value);
					}
					if (name2 == "Name")
					{
						glyph.name = value;
					}
					break;
				}
				case "strn":
				{
					string name = reader.ReadString(length);
					if (num == 0)
					{
						glyph.name = name;
					}
					num++;
					break;
				}
				}
				reader.Seek(position + length, SeekOrigin.Begin);
			}
			return glyph;
		}

		private void loadPTCFile(PTCFile reader)
		{
			string type = "";
			int length = 0;
			glyphTables = new GlyphTable[22];
			while (reader.ReadHeader(ref type, ref length))
			{
				long position = reader.Position;
				if (type == "font")
				{
					loadFontFromPTC(reader);
				}
				reader.Seek(position + length, SeekOrigin.Begin);
			}
		}

		private void loadFontFromPTC(PTCFile reader)
		{
			string type = null;
			int length = 0;
			while (reader.ReadHeader(ref type, ref length))
			{
				long position = reader.Position;
				if (type == "nvp ")
				{
					string name = null;
					string value = null;
					reader.ReadNameValuePair(ref name, ref value);
					if (name == "Name")
					{
						if (string.Compare(value, "Cricut(R) Pagoda") == 0)
						{
							header.cartHeader.fontName = "Cricut(R) Pagoda Passion";
						}
						else
						{
							header.cartHeader.fontName = value;
						}
						for (int i = 0; i < header.fontDefs.Length; i++)
						{
							header.fontDefs[i].fontFilename = value;
						}
					}
					_ = name == "Category";
					_ = name == "Keywords";
				}
				if (type == "fhdr")
				{
					LoadFontHeaderFromPTC(reader, position + length, header);
				}
				if (type == "tabl")
				{
					int tablePos = -1;
					GlyphTable glyphTable = LoadTableFromPTC(reader, position + length, ref tablePos);
					if (tablePos >= 0)
					{
						glyphTables[tablePos] = glyphTable;
					}
				}
				reader.Seek(position + length, SeekOrigin.Begin);
			}
		}

		private void LoadFontHeaderFromPTC(PTCFile reader, long stop, Header header)
		{
			string type = null;
			int length = 0;
			while (reader.Position < stop && reader.ReadHeader(ref type, ref length))
			{
				long position = reader.Position;
				if (type == "nvp ")
				{
					string name = null;
					string value = null;
					reader.ReadNameValuePair(ref name, ref value);
					if (name == "Count")
					{
						int num = Convert.ToInt32(value);
						header.fontDefs = new FontDef[num];
					}
				}
				else if (type == "fdef")
				{
					LoadFontDefFromPTC(reader, position + length, header);
				}
				reader.Seek(position + length, SeekOrigin.Begin);
			}
		}

		private void LoadFontDefFromPTC(PTCFile reader, long stop, Header header)
		{
			string type = null;
			int length = 0;
			int num = 0;
			while (reader.Position < stop && reader.ReadHeader(ref type, ref length))
			{
				long position = reader.Position;
				if (type == "nvp ")
				{
					string name = null;
					string value = null;
					reader.ReadNameValuePair(ref name, ref value);
					switch (name)
					{
					case "Index":
						num = Convert.ToInt32(value);
						break;
					case "Feature No":
					{
						int featureno = Convert.ToInt32(value);
						if (header.fontDefs[num] == null)
						{
							header.fontDefs[num] = new FontDef("", featureno, "");
						}
						else
						{
							header.fontDefs[num].featureno = featureno;
						}
						break;
					}
					case "Filename":
						if (header.fontDefs[num] == null)
						{
							header.fontDefs[num] = new FontDef(value, 0, "");
						}
						else
						{
							header.fontDefs[num].fontFilename = value;
						}
						break;
					case "Label":
						if (header.fontDefs[num] == null)
						{
							header.fontDefs[num] = new FontDef("", 0, value);
						}
						else
						{
							header.fontDefs[num].label = value;
						}
						break;
					}
				}
				if (type == "kmap")
				{
					LoadKeymapFromPTC(reader, position + length, header.fontDefs[num].keymap);
				}
				reader.Seek(position + length, SeekOrigin.Begin);
			}
		}

		private void LoadKeymapFromPTC(PTCFile reader, long stop, FontKeyMap fontKeyMap)
		{
			string type = null;
			int length = 0;
			int num = 0;
			while (reader.Position < stop && reader.ReadHeader(ref type, ref length))
			{
				_ = reader.Position;
				if (type == "nvp ")
				{
					string name = null;
					string value = null;
					reader.ReadNameValuePair(ref name, ref value);
					if (name == "Label")
					{
						fontKeyMap.label = value;
					}
					else if (name == "Map Count")
					{
						int num2 = Convert.ToInt32(value);
						fontKeyMap.map = new int[num2];
					}
				}
				if (type == "strn")
				{
					string value2 = reader.ReadString(length);
					fontKeyMap.map[num++] = Convert.ToInt32(value2);
				}
			}
		}

		private GlyphTable LoadTableFromPTC(PTCFile reader, long stop, ref int tablePos)
		{
			GlyphTable glyphTable = new GlyphTable(96);
			string type = null;
			int length = 0;
			while (reader.Position < stop && reader.ReadHeader(ref type, ref length))
			{
				long position = reader.Position;
				if (type == "nvp ")
				{
					string name = null;
					string value = null;
					reader.ReadNameValuePair(ref name, ref value);
					if (name == "Name")
					{
						glyphTable.fontName = value;
					}
					if (name == "Scaling")
					{
						glyphTable.scaling = (float)Convert.ToDouble(value);
					}
					if (name == "Table Position")
					{
						tablePos = Convert.ToInt32(value);
					}
				}
				if (type == "glph")
				{
					int glyphPosition = -1;
					Glyph glyph = loadGlyphFromPTC(reader, position + length, ref glyphPosition);
					if (glyph != null && glyphPosition >= 0)
					{
						glyphTable.glyphs[glyphPosition] = glyph;
					}
				}
				reader.Seek(position + length, SeekOrigin.Begin);
			}
			return glyphTable;
		}

		public static Shape loadPTC(string filename)
		{
			Shape shape = new Shape();
			shape.header = new Header();
			PTCFile pTCFile = new PTCFile(filename, FileMode.Open);
			shape.loadPTCFile(pTCFile);
			pTCFile.Close();
			pTCFile.Dispose();
			return shape;
		}

		public static Shape loadFont(string filename)
		{
			Shape shape = new Shape();
			shape.header = new Header(filename);
			string directoryName = Path.GetDirectoryName(filename);
			shape.loadHeader(directoryName);
			return shape;
		}

		public static int findGlyphIndex(int asciiVal, FontKeyMap keymap)
		{
			for (int i = 0; i < keymap.map.Length; i++)
			{
				if (keymap.map[i] == asciiVal)
				{
					return i;
				}
			}
			return -1;
		}

		public static int findGlyphIndex(string name, FontKeyMap keymap)
		{
			int num = -1;
			for (int i = 0; i < charNames.Length; i++)
			{
				if (name == charNames[i])
				{
					num = i + 32;
				}
			}
			if (-1 == num)
			{
				Console.WriteLine("Can't find ASCII value!\n");
				return -1;
			}
			int num2 = findGlyphIndex(num, keymap);
			if (-1 == num2)
			{
				Console.WriteLine(name + " -- Can't find mapping!\n");
			}
			return num2;
		}

		private static string fixHexShort(string s)
		{
			return s.Substring(2, 2) + s.Substring(0, 2);
		}

		private static string fixHexInt(string s)
		{
			return s.Substring(6, 2) + s.Substring(4, 2) + s.Substring(2, 2) + s.Substring(0, 2);
		}

		public bool loadHeader(string path)
		{
			uint thisAddr = 17946u;
			string text = "";
			_ = fixHexShort(Utils.ushort_to_hex(ushort.MaxValue)) + fixHexShort(Utils.ushort_to_hex(0)) + fixHexInt(Utils.uint_to_hex(0u));
			for (int i = 0; (long)i < 2242L; i++)
			{
			}
			string text2 = "";
			char[] array = new char[10] { 'P', 'R', 'O', 'G', 'R', 'A', 'M', 'M', 'E', 'D' };
			char[] array2 = array;
			foreach (char c in array2)
			{
				text2 += Utils.byte_to_hex((byte)c);
			}
			FontDef[] fontDefs = header.fontDefs;
			int num = 0;
			if (num < fontDefs.Length)
			{
				FontDef fontDef = fontDefs[num];
				header.cartHeader.keyHeightCode = findGlyphIndex(header.cartHeader.keyHeightChar, fontDef.keymap);
				header.cartHeader.spaceKeyCode = findGlyphIndex(32, fontDef.keymap);
			}
			string chunk = header.getChunk(18512);
			ushort num2;
			for (int k = 8; k < chunk.Length; k += num2)
			{
				num2 = Utils.hex_to_ushort(chunk.ToCharArray(), k);
				ushort num3 = Utils.hex_to_ushort(chunk.ToCharArray(), k + 4);
				text = "unknown";
				int num4 = -1;
				switch (num3)
				{
				case 21332:
					text = "String";
					break;
				case 16706:
					text = "Bytes";
					break;
				case 16725:
					text = "UShorts";
					break;
				case 17477:
					text = "Dial";
					break;
				case 20562:
					text = "Pressure";
					num4 = 5;
					break;
				case 21328:
					text = "Speed";
					num4 = 6;
					break;
				case 21338:
					text = "FontSize";
					num4 = 7;
					break;
				case 17990:
					text = "FontDef";
					break;
				case 18004:
					text = "Features";
					num4 = 2;
					break;
				case 18254:
					text = "GlyphNames";
					num4 = 3;
					break;
				case 18259:
					text = "GlyphNamesShift";
					num4 = 4;
					break;
				case 18252:
					text = "Glyph";
					break;
				case 18260:
					text = "GlyphTable";
					break;
				case 17224:
					text = "CartHeader";
					num4 = 1;
					break;
				}
				char[] array3 = new char[num2];
				for (int l = 0; l < num2; l++)
				{
					array3[l] = chunk[k + l];
				}
				_ = fixHexShort(Utils.ushort_to_hex(num3)) + fixHexShort(Utils.ushort_to_hex((ushort)((int)num2 / 2))) + fixHexInt(Utils.uint_to_hex(thisAddr));
				if (-1 == num4)
				{
					ushort num5 = num3;
					if (num5 != 18252)
					{
						_ = 18260;
					}
				}
				int num6 = ((int)num2 / 2 + 15) / 16;
				thisAddr += (uint)(num6 * 16);
			}
			int num7 = 0;
			FontDef[] fontDefs2 = header.fontDefs;
			foreach (FontDef fontDef2 in fontDefs2)
			{
				if (fontDef2.fontFilename != null)
				{
					string text3 = "";
					switch (fontDef2.featureno)
					{
					case 0:
						text3 = "std.fex";
						break;
					case 1:
						text3 = "std_s.fex";
						break;
					case 2:
						text3 = "opt0.fex";
						break;
					case 3:
						text3 = "opt0_s.fex";
						break;
					case 4:
						text3 = "opt1.fex";
						break;
					case 5:
						text3 = "opt1_s.fex";
						break;
					case 6:
						text3 = "opt2.fex";
						break;
					case 7:
						text3 = "opt2_s.fex";
						break;
					case 8:
						text3 = "opt3.fex";
						break;
					case 9:
						text3 = "opt3_s.fex";
						break;
					case 10:
						text3 = "opt4.fex";
						break;
					case 11:
						text3 = "opt4_s.fex";
						break;
					case 12:
						text3 = "opt5.fex";
						break;
					case 13:
						text3 = "opt5_s.fex";
						break;
					case 14:
						text3 = "opt6.fex";
						break;
					case 15:
						text3 = "opt6_s.fex";
						break;
					case 16:
						text3 = "opt7.fex";
						break;
					case 17:
						text3 = "opt7_s.fex";
						break;
					case 18:
						text3 = "opt8.fex";
						break;
					case 19:
						text3 = "opt8_s.fex";
						break;
					case 20:
						text3 = "opt9.fex";
						break;
					case 21:
						text3 = "opt9_s.fex";
						break;
					}
					fontDef2.hexFilename = path + "\\" + text3;
					glyphTables[fontDef2.featureno] = loadFont(fontDef2.hexFilename, fontDef2.keymap, num7++, ref thisAddr);
				}
			}
			string text4 = "";
			char[] array4 = new char[11]
			{
				'E', 'N', 'D', ' ', 'P', 'R', 'O', 'G', 'R', 'A',
				'M'
			};
			char[] array5 = array4;
			foreach (char c2 in array5)
			{
				text4 += Utils.byte_to_hex((byte)c2);
			}
			text = header.cartHeader.fontName.Replace(" ", "");
			text = text.Replace(",", "");
			text += "_";
			text += DateTime.Today.Year.ToString().Substring(2, 2);
			text += DateTime.Today.Month;
			text += DateTime.Today.Day;
			return true;
		}

		public static GlyphTable loadFont(string filename, FontKeyMap keymap, int fontId, ref uint thisAddr)
		{
			MyStream myStream = new MyStream(filename);
			char[] array = new char[8];
			GlyphTable glyphTable = new GlyphTable(96);
			string text = null;
			while (myStream.Read(array, 0, 8) > 0)
			{
				ushort num = Utils.hex_to_ushort(array, 0);
				if (num == 0)
				{
					break;
				}
				ushort num2 = Utils.hex_to_ushort(array, 4);
				char[] array2 = new char[num + 8];
				array.CopyTo(array2, 0);
				myStream.Read(array2, 8, num - 8);
				ushort num3 = Utils.hex_to_ushort(array2, 12);
				string text2 = "";
				switch (num2)
				{
				case 18252:
				{
					Utils.hex_to_ushort(array2, 8);
					text2 = Utils.decode_string(array2, 12 + num3);
					int num4 = findGlyphIndex(text2, keymap);
					if (-1 != num4)
					{
						Glyph glyph = new Glyph();
						glyph.width = Utils.hex_to_short(array2, 20);
						glyph.xMin = Utils.hex_to_short(array2, 24);
						glyph.yMin = Utils.hex_to_short(array2, 28);
						glyph.xMax = Utils.hex_to_short(array2, 32);
						glyph.yMax = Utils.hex_to_short(array2, 36);
						short yMin = glyph.yMin;
						glyph.yMin = (short)(-glyph.yMax);
						glyph.yMax = (short)(-yMin);
						glyph.nContours = (byte)Utils.hex_to_ushort(array2, 40);
						glyph.nFlags = Utils.hex_to_ushort(array2, 44);
						glyph.xLength = Utils.hex_to_ushort(array2, 48);
						glyph.yLength = Utils.hex_to_ushort(array2, 52);
						int num5 = Utils.hex_to_ushort(array2, 56);
						int num6 = Utils.hex_to_ushort(array2, 56 + num5);
						int num7 = Utils.hex_to_ushort(array2, 56 + num5 + num6);
						int num8 = Utils.hex_to_ushort(array2, 56 + num5 + num6 + num7);
						Utils.hex_to_ushort(array2, 56 + num5 + num6 + num7 + num8);
						glyph.endContours = new ushort[glyph.nContours];
						for (int i = 0; i < glyph.nContours; i++)
						{
							glyph.endContours[i] = Utils.hex_to_ushort(array2, 56 + num5 + i * 4 + 8);
						}
						glyph.flags = new byte[glyph.nFlags];
						for (int j = 0; j < glyph.nFlags; j++)
						{
							glyph.flags[j] = Utils.hex_to_byte(array2, 56 + num5 + num6 + j * 2 + 8);
						}
						glyph.xCoords = new byte[glyph.xLength];
						for (int k = 0; k < glyph.xLength; k++)
						{
							glyph.xCoords[k] = Utils.hex_to_byte(array2, 56 + num5 + num6 + num7 + k * 2 + 8);
						}
						glyph.yCoords = new byte[glyph.yLength];
						for (int l = 0; l < glyph.yLength; l++)
						{
							glyph.yCoords[l] = Utils.hex_to_byte(array2, 56 + num5 + num6 + num7 + num8 + l * 2 + 8);
						}
						glyph.name = text2;
						glyphTable.glyphs[num4] = glyph;
						if (text != null && string.Compare(text2, "/space") != 0)
						{
							int num9 = num4 / 14;
							int num10 = num4 - num9 * 14 - 2;
							glyph.imgPath = text + "\\Image" + (num9 * 10 + num10 + 1).ToString().PadLeft(2, '0') + ".jpg";
						}
					}
					break;
				}
				default:
					Console.WriteLine("\nUnrecognized data!\n");
					break;
				case 18260:
					break;
				}
			}
			myStream.Close();
			return glyphTable;
		}

		private static void getCoord(byte flags, ref short dx, ref short dy, ref short xsiz, ref short ysiz, byte[] xcoords, byte[] ycoords)
		{
			switch (flags & 0x12)
			{
			case 18:
			{
				byte b = xcoords[xsiz];
				dx = b;
				dx = (short)(-dx);
				xsiz++;
				break;
			}
			case 2:
			{
				byte b = xcoords[xsiz];
				dx = b;
				xsiz++;
				break;
			}
			case 16:
				dx = 0;
				break;
			default:
				dx = (short)(xcoords[xsiz] | (xcoords[xsiz + 1] << 8));
				xsiz += 2;
				break;
			}
			switch (flags & 0x24)
			{
			case 36:
			{
				byte b = ycoords[ysiz];
				dy = b;
				dy = (short)(-dy);
				ysiz++;
				break;
			}
			case 4:
			{
				byte b = ycoords[ysiz];
				dy = b;
				ysiz++;
				break;
			}
			case 32:
				dy = 0;
				break;
			default:
				_ = ycoords[ysiz];
				_ = ycoords[ysiz + 1];
				dy = (short)(ycoords[ysiz] | (ushort)(ycoords[ysiz + 1] << 8));
				ysiz += 2;
				break;
			}
		}

		public Glyph getGlyph(int fontId, int glyphId)
		{
			if (-1 == fontId || -1 == glyphId)
			{
				return null;
			}
			if (glyphTables == null || glyphTables[fontId] == null || glyphTables[fontId].glyphs[glyphId] == null)
			{
				return null;
			}
			return glyphTables[fontId].glyphs[glyphId];
		}

		public bool drawGlyph(int fontId, int glyphId, SceneGlyph sceneGlyph)
		{
			if (glyphTables == null || glyphTables[fontId] == null || glyphTables[fontId].glyphs[glyphId] == null)
			{
				return false;
			}
			drawGlyph(glyphTables[fontId].glyphs[glyphId], sceneGlyph);
			return true;
		}

		private PointF PointF_mid(PointF p1, PointF p2)
		{
			return new PointF((p1.X + p2.X) / 2f, (p1.Y + p2.Y) / 2f);
		}

		private float PointF_dist(PointF p1, PointF p2)
		{
			float num = p2.X - p1.X;
			float num2 = p2.Y - p1.Y;
			float num3 = num * num + num2 * num2;
			if ((double)num3 < 1E-05)
			{
				return 0f;
			}
			return (float)Math.Sqrt(num3);
		}

		private float pointToPointDist(PointF p1, PointF p2, ref PointF d)
		{
			d.X = p2.X - p1.X;
			d.Y = p2.Y - p1.Y;
			float num = d.X * d.X + d.Y * d.Y;
			if ((double)num < 1E-05)
			{
				return 0f;
			}
			return (float)Math.Sqrt(num);
		}

		private bool pointToLineDist(PointF p, PointF L1, PointF L2, ref float dist)
		{
			PointF d = new PointF(0f, 0f);
			float num = pointToPointDist(L1, L2, ref d);
			float num2 = p.X - L1.X;
			float num3 = p.Y - L1.Y;
			if ((double)num < 1E-05)
			{
				dist = pointToPointDist(p, L1, ref d);
				return true;
			}
			float num4 = (num2 * d.X + num3 * d.Y) / (num * num);
			if (num4 < 0f || num4 > 1f)
			{
				return false;
			}
			dist = pointToPointDist(p2: new PointF(L1.X + num4 * d.X, L1.Y + num4 * d.Y), p1: p, d: ref d);
			return true;
		}

		public bool isSelected()
		{
			return selected;
		}

		public void setSelectionPoint(PointF p, Matrix m, float d)
		{
			pc = null;
			graphics = null;
			cutMatrix = m;
			selecting = true;
			selected = false;
			selectionDist = d;
			selectionPoint.X = p.X;
			selectionPoint.Y = p.Y;
		}

		public void setSelectionOff()
		{
			selecting = false;
		}

		public void setGraphics(Graphics g, Pen pen, bool flipShape, bool welding, bool kerning)
		{
			pc = null;
			graphics = g;
			flipShapes = flipShape;
			this.welding = welding;
			this.kerning = kerning;
			if (pen != null)
			{
				this.pen = pen;
			}
			else
			{
				this.pen = blackPen;
			}
			cutMatrix = new Matrix();
		}

		public void setCricut(PcControl pc, Matrix m)
		{
			graphics = null;
			this.pc = pc;
			cutMatrix = m;
		}

		public static Clipper openClipper()
		{
			clipper = new Clipper();
			return clipper;
		}

		public static void closeClipper()
		{
			clipper = null;
		}

		public bool isClipping()
		{
			if (clipper != null)
			{
				if (!welding)
				{
					return kerning;
				}
				return true;
			}
			return false;
		}

		private int rv(float f)
		{
			return (int)Math.Round(f);
		}

		private void moveTo(PointF p)
		{
			if (isClipping())
			{
				clipper.addFirstVert(p.X, p.Y);
				return;
			}
			_ = graphics;
			lastPoint.X = p.X;
			lastPoint.Y = p.Y;
		}

		private void drawTo(PointF p)
		{
			if (isClipping())
			{
				clipper.addNextVert(p.X, p.Y);
				return;
			}
			if (rendering)
			{
				renderCanvas.addEdge(p.X, p.Y, lastPoint.X, lastPoint.Y);
			}
			if (selecting)
			{
				float dist = 0f;
				if (pointToLineDist(selectionPoint, p, lastPoint, ref dist) && dist <= selectionDist)
				{
					selected = true;
					selectedContour = contourId;
				}
			}
			if (graphics != null)
			{
				try
				{
					graphics.DrawLine(pen, lastPoint, p);
				}
				catch
				{
				}
			}
			lastPoint.X = p.X;
			lastPoint.Y = p.Y;
		}

		private void curveTo(PointF p0, PointF p1, PointF p2, PointF p3)
		{
			if (selecting || rendering || isClipping())
			{
				float num = PointF_dist(p0, p1) + PointF_dist(p1, p2) + PointF_dist(p2, p3);
				float num2 = subdivDist;
				if (pc != null && isClipping())
				{
					num2 = 36f;
				}
				if (num < num2)
				{
					drawTo(p3);
				}
				else
				{
					PointF p4 = PointF_mid(p0, p1);
					PointF pointF = PointF_mid(p1, p2);
					PointF p5 = PointF_mid(p2, p3);
					PointF pointF2 = PointF_mid(p4, pointF);
					PointF pointF3 = PointF_mid(pointF, p5);
					PointF pointF4 = PointF_mid(pointF2, pointF3);
					curveTo(p0, p4, pointF2, pointF4);
					curveTo(pointF4, pointF3, p5, p3);
				}
				lastPoint.X = p3.X;
				lastPoint.Y = p3.Y;
				return;
			}
			if (graphics != null)
			{
				try
				{
					graphics.DrawBezier(pen, p0, p1, p2, p3);
				}
				catch
				{
				}
			}
			lastPoint.X = p3.X;
			lastPoint.Y = p3.Y;
		}

		private void pc_drawBgn()
		{
			if (isClipping())
			{
				clipper.polyBgn();
			}
			else if (pc != null)
			{
				pc.drawBgn();
			}
		}

		private void pc_drawEnd()
		{
			if (isClipping())
			{
				clipper.polyEnd();
			}
			if (pc != null)
			{
				pc.drawEnd();
			}
		}

		private void pc_moveTo(PointF p)
		{
			if (pc == null || isClipping())
			{
				moveTo(p);
			}
			else
			{
				pc.moveTo(rv(p.X), rv(p.Y));
			}
		}

		private void pc_drawTo(int tick, PointF p)
		{
			if (pc == null || isClipping())
			{
				if (tick < 1)
				{
					drawTo(p);
				}
				return;
			}
			switch (tick)
			{
			case -1:
				pc.drawTo(rv(p.X), rv(p.Y));
				break;
			case 0:
				pc.drawTick(0, rv(p.X), rv(p.Y));
				break;
			case 1:
				pc.drawTick(1, rv(p.X), rv(p.Y));
				break;
			}
		}

		private void pc_curveTo(int tick, PointF p0, PointF p1, PointF p2, PointF p3)
		{
			if (pc == null || isClipping())
			{
				if (tick < 1)
				{
					curveTo(p0, p1, p2, p3);
				}
				return;
			}
			switch (tick)
			{
			case -1:
				pc.curveTo(rv(p0.X), rv(p0.Y), rv(p1.X), rv(p1.Y), rv(p2.X), rv(p2.Y), rv(p3.X), rv(p3.Y));
				break;
			case 0:
				pc.curveTick(0, rv(p0.X), rv(p0.Y), rv(p1.X), rv(p1.Y), rv(p2.X), rv(p2.Y), rv(p3.X), rv(p3.Y));
				break;
			case 1:
				pc.curveTick(1, rv(p0.X), rv(p0.Y), rv(p1.X), rv(p1.Y), rv(p2.X), rv(p2.Y), rv(p3.X), rv(p3.Y));
				break;
			}
		}

		public void assignPoint(ref PointF p, short x, short y)
		{
			if (flipShapes)
			{
				x = (short)(flipMaxX - (x - flipMinX));
			}
			PointF[] array = new PointF[1]
			{
				new PointF(x, -y)
			};
			cutMatrix.TransformPoints(array);
			p.X = array[0].X;
			p.Y = array[0].Y;
			if (p.X < bboxMinX)
			{
				bboxMinX = p.X;
			}
			if (p.Y < bboxMinY)
			{
				bboxMinY = p.Y;
			}
			if (p.X > bboxMaxX)
			{
				bboxMaxX = p.X;
			}
			if (p.Y > bboxMaxY)
			{
				bboxMaxY = p.Y;
			}
		}

		private void drawGlyph(Glyph g, SceneGlyph sceneGlyph)
		{
			short dx = 0;
			short dy = 0;
			short num = 0;
			short num2 = 0;
			short x = 0;
			short y = 0;
			short xsiz = 0;
			short ysiz = 0;
			short num3 = 0;
			PointF[] array = new PointF[4];
			PointF[] array2 = new PointF[4];
			bool flag = false;
			if (invisPen == null)
			{
				invisPen = new Pen(Color.LightBlue, 1f);
				invisPen.DashStyle = DashStyle.Dash;
			}
			if (contourPen == null)
			{
				contourPen = new Pen(Color.FromArgb(255, 128, 128), 1f);
				contourPen.DashStyle = DashStyle.Dash;
			}
			if (invisContourPen == null)
			{
				invisContourPen = new Pen(Color.Magenta, 1f);
				invisContourPen.DashStyle = DashStyle.Dash;
			}
			flipMinX = g.xMin;
			flipMaxX = g.xMax;
			bboxMinX = (bboxMinY = float.MaxValue);
			bboxMaxX = (bboxMaxY = float.MinValue);
			for (int i = 0; i < 4; i++)
			{
				ref PointF reference = ref array[i];
				reference = default(Point);
				ref PointF reference2 = ref array2[i];
				reference2 = default(Point);
			}
			pc_drawBgn();
			selectedContour = -1;
			for (int j = 0; j < g.nContours; j++)
			{
				Pen pen = this.pen;
				int num4 = 0;
				if (sceneGlyph != null && sceneGlyph.contourInvis != null && sceneGlyph.contourInvis[j])
				{
					num4 |= 1;
				}
				if (sceneGlyph != null && sceneGlyph == SceneGlyph.selectedGlyph && j == sceneGlyph.selectedContour)
				{
					num4 |= 2;
				}
				if (((uint)num4 & (true ? 1u : 0u)) != 0 && (pc != null || rendering || isClipping()))
				{
					while (num3 < g.endContours[j])
					{
						byte b = g.flags[num3++];
						if ((b & 1) == 0)
						{
							while ((b & 1) == 0)
							{
								getCoord(b, ref dx, ref dy, ref xsiz, ref ysiz, g.xCoords, g.yCoords);
								num = (short)(num + dx);
								num2 = (short)(num2 + dy);
								b = g.flags[num3++];
							}
							getCoord(b, ref dx, ref dy, ref xsiz, ref ysiz, g.xCoords, g.yCoords);
							num = (short)(num + dx);
							num2 = (short)(num2 + dy);
						}
						else
						{
							getCoord(b, ref dx, ref dy, ref xsiz, ref ysiz, g.xCoords, g.yCoords);
							num = (short)(num + dx);
							num2 = (short)(num2 + dy);
						}
					}
					continue;
				}
				switch (num4)
				{
				case 1:
					this.pen = invisPen;
					break;
				case 2:
					this.pen = contourPen;
					break;
				case 3:
					this.pen = invisContourPen;
					break;
				}
				contourId = j;
				bool flag2 = true;
				int num5 = 0;
				while (num3 < g.endContours[j])
				{
					byte b2 = g.flags[num3++];
					if ((b2 & 1) == 0)
					{
						int num6 = 0;
						assignPoint(ref array[num6++], num, num2);
						if (flag2)
						{
							flag2 = false;
						}
						while ((b2 & 1) == 0)
						{
							getCoord(b2, ref dx, ref dy, ref xsiz, ref ysiz, g.xCoords, g.yCoords);
							num = (short)(num + dx);
							num2 = (short)(num2 + dy);
							assignPoint(ref array[num6++], num, num2);
							b2 = g.flags[num3++];
						}
						getCoord(b2, ref dx, ref dy, ref xsiz, ref ysiz, g.xCoords, g.yCoords);
						num = (short)(num + dx);
						num2 = (short)(num2 + dy);
						assignPoint(ref array[num6++], num, num2);
						_ = 4;
						if (num5 == 0 && g.endContours[j] > 1)
						{
							flag = true;
							array2[0].X = array[0].X;
							array2[0].Y = array[0].Y;
							array2[1].X = array[1].X;
							array2[1].Y = array[1].Y;
							array2[2].X = array[2].X;
							array2[2].Y = array[2].Y;
							array2[3].X = array[3].X;
							array2[3].Y = array[3].Y;
							pc_curveTo(0, array2[0], array2[1], array2[2], array2[3]);
						}
						else
						{
							pc_curveTo(-1, array[0], array[1], array[2], array[3]);
						}
						num5++;
						continue;
					}
					getCoord(b2, ref dx, ref dy, ref xsiz, ref ysiz, g.xCoords, g.yCoords);
					num = (short)(num + dx);
					num2 = (short)(num2 + dy);
					assignPoint(ref array[0], num, num2);
					if (flag2)
					{
						x = num;
						y = num2;
						pc_moveTo(array[0]);
						flag2 = false;
						continue;
					}
					if (num5 == 0 && g.endContours[j] > 1)
					{
						flag = false;
						array2[0].X = array[0].X;
						array2[0].Y = array[0].Y;
						pc_drawTo(0, array2[0]);
					}
					else
					{
						pc_drawTo(-1, array[0]);
					}
					num5++;
				}
				assignPoint(ref array[0], x, y);
				if (1 == num5)
				{
					if (isClipping())
					{
						clipper.discardEdges();
					}
					if (rendering)
					{
						renderCanvas.discardEdges();
					}
				}
				else
				{
					pc_drawTo(-1, array[0]);
					if (flag)
					{
						pc_curveTo(1, array2[0], array2[1], array2[2], array2[3]);
					}
					else
					{
						pc_drawTo(1, array2[0]);
					}
					if (isClipping())
					{
						clipper.endContour();
					}
					if (rendering)
					{
						renderCanvas.endContour();
					}
				}
				this.pen = pen;
			}
			if (selecting && selectedContour != -1)
			{
				sceneGlyph.selectedContour = selectedContour;
			}
			pc_drawEnd();
		}

		public unsafe Image renderFullSize(Glyph g, int rasterWidth, int rasterHeight, int multiplier, int erode, Color col, SceneGlyph sceneGlyph)
		{
			float num = subdivDist;
			renderCanvas = new RenderCanvas(rasterWidth, rasterHeight);
			RenderCanvas.widthMultiplier = multiplier;
			RenderCanvas.heightMultiplier = multiplier;
			Color color = Color.FromArgb(255, col.R, col.G, col.B);
			Bitmap bitmap = new Bitmap(rasterWidth, rasterHeight, PixelFormat.Format32bppArgb);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.Clear(Color.FromArgb(0, col.R, col.G, col.B));
			rendering = true;
			subdivDist = 5f;
			drawGlyph(g, sceneGlyph);
			subdivDist = num;
			renderCanvas.render();
			BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, rasterWidth, rasterHeight), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
			int stride = bitmapData.Stride;
			int num2 = stride - rasterWidth * 4;
			byte* ptr = (byte*)(void*)bitmapData.Scan0;
			for (int i = 0; i < rasterHeight; i++)
			{
				fixed (int* ptr2 = &renderCanvas.raster[i, 0])
				{
					for (int j = 0; j < rasterWidth; j++)
					{
						if (ptr2[j] != 0)
						{
							*ptr = color.B;
							ptr[1] = color.G;
							ptr[2] = color.R;
							ptr[3] = byte.MaxValue;
						}
						ptr += 4;
					}
				}
				ptr += num2;
			}
			bitmap.UnlockBits(bitmapData);
			rendering = false;
			renderCanvas = null;
			return bitmap;
		}

		public unsafe Image renderGlyph(Glyph g, int rasterSize)
		{
			float num = subdivDist;
			renderCanvas = new RenderCanvas(rasterSize, rasterSize);
			RenderCanvas.widthMultiplier = rasterSize;
			RenderCanvas.heightMultiplier = rasterSize;
			rendering = true;
			subdivDist = 0.1f;
			drawGlyph(g, null);
			subdivDist = num;
			renderCanvas.render();
			Bitmap bitmap = new Bitmap(rasterSize, rasterSize, PixelFormat.Format32bppPArgb);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.Clear(Color.FromArgb(0, 0, 0, 0));
			BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, rasterSize, rasterSize), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
			int stride = bitmapData.Stride;
			int num2 = stride - rasterSize * 4;
			byte* ptr = (byte*)(void*)bitmapData.Scan0;
			for (int i = 0; i < rasterSize; i++)
			{
				fixed (int* ptr2 = &renderCanvas.raster[i, 0])
				{
					for (int j = 0; j < rasterSize; j++)
					{
						if (ptr2[j] != 0)
						{
							ptr[3] = byte.MaxValue;
						}
						ptr += 4;
					}
					ptr += num2;
				}
			}
			bitmap.UnlockBits(bitmapData);
			rendering = false;
			renderCanvas = null;
			return bitmap;
		}

		public static RectangleF getTransformedBBox(Matrix m, Glyph glyph)
		{
			PointF[] array = new PointF[4]
			{
				new PointF(glyph.xMin, glyph.yMin),
				new PointF(glyph.xMax, glyph.yMin),
				new PointF(glyph.xMax, glyph.yMax),
				new PointF(glyph.xMin, glyph.yMax)
			};
			m.TransformPoints(array);
			float num = float.MaxValue;
			float num2 = float.MinValue;
			float num3 = float.MaxValue;
			float num4 = float.MinValue;
			for (int i = 0; i < 4; i++)
			{
				if (array[i].X < num)
				{
					num = array[i].X;
				}
				if (array[i].X > num2)
				{
					num2 = array[i].X;
				}
				if (array[i].Y < num3)
				{
					num3 = array[i].Y;
				}
				if (array[i].Y > num4)
				{
					num4 = array[i].Y;
				}
			}
			return new RectangleF(num, num3, num2 - num, num4 - num3);
		}

		public static bool measureGlyph(Shape shape, int fontId, int keyId, ref float scale, ref float wide, ref float high, ref float top, ref float bottom)
		{
			Glyph glyph = shape.getGlyph(fontId, keyId);
			if (glyph == null)
			{
				return false;
			}
			Matrix matrix = new Matrix();
			matrix.Reset();
			matrix.Multiply(Canvas.glyphToWorld(0f, 0f, 1f), MatrixOrder.Append);
			RectangleF transformedBBox = getTransformedBBox(matrix, glyph);
			scale = 1f;
			if (transformedBBox.Width > transformedBBox.Height)
			{
				scale = transformedBBox.Width;
			}
			else
			{
				scale = transformedBBox.Height;
			}
			wide = transformedBBox.Width;
			high = transformedBBox.Height;
			top = transformedBBox.Top;
			bottom = transformedBBox.Bottom;
			return true;
		}
	}
}
