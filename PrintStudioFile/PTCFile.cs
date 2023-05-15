using System;
using System.IO;
using System.Runtime.InteropServices;
using Cricut_Design_Studio;

namespace PrintStudioFile
{
    public class PTCFile : Stream
    {
        private FileStream stream;

        private IntPtr fileptr;

        public string Filename => stream.Name;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => stream.Length;

        public override long Position
        {
            get
            {
                return stream.Position;
            }
            set
            {
                stream.Position = value;
                SetLocationForUnmanagedCode(value);
            }
        }

        public override bool CanTimeout => false;

        public bool IsEncrypted
        {
            get
            {
                bool success = false;
                int num = 0;
                try
                {
                }
                finally
                {
                    stream.SafeFileHandle.DangerousAddRef(ref success);
                    num = PTCIsEncrypted(fileptr, stream.SafeFileHandle.DangerousGetHandle());
                    if (success)
                    {
                        stream.SafeFileHandle.DangerousRelease();
                    }
                }
                return num != 0;
            }
        }

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr XZHORLCWMELWHPDO();

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void LYMBZCADCFZQLSXT(IntPtr fileptr);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int HKPHHCJYFDRNOJTC(IntPtr fileptr, IntPtr handle, [MarshalAs(UnmanagedType.LPArray)] byte[] bytes, int offset, int length);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int LVCBWYVVGMSHMVUZ(IntPtr fileptr, IntPtr handle, [MarshalAs(UnmanagedType.LPArray)] char[] bytes, int offset, int length);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int QABSRZNLQZYARSBE(IntPtr fileptr, IntPtr handle, [MarshalAs(UnmanagedType.LPArray)] byte[] headerBytes);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int HKPHHCJYFDRNOJTCCurveCount(IntPtr fileptr, IntPtr handle);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int PTCIsEncrypted(IntPtr fileptr, IntPtr handle);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int FTNYWRNYCBNACURR(IntPtr fileptr, IntPtr handle, [MarshalAs(UnmanagedType.LPArray)] int[] curveCoords);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int FWURHJYEKXGKONEG(IntPtr fileptr, IntPtr handle, long location);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DYADLQZGMXGZLPYD(IntPtr fileptr, IntPtr handle, [MarshalAs(UnmanagedType.LPArray)] int[] args);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void PTCFlushEncryption(IntPtr fileptr, IntPtr handle);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int RMHBEOHCQCVCHUKC(IntPtr fileptr, IntPtr handle, [MarshalAs(UnmanagedType.LPArray)] ushort[] endContours, [MarshalAs(UnmanagedType.LPArray)] byte[] flags, [MarshalAs(UnmanagedType.LPArray)] byte[] xCoords, [MarshalAs(UnmanagedType.LPArray)] byte[] yCoords);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int RMZVBIMJABJOAGBK(IntPtr fileptr, IntPtr handle, [MarshalAs(UnmanagedType.LPArray)] byte[] bytes, int length);

        public PTCFile(string filename, FileMode mode)
        {
            Form1.myRootForm.trace("Font Open - New:" + filename);
            fileptr = XZHORLCWMELWHPDO();
            Form1.myRootForm.trace("Font Open - Open");
            stream = new FileStream(filename, mode, (mode != FileMode.Create) ? FileAccess.Read : FileAccess.ReadWrite);
            Form1.myRootForm.trace("Font Open - Opened");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        private void SetLocationForUnmanagedCode(long offset)
        {
            bool success = false;
            try
            {
            }
            finally
            {
                stream.SafeFileHandle.DangerousAddRef(ref success);
                FWURHJYEKXGKONEG(fileptr, stream.SafeFileHandle.DangerousGetHandle(), offset);
                if (success)
                {
                    stream.SafeFileHandle.DangerousRelease();
                }
            }
        }

        public void FlushEncryption()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    SetLocationForUnmanagedCode(offset);
                    break;

                case SeekOrigin.Current:
                    SetLocationForUnmanagedCode(offset + Position);
                    break;

                case SeekOrigin.End:
                    SetLocationForUnmanagedCode(Length - offset);
                    break;
            }
            return stream.Seek(offset, origin);
        }

        public int ReadChars(char[] buffer, int offset, int count)
        {
            int num = -1;
            _ = stream.Position;
            bool success = false;
            try
            {
            }
            finally
            {
                stream.SafeFileHandle.DangerousAddRef(ref success);
                num = LVCBWYVVGMSHMVUZ(fileptr, stream.SafeFileHandle.DangerousGetHandle(), buffer, offset, count);
                if (success)
                {
                    stream.SafeFileHandle.DangerousRelease();
                }
            }
            return num;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int num = -1;
            _ = stream.Position;
            bool success = false;
            try
            {
            }
            finally
            {
                stream.SafeFileHandle.DangerousAddRef(ref success);
                num = HKPHHCJYFDRNOJTC(fileptr, stream.SafeFileHandle.DangerousGetHandle(), buffer, offset, count);
                if (success)
                {
                    stream.SafeFileHandle.DangerousRelease();
                }
            }
            return num;
        }

