using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace PrintStudioFile
{
	public class PrintStudioFile
	{
		public const int HeaderLength = 8;

		public const string chunkTag_Image = "img ";

		public const string chunkTag_String = "strn";

		public const string chunkTag_NameValuePair = "nvp ";

		public const string chunkTag_NameValuePairs = "nvps";

		public const string chunkTag_CutPaths = "cpts";

		public const string chunkTag_CutPath = "cpth";

		public const string chunkTag_PreviewBitmap = "prvw";

		private const int HeaderTypeLength = 4;

		private const int HeaderLengthLength = 4;

		public string filename;

		public PTCFile stream;

		private static int bitmapNum;

		public bool IsEncrypted => stream.IsEncrypted;

		private byte[] ChunkHeader()
		{
			return new byte[8];
		}

		private void DecodeHeader(byte[] chunkHeader, ref string chunkType, ref long chunkLength)
		{
			chunkType = "";
			for (int i = 0; i < 4; i++)
			{
				chunkType += (char)chunkHeader[i];
			}
			chunkLength = 0L;
			for (int j = 0; j < 4; j++)
			{
				chunkLength *= 256L;
				chunkLength += chunkHeader[j + 4];
			}
		}

		private void EncodeHeader(byte[] chunkHeader, string chunkType, long chunkLength)
		{
			char[] array = chunkType.ToCharArray();
			for (int i = 0; i < 4; i++)
			{
				chunkHeader[i] = (byte)array[i];
			}
			for (int j = 0; j < 4; j++)
			{
				chunkHeader[7 - j] = (byte)(chunkLength % 256);
				chunkLength /= 256;
			}
		}

		public void ReadHeader(ref string chunkType, ref long chunkLength)
		{
			byte[] chunkHeader = ChunkHeader();
			stream.ReadHeader(chunkHeader);
			DecodeHeader(chunkHeader, ref chunkType, ref chunkLength);
			Console.WriteLine("Read header type '" + chunkType + "' and encryption is " + (stream.IsEncrypted ? "" : "not") + " encrypted");
		}

		public void RewriteHeader(long position)
		{
			byte[] array = ChunkHeader();
			stream.FlushEncryption();
			long position2 = stream.Position;
			EncodeHeader(array, "null", position2 - position - 8);
			stream.Seek(position + 4, SeekOrigin.Begin);
			stream.Write(array, 4, 4);
			stream.Seek(position2, SeekOrigin.Begin);
		}

		private void ReadInternal(PrintStudioFileReaderBase reader, long endPosition)
		{
			string chunkType = "ILLEGAL CHUNK TYPE";
			long chunkLength = -1L;
			Console.WriteLine("About to get stream position");
			long num = stream.Position;
			Console.WriteLine("About to enter while loop");
			while (num < endPosition && !reader.Stop)
			{
				ReadHeader(ref chunkType, ref chunkLength);
				Console.WriteLine("Read " + chunkType + " length " + chunkLength);
				if (reader.ProcessTag(stream, chunkType, chunkLength))
				{
					ReadInternal(reader, num + chunkLength);
					reader.EndTag(stream, chunkType);
				}
				if (!reader.Stop)
				{
					num += chunkLength + 8;
					stream.Seek(num, SeekOrigin.Begin);
				}
			}
		}

		public void Read(PrintStudioFileReaderBase reader, string filename)
		{
			stream = null;
			stream = new PTCFile(filename, FileMode.Open);
			Console.WriteLine("Opened " + filename + " for reading");
			ReadInternal(reader, stream.Length);
			stream.Close();
			stream.Dispose();
		}

		public void WriteAnyBitmap(string type, Image image, ImageFormat format)
		{
			long position = this.stream.Position;
			this.stream.SetLength(position);
			WriteHeader(type, 0L);
			EncryptedStream encryptedStream = new EncryptedStream(this.stream, position + 8, 0L);
			image.Save(encryptedStream, format);
			RewriteHeader(position);
			Stream stream = new FileStream("c:\\outbitmap_" + bitmapNum + ".png", FileMode.Create);
			image.Save(stream, ImageFormat.Png);
			stream.Close();
			bitmapNum++;
		}

		public void WriteImage(Image image, ImageFormat format)
		{
			WriteAnyBitmap("img ", image, format);
		}

		public void WritePreviewBitmap(Image image, ImageFormat format)
		{
			float num = Math.Min(128f / (float)image.Width, 128f / (float)image.Height);
			Image image2 = new Bitmap((int)((float)image.Width * num), (int)((float)image.Height * num));
			using (Graphics graphics = Graphics.FromImage(image2))
			{
				graphics.DrawImage(image, 0, 0, image2.Width, image2.Height);
			}
			WriteAnyBitmap("prvw", image2, format);
		}

		public void WriteAsciiData(string type, string s)
		{
			if (s == null)
			{
				s = "";
			}
			WriteHeader(type, s.Length);
			byte[] bytes = new ASCIIEncoding().GetBytes(s);
			stream.Write(bytes, 0, bytes.Length);
		}

		public void WriteString(string s)
		{
			WriteAsciiData("strn", s);
		}

		public void WriteNameValuePair(string name, string value)
		{
			if (value == null)
			{
				value = "";
			}
			WriteHeader("nvp ", 16 + name.Length + value.Length);
			WriteString(name);
			WriteString(value);
		}

		public void WriteNameValuePairs(string[] names, string[] values)
		{
			if (names.Length != values.Length)
			{
				string text = "mismatched length between names and values, names had " + names.Length;
				text += " entries, values had ";
				text += values.Length;
				throw new Exception(text);
			}
			long position = stream.Position;
			WriteHeader("nvps", 0L);
			for (int i = 0; i < names.Length; i++)
			{
				WriteNameValuePair(names[i], values[i]);
			}
			RewriteHeader(position);
		}

		public void WriteHeader(string p, long l)
		{
		}

		public void WriteHeader(string p, ref long glyphPosition)
		{
		}

		public void WriteFloat(float f)
		{
			WriteAsciiData("flot", f.ToString());
		}
	}
}
