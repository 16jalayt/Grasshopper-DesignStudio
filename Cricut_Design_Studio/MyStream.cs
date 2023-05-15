using System;
using System.Collections;
using System.IO;

namespace Cricut_Design_Studio
{
	public class MyStream
	{
		private static ArrayList filelist;

		private StreamReader sr;

		private MemoryStream itemStream;

		private int itemStreamPosition;

		private byte[] itemStreamBuffer;

		public MyStream(string filename)
		{
			filename.Replace("\\", "\\");
			sr = new StreamReader(filename);
		}

		public int Read(char[] buffer, int index, int count)
		{
			if (itemStream != null)
			{
				if (itemStreamPosition + count > itemStreamBuffer.Length)
				{
					count = itemStreamBuffer.Length - itemStreamPosition;
				}
				if (count > 0)
				{
					Array.Copy(itemStreamBuffer, itemStreamPosition, buffer, index, count);
					itemStreamPosition += count;
				}
				return count;
			}
			if (sr != null)
			{
				return sr.Read(buffer, index, count);
			}
			return 0;
		}

		public string ReadLine()
		{
			if (itemStream != null)
			{
				string text = "";
				while (itemStreamPosition < itemStreamBuffer.Length)
				{
					char c = (char)itemStreamBuffer[itemStreamPosition++];
					if ('\n' == c)
					{
						break;
					}
					if ('\r' != c)
					{
						text += c;
					}
				}
				if (itemStreamPosition >= itemStreamBuffer.Length && text.Length < 1)
				{
					return null;
				}
				return text;
			}
			if (sr != null)
			{
				return sr.ReadLine();
			}
			return null;
		}

		public void Close()
		{
			if (itemStream != null)
			{
				itemStreamBuffer = null;
				itemStream.Close();
			}
			else if (sr != null)
			{
				sr.Close();
			}
		}

		public static ArrayList searchFolder(bool top, string parent)
		{
			if (top)
			{
				filelist = new ArrayList();
			}
			string[] directories = Directory.GetDirectories(parent);
			foreach (string text in directories)
			{
				string[] files = Directory.GetFiles(text, "*.*");
				foreach (string value in files)
				{
					filelist.Add(value);
				}
				searchFolder(top: false, text);
			}
			return filelist;
		}

		public static string getFilename(bool zip, ArrayList files, int i)
		{
			if (zip)
			{
				return (string)files[i];
			}
			return (string)files[i];
		}
	}
}