        public override void SetLength(long l)
        {
            stream.SetLength(l);
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public override void Close()
        {
            LYMBZCADCFZQLSXT(fileptr);
            fileptr = IntPtr.Zero;
            stream.Close();
        }

        public void ReadHeader(byte[] chunkHeader)
        {
            bool success = false;
            try
            {
            }
            finally
            {
                stream.SafeFileHandle.DangerousAddRef(ref success);
                QABSRZNLQZYARSBE(fileptr, stream.SafeFileHandle.DangerousGetHandle(), chunkHeader);
                if (success)
                {
                    stream.SafeFileHandle.DangerousRelease();
                }
            }
        }

        public bool ReadHeader(ref string type, ref int length)
        {
            if (stream.Position + 8 > stream.Length)
            {
                return false;
            }
            length = 0;
            byte[] array = new byte[8];
            for (int i = 0; i < 4; i++)
            {
                array[i] = 32;
            }
            ReadHeader(array);
            type = "";
            for (int j = 0; j < 4; j++)
            {
                type += (char)array[j];
            }
            for (int k = 0; k < 4; k++)
            {
                length = length * 256 + array[k + 4];
            }
            if (type == "" || type == "    ")
            {
                return false;
            }
            return true;
        }

        public int ReadCurveCount()
        {
            bool success = false;
            int num = 0;
            try
            {
            }
            finally
            {
                stream.SafeFileHandle.DangerousAddRef(ref success);
                num = HKPHHCJYFDRNOJTCCurveCount(fileptr, stream.SafeFileHandle.DangerousGetHandle());
                if (success)
                {
                    stream.SafeFileHandle.DangerousRelease();
                }
            }
            return num;
        }

        public int ReadCurveCoords(int[] coords)
        {
            bool success = false;
            int num = 0;
            try
            {
            }
            finally
            {
                stream.SafeFileHandle.DangerousAddRef(ref success);
                num = FTNYWRNYCBNACURR(fileptr, stream.SafeFileHandle.DangerousGetHandle(), coords);
                if (success)
                {
                    stream.SafeFileHandle.DangerousRelease();
                }
            }
            return num;
        }

        public int ReadGlyphCounts(ref int width, ref int xmin, ref int ymin, ref int xmax, ref int ymax, ref int nContours, ref int nFlags, ref int xLength, ref int yLength)
        {
            int[] array = new int[9];
            bool success = false;
            int num = 0;
            try
            {
            }
            finally
            {
                stream.SafeFileHandle.DangerousAddRef(ref success);
                num = DYADLQZGMXGZLPYD(fileptr, stream.SafeFileHandle.DangerousGetHandle(), array);
                if (success)
                {
                    stream.SafeFileHandle.DangerousRelease();
                }
            }
            width = array[0];
            xmin = array[1];
            ymin = array[2];
            xmax = array[3];
            ymax = array[4];
            nContours = array[5];
            nFlags = array[6];
            xLength = array[7];
            yLength = array[8];
            return num;
        }

        public int ReadGlyphData(ushort[] endContours, byte[] flags, byte[] xCoords, byte[] yCoords)
        {
            bool success = false;
            int num = 0;
            try
            {
            }
            finally
            {
                stream.SafeFileHandle.DangerousAddRef(ref success);
                num = RMHBEOHCQCVCHUKC(fileptr, stream.SafeFileHandle.DangerousGetHandle(), endContours, flags, xCoords, yCoords);
                if (success)
                {
                    stream.SafeFileHandle.DangerousRelease();
                }
            }
            return num;
        }

        public int ReadDerivationCheck(byte[] bytes, int length)
        {
            bool success = false;
            int num = 0;
            try
            {
            }
            finally
            {
                stream.SafeFileHandle.DangerousAddRef(ref success);
                num = RMZVBIMJABJOAGBK(fileptr, stream.SafeFileHandle.DangerousGetHandle(), bytes, length);
                if (success)
                {
                    stream.SafeFileHandle.DangerousRelease();
                }
            }
            return num;
        }

        public string ReadString(int length)
        {
            byte[] array = new byte[length];
            char[] array2 = new char[length];
            Read(array, 0, length);
            for (int i = 0; i < length; i++)
            {
                array2[i] = (char)array[i];
            }
            return new string(array2);
        }

        public void ReadNameValuePair(ref string name, ref string value)
        {
            string type = "";
            int length = 0;
            ReadHeader(ref type, ref length);
            if (type != "strn")
            {
                throw new Exception("Expected type strn in name value pair, got type " + type);
            }
            name = ReadString(length);
            ReadHeader(ref type, ref length);
            if (type != "strn")
            {
                throw new Exception("Expected type strn in name value pair, got type " + type);
            }
            value = ReadString(length);
        }
    }
}