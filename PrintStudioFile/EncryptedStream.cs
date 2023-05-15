using System.IO;

namespace PrintStudioFile
{
	public class EncryptedStream : Stream
	{
		private Stream baseStream;

		private long baseOffset;

		private long chunkLength;

		public override bool CanRead => baseStream.CanRead;

		public override bool CanSeek => baseStream.CanSeek;

		public override bool CanWrite => baseStream.CanWrite;

		public override long Length => baseStream.Length - baseOffset;

		public override long Position
		{
			get
			{
				return baseStream.Position - baseOffset;
			}
			set
			{
				baseStream.Position = value + baseOffset;
			}
		}

		public override bool CanTimeout => false;

		public EncryptedStream(Stream stream, long offset, long length)
		{
			baseStream = stream;
			baseOffset = offset;
			chunkLength = length;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			baseStream.Write(buffer, offset, count);
			if (chunkLength < Position + count)
			{
				chunkLength = Position + count;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (origin == SeekOrigin.Begin)
			{
				offset += baseOffset;
			}
			return baseStream.Seek(offset, origin);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return baseStream.Read(buffer, offset, count);
		}

		public override void SetLength(long l)
		{
			baseStream.SetLength(l + baseOffset);
		}

		public override void Flush()
		{
			baseStream.Flush();
		}

		public override void Close()
		{
		}
	}
}
