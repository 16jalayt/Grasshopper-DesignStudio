using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using ChunkComm;

namespace Cricut_Design_Studio
{
    public class PcControl
    {
        private const byte PcCmd_Query = 16;

        private const byte PcCmd_Draw = 32;

        private const byte PcCmd_Jukebox = 128;

        private const byte PcQuery_GetCanvas = 1;

        private const byte PcQuery_GetVersion = 2;

        private const byte PcQuery_NOOP = 4;

        private const byte PcQuery_GetCartName = 8;

        public const byte PcDraw_Bgn = 1;

        public const byte PcDraw_End = 2;

        public const byte PcJukebox_Reset = 1;

        public const byte PcJukebox_Select = 2;

        public const byte PcGypsy_Cmd = 4;

        public const byte PcGypsy_Set = 8;

        public const byte PcGypsy_Cmd_LoadPaper = 1;

        public const byte PcGypsy_Cmd_LoadLast = 2;

        public const byte PcGypsy_Cmd_UnLoadPaper = 3;

        public const byte PcGypsy_Set_Speed = 1;

        public const byte PcGypsy_Set_Pressure = 2;

        public const ushort PcDraw_MoveTo = 19796;

        public const ushort PcDraw_DrawTo = 17492;

        public const ushort PcDraw_CurveTo = 17236;

        public const ushort PcDraw_DrawTick = 17524;

        public const ushort PcDraw_CurveTick = 17268;

        private Comm comm;

        public int PaperOriginX;

        public int PaperOriginY;

        public int PaperCornerX;

        public int PaperCornerY;

        public int cricutStatus;

        private ArrayList JukeboxCarts = new ArrayList();

        private static long startCounter = 0L;

        private static long updateFreq = 0L;

        private static int waitAtLeastThisLong = 10;

        private int longDelay = 10;

        private int shortDelay = 2;

        public bool badUSBDriver;

        public bool getCanvas(ref int ox, ref int oy, ref int cx, ref int cy)
        {
            Thread.Sleep(250);
            byte[] array = new byte[5] { 4, 17, 0, 0, 0 };
            usb_write(array, 0, array.Length);
            byte[] array2 = new byte[8];
            int num = usb_read(array2);
            if (8 == num)
            {
                ox = (ushort)((array2[0] << 8) | array2[1]);
                oy = (ushort)((array2[2] << 8) | array2[3]);
                cx = (ushort)((array2[4] << 8) | array2[5]);
                cy = (ushort)((array2[6] << 8) | array2[7]);
                return true;
            }
            return false;
        }

        public bool getVersion(ref int machine, ref int major, ref int minor)
        {
            Thread.Sleep(250);
            byte[] array = new byte[5] { 4, 18, 0, 0, 0 };
            usb_write(array, 0, array.Length);
            byte[] array2 = new byte[6];
            int num = usb_read(array2);
            if (6 == num)
            {
                machine = (ushort)((array2[0] << 8) | array2[1]);
                major = (ushort)((array2[2] << 8) | array2[3]);
                minor = (ushort)((array2[4] << 8) | array2[5]);
                return true;
            }
            return false;
        }

        public string getCartridge(ref int cartIsProgrammed)
        {
            Thread.Sleep(250);
            byte[] array = new byte[5] { 4, 24, 0, 0, 0 };
            usb_write(array, 0, array.Length);
            byte[] array2 = new byte[40];
            usb_read(array2);
            cartIsProgrammed = (ushort)((array2[0] << 8) | array2[1]);
            int num = (ushort)((array2[2] << 8) | array2[3]);
            int num2 = 0;
            for (num2 = 0; num2 < num && array2[4 + num2] != 0; num2++)
            {
            }
            char[] array3 = new char[num2];
            for (int i = 0; i < num2; i++)
            {
                array3[i] = (char)array2[4 + i];
            }
            string text = new string(array3);
            char[] array4 = new char[1];
            char[] trimChars = array4;
            return text.TrimEnd(trimChars);
        }

        public bool NOOP(out ushort err, out ushort paper)
        {
            Thread.Sleep(250);
            byte[] array = new byte[5] { 4, 20, 0, 0, 0 };
            usb_write(array, 0, array.Length);
            err = (paper = 0);
            byte[] array2 = new byte[4];
            int num = usb_read(array2);
            if (4 == num)
            {
                err = (ushort)((array2[0] << 8) | array2[1]);
                paper = (ushort)((array2[2] << 8) | array2[3]);
                return true;
            }
            return false;
        }

        public int selectJukeboxes(bool justSelectFirst)
        {
            int cartIsProgrammed = 0;
            int result = 0;
            for (int i = 1; i <= 7; i++)
            {
                string text = null;
                Thread.Sleep(250);
                switch (jukeboxSelect(i))
                {
                    case 40:
                        JukeboxCarts.Add("");
                        break;

                    case 0:
                    case 32:
                        Thread.Sleep(125);
                        text = getCartridge(ref cartIsProgrammed);
                        Thread.Sleep(250);
                        JukeboxCarts.Add(text);
                        if (justSelectFirst)
                        {
                            return 0;
                        }
                        break;

                    case 8:
                        result = i;
                        Thread.Sleep(250);
                        break;

                    default:
                        Console.WriteLine(i + " Jukebox Error.");
                        break;
                }
            }
            return result;
        }

        public ArrayList readJukeboxes(bool justSelectFirst)
        {
            switch (jukeboxReset())
            {
                case 40:
                    return null;

                case 32:
                    Thread.Sleep(1500);
                    return null;

                case 0:
                    Thread.Sleep(1500);
                    return null;

                default:
                    {
                        int num = 0;
                        JukeboxCarts.Clear();
                        do
                        {
                            num = selectJukeboxes(justSelectFirst);
                        }
                        while (7 == num);
                        return JukeboxCarts;
                    }
            }
        }

        public int jukeboxReset()
        {
            Thread.Sleep(250);
            byte[] array = new byte[5] { 4, 129, 0, 0, 0 };
            usb_write(array, 0, array.Length);
            byte[] array2 = new byte[4];
            usb_read(array2);
            return (ushort)((array2[0] << 8) | array2[1]);
        }

        public int jukeboxSelect(int i)
        {
            Thread.Sleep(250);
            byte[] array = new byte[5]
            {
                4,
                130,
                (byte)i,
                0,
                0
            };
            usb_write(array, 0, array.Length);
            byte[] array2 = new byte[4];
            usb_read(array2);
            return (ushort)((array2[0] << 8) | array2[1]);
        }

        public int gypsyCommand(int cmd)
        {
            Thread.Sleep(250);
            byte[] array = new byte[5]
            {
                4,
                132,
                (byte)cmd,
                0,
                0
            };
            usb_write(array, 0, array.Length);
            byte[] data = new byte[4];
            usb_read(data);
            return 0;
        }

        public int gypsySetting(int cmd, int value)
        {
            Thread.Sleep(250);
            byte[] array = new byte[5]
            {
                4,
                136,
                (byte)cmd,
                (byte)value,
                0
            };
            usb_write(array, 0, array.Length);
            byte[] data = new byte[4];
            usb_read(data);
            return 0;
        }

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DllLoadFTDILibrary();

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void usb_write([MarshalAs(UnmanagedType.LPArray)] byte[] data, int start, int size);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void usb_writeBlock([MarshalAs(UnmanagedType.LPArray)] byte[] data, int start, int size);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int usb_read([MarshalAs(UnmanagedType.LPArray)] byte[] data);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_moveTo(int x, int y);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_lineTo(int x, int y);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_lineIn(int x, int y);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_lineOut(int x, int y);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_curveTo(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_curveIn(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int usb_curveOut(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        private static void waitAtLeast(int ms)
        {
            long lpPerformanceCount = 0L;
            QueryPerformanceCounter(out lpPerformanceCount);
            int num = (int)Math.Round((double)(lpPerformanceCount - startCounter) / (double)updateFreq * 1000.0);
            if (num < ms)
            {
                Thread.Sleep(ms - num);
            }
        }

        private static void resetWait(int ms)
        {
            QueryPerformanceCounter(out startCounter);
            waitAtLeastThisLong = ms;
        }

        public virtual bool bgn()
        {
            comm = new Comm(200000);
            if (!comm.isDriverOkay())
            {
                badUSBDriver = true;
                return false;
            }
            if (!comm.isAvailable())
            {
                return false;
            }
            comm.dump();
            QueryPerformanceFrequency(out updateFreq);
            resetWait(longDelay);
            return true;
        }

        public virtual bool end()
        {
            comm.Close();
            return true;
        }

        public virtual void drawBgn()
        {
            Thread.Sleep(250);
            byte[] array = new byte[5] { 4, 33, 0, 0, 0 };
            usb_write(array, 0, array.Length);
            resetWait(longDelay);
        }

        public virtual void drawEnd()
        {
            waitAtLeast(waitAtLeastThisLong);
            byte[] array = new byte[5] { 4, 34, 0, 0, 0 };
            usb_write(array, 0, array.Length);
            resetWait(longDelay);
        }

        public virtual void moveTo(int x, int y)
        {
            waitAtLeast(waitAtLeastThisLong);
            cricutStatus = usb_moveTo(x, y);
            resetWait(longDelay);
        }

        public virtual void drawTo(int x, int y)
        {
            waitAtLeast(waitAtLeastThisLong);
            cricutStatus = usb_lineTo(x, y);
            resetWait(shortDelay);
        }

        public virtual void curveTo(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
        {
            waitAtLeast(waitAtLeastThisLong);
            cricutStatus = usb_curveTo(x0, y0, x1, y1, x2, y2, x3, y3);
            resetWait(shortDelay);
        }

        public virtual void drawTick(int tickCode, int x, int y)
        {
            waitAtLeast(waitAtLeastThisLong);
            switch (tickCode)
            {
                case 0:
                    cricutStatus = usb_lineIn(x, y);
                    break;

                case 1:
                    cricutStatus = usb_lineOut(x, y);
                    break;
            }
            resetWait(shortDelay);
        }

        public virtual void curveTick(int tickCode, int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
        {
            waitAtLeast(waitAtLeastThisLong);
            switch (tickCode)
            {
                case 0:
                    cricutStatus = usb_curveIn(x0, y0, x1, y1, x2, y2, x3, y3);
                    break;

                case 1:
                    cricutStatus = usb_curveOut(x0, y0, x1, y1, x2, y2, x3, y3);
                    break;
            }
            resetWait(shortDelay);
        }
    }
}