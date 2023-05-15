using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Cricut_Design_Studio
{
	public class FontMetadata
	{
		private XmlDocument document;

		private string filename;

		private static int counter;

		public FontMetadata()
		{
			document = new XmlDocument();
		}

		public bool read(string filename)
		{
			this.filename = filename;
			if (File.Exists(filename))
			{
				XmlReader xmlReader = new XmlTextReader(filename);
				try
				{
					document.Load(xmlReader);
				}
				catch
				{
					xmlReader.Close();
					return false;
				}
				xmlReader.Close();
			}
			else
			{
				XmlElement newChild = document.CreateElement(null, "fonts", null);
				document.AppendChild(newChild);
			}
			document.Save(filename);
			return true;
		}

		public bool append(string filename)
		{
			XmlDocument xmlDocument = new XmlDocument();
			if (File.Exists(filename))
			{
				XmlReader xmlReader = new XmlTextReader(filename);
				try
				{
					xmlDocument.Load(xmlReader);
				}
				catch
				{
					xmlReader.Close();
					return false;
				}
				xmlReader.Close();
			}
			else
			{
				XmlElement newChild = xmlDocument.CreateElement(null, "fonts", null);
				xmlDocument.AppendChild(newChild);
			}
			MergeFontsWeDoNotHave(document.ChildNodes, xmlDocument.ChildNodes);
			return true;
		}

		private XmlNode FindFontsTag(XmlNodeList xmlNodeList)
		{
			foreach (XmlNode xmlNode in xmlNodeList)
			{
				if (xmlNode.Name == "fonts")
				{
					return xmlNode;
				}
			}
			return null;
		}

		private Dictionary<string, XmlNode> IndexFontNodes(XmlNodeList nodeToFonts)
		{
			Dictionary<string, XmlNode> dictionary = new Dictionary<string, XmlNode>();
			foreach (XmlNode nodeToFont in nodeToFonts)
			{
				if (!(nodeToFont.Name == "font"))
				{
					continue;
				}
				foreach (XmlNode childNode in nodeToFont.ChildNodes)
				{
					if (childNode.Name == "name")
					{
						dictionary[childNode.InnerText] = nodeToFont;
					}
				}
			}
			return dictionary;
		}

		private void MergeFontsWeDoNotHave(XmlNodeList xmlNodeListTo, XmlNodeList xmlNodeListFrom)
		{
			XmlNode xmlNode = FindFontsTag(xmlNodeListTo);
			Dictionary<string, XmlNode> dictionary = IndexFontNodes(xmlNode.ChildNodes);
			if (xmlNode == null)
			{
				throw new Exception("Expected to find 'fonts' tag in user xml data");
			}
			XmlNode xmlNode2 = FindFontsTag(xmlNodeListFrom);
			if (xmlNode2 == null)
			{
				return;
			}
			foreach (XmlNode childNode in xmlNode2.ChildNodes)
			{
				if (!(childNode.Name == "font"))
				{
					continue;
				}
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					if (childNode2.Name == "name" && !dictionary.ContainsKey(childNode2.InnerText))
					{
						XmlNode newChild = document.ImportNode(childNode, deep: true);
						xmlNode.AppendChild(newChild);
					}
				}
			}
		}

		public bool save()
		{
			if (filename != null)
			{
				document.Save(filename);
			}
			return true;
		}

		public void setData(string fontName, Shape.FontProperties props)
		{
			XmlElement documentElement = document.DocumentElement;
			XmlNode xmlNode = documentElement.SelectSingleNode("/fonts/font[name=\"" + fontName + "\"]");
			fontName = fontName.Replace("&", "&amp;");
			fontName = fontName.Replace("<", "&lt;");
			fontName = fontName.Replace(">", "&gt;");
			if (xmlNode == null)
			{
				XmlElement xmlElement = document.CreateElement("font");
				xmlElement.InnerXml = "<name>" + fontName + "</name><creator>" + props.creatorString + "</creator><family>" + props.family.ToString() + "</family><favorite>" + props.favorite + "</favorite><owned>" + props.owned + "</owned><textFont>" + props.isTextFont + "</textFont><tag>" + fontName + "</tag>";
				for (int i = 0; i < 14; i++)
				{
					XmlElement xmlElement2 = document.CreateElement("features");
					xmlElement.AppendChild(xmlElement2);
					XmlElement xmlElement3 = document.CreateElement("feature");
					xmlElement3.SetAttribute("id", i.ToString());
					for (int j = 0; j < 50; j++)
					{
						XmlElement xmlElement4 = document.CreateElement("glyph");
						xmlElement4.SetAttribute("id", j.ToString());
						xmlElement3.AppendChild(xmlElement4);
					}
					xmlElement2.AppendChild(xmlElement3);
				}
				xmlElement.SetAttribute("id", counter.ToString());
				counter++;
				documentElement.AppendChild(xmlElement);
			}
			else
			{
				XmlNode xmlNode2 = null;
				xmlNode2 = xmlNode.SelectSingleNode("family");
				if (xmlNode2 != null)
				{
					xmlNode2.InnerText = props.family.ToString();
				}
				xmlNode2 = xmlNode.SelectSingleNode("favorite");
				if (xmlNode2 != null)
				{
					xmlNode2.InnerText = props.favorite.ToString();
				}
				xmlNode2 = xmlNode.SelectSingleNode("owned");
				if (xmlNode2 != null)
				{
					xmlNode2.InnerText = props.owned.ToString();
				}
			}
		}

		public bool getData(string fontName, Shape.FontProperties props)
		{
			XmlElement documentElement = document.DocumentElement;
			XmlNode xmlNode = documentElement.SelectSingleNode("/fonts/font[name=\"" + fontName + "\"]");
			if (xmlNode != null)
			{
				XmlNode xmlNode2 = xmlNode.SelectSingleNode("creator");
				props.creatorString = xmlNode2.InnerText;
				xmlNode2 = xmlNode.SelectSingleNode("family");
				if (xmlNode2.InnerText.CompareTo("Font") == 0)
				{
					props.family = Shape.FontProperties.FontFamily.Font;
				}
				else if (xmlNode2.InnerText.CompareTo("Image") == 0)
				{
					props.family = Shape.FontProperties.FontFamily.Image;
				}
				else if (xmlNode2.InnerText.CompareTo("Educational") == 0)
				{
					props.family = Shape.FontProperties.FontFamily.Educational;
				}
				else if (xmlNode2.InnerText.CompareTo("Seasonal") == 0)
				{
					props.family = Shape.FontProperties.FontFamily.Seasonal;
				}
				xmlNode2 = xmlNode.SelectSingleNode("favorite");
				if (xmlNode2.InnerText.CompareTo("True") == 0)
				{
					props.favorite = true;
				}
				else
				{
					props.favorite = false;
				}
				xmlNode2 = xmlNode.SelectSingleNode("owned");
				if (xmlNode2.InnerText.CompareTo("True") == 0)
				{
					props.owned = true;
				}
				else
				{
					props.owned = false;
				}
				xmlNode2 = xmlNode.SelectSingleNode("textFont");
				if (xmlNode2.InnerText.CompareTo("True") == 0)
				{
					props.isTextFont = true;
				}
				else
				{
					props.isTextFont = false;
				}
				return true;
			}
			return false;
		}

		public bool addTag(string fontName, int featureId, int glyphId, string tag)
		{
			XmlElement documentElement = document.DocumentElement;
			XmlNode xmlNode = documentElement.SelectSingleNode("/fonts/font[name=\"" + fontName + "\"]");
			string xpath = "features/feature[@id='" + featureId + "']/glyph[@id='" + glyphId + "']";
			XmlNode xmlNode2 = xmlNode.SelectSingleNode(xpath);
			if (xmlNode2 == null)
			{
				return false;
			}
			XmlElement xmlElement = document.CreateElement("tag");
			xmlElement.InnerText = tag;
			xmlNode2.AppendChild(xmlElement);
			return true;
		}

		public bool setTags(string fontName, int featureId, int glyphId, string[] tags)
		{
			XmlElement documentElement = document.DocumentElement;
			XmlNode xmlNode = documentElement.SelectSingleNode("/fonts/font[name=\"" + fontName + "\"]");
			documentElement.SelectSingleNode("/fonts/font[name='" + fontName + "']/features/feature[@id='" + featureId + "']");
			string xpath = "features/feature[@id='" + featureId + "']/glyph[@id='" + glyphId + "']";
			XmlNode xmlNode2 = xmlNode.SelectSingleNode(xpath);
			if (xmlNode2 == null)
			{
				return false;
			}
			for (int num = xmlNode2.ChildNodes.Count - 1; num >= 0; num--)
			{
				if (xmlNode2.ChildNodes[num].Name.CompareTo("tag") == 0)
				{
					xmlNode2.RemoveChild(xmlNode2.ChildNodes[num]);
				}
			}
			for (int i = 0; i < tags.Length; i++)
			{
				if (tags[i] != null && tags[i].Length >= 1)
				{
					XmlElement xmlElement = document.CreateElement("tag");
					xmlElement.InnerText = tags[i];
					xmlNode2.AppendChild(xmlElement);
				}
			}
			return true;
		}

		public string[] getTags(string fontName, int featureId, int glyphId)
		{
			fontName = fontName.Replace('\'', ' ');
			XmlElement documentElement = document.DocumentElement;
			XmlNode xmlNode = documentElement.SelectSingleNode("/fonts/font[name=\"" + fontName + "\"]");
			if (xmlNode == null)
			{
				return null;
			}
			string xpath = "features/feature[@id='" + featureId + "']/glyph[@id='" + glyphId + "']";
			XmlNode xmlNode2 = xmlNode.SelectSingleNode(xpath);
			if (xmlNode2 == null || xmlNode2.ChildNodes.Count < 1)
			{
				return null;
			}
			string[] array = new string[xmlNode2.ChildNodes.Count];
			for (int i = 0; i < xmlNode2.ChildNodes.Count; i++)
			{
				array[i] = xmlNode2.ChildNodes[i].InnerText;
			}
			return array;
		}
	}
}
