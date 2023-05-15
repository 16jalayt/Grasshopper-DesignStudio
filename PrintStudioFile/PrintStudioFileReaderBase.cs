using System.IO;

namespace PrintStudioFile
{
	public class PrintStudioFileReaderBase
	{
		public virtual bool Stop => false;

		public virtual bool ProcessTag(PTCFile stream, string chunkType, long chunkLength)
		{
			return false;
		}

		public virtual void EndTag(Stream stream, string chunkType)
		{
		}
	}
}
