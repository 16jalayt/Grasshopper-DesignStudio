using System;
using System.Runtime.InteropServices;

namespace Cricut_Design_Studio
{
    public class USB_Interface
    {
        public enum ListDeviceBy
        {
            SerialNo,
            Description,
            Location
        }

        public bool driverOkay;

        private static int timeoutCounter = 10000;

        public bool deviceOpen;

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DllLoadFTDILibrary();

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint FT_CheckLoadStatus();

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint FT_GetLibraryVersion(ref ulong ver);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint FT_ListDevices();

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint FT_ListDevices_SerNo(int devIndex, [MarshalAs(UnmanagedType.LPArray)] byte[] p_data);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint FT_ListDevices_Desc(int devIndex, [MarshalAs(UnmanagedType.LPArray)] byte[] p_data);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint FT_Open(int devIndex);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint FT_Close();

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint FT_SetDivisor(ushort divisor);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint FT_SetBaudRate(ulong baud);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint FT_Write([MarshalAs(UnmanagedType.LPArray)] byte[] p_data, ulong size);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint FT_Read([MarshalAs(UnmanagedType.LPArray)] byte[] p_data, ulong size);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint FT_GetStatus(ref ulong rxsize, ref ulong txsize);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint FT_EE_Read(ref ushort vid, ref ushort pid, ref ushort power);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint FT_EE_Program(ushort power);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint FT_EE_ProgramToDefault();

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void usb_setTimeout(int tc, int rs);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void usb_writeBlock([MarshalAs(UnmanagedType.LPArray)] byte[] data, int start, int size);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void usb_sendChar(byte c);

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int usb_receiveChar();

        [DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int usb_getLength();

        public USB_Interface()
        {
            ulong ver = 0uL;
            if (!DllLoadFTDILibrary())
            {
                Console.WriteLine("Can't load FTDI Library DLL!");
                return;
            }
            uint num;
            try
            {
                num = FT_CheckLoadStatus();
                FT_GetLibraryVersion(ref ver);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            if (1 == num && ver >= 196882)
            {
                driverOkay = true;
            }
            Random random = new Random();
            usb_setTimeout(timeoutCounter, random.Next(0, 32767));
        }

        public static void setTimeout(int tc)
        {
            timeoutCounter = tc;
            usb_setTimeout(timeoutCounter, -1);
        }

        public uint listDevices()
        {
            return FT_ListDevices();
        }

        public string listDevice(int devIndex, ListDeviceBy by)
        {
            byte[] array = new byte[64];
            uint num = 0u;
            switch (by)
            {
                case ListDeviceBy.Description:
                    num = FT_ListDevices_Desc(devIndex, array);
                    break;

                case ListDeviceBy.SerialNo:
                    num = FT_ListDevices_SerNo(devIndex, array);
                    break;

                default:
                    return null;
            }
            if (num == 0)
            {
                string text = "";
                byte[] array2 = array;
                foreach (byte b in array2)
                {
                    if (b == 0)
                    {
                        break;
                    }
                    text += (char)b;
                }
                return text;
            }
            return null;
        }

        public string listDeviceSerNo(int devIndex)
        {
            return listDevice(devIndex, ListDeviceBy.SerialNo);
        }

        public string listDeviceDesc(int devIndex)
        {
            return listDevice(devIndex, ListDeviceBy.Description);
        }

        public bool openDevice(int devIndex)
        {
            if (deviceOpen)
            {
                return false;
            }
            if (FT_Open(devIndex) == 0)
            {
                deviceOpen = true;
                return true;
            }
            return false;
        }

        public bool closeDevice()
        {
            if (deviceOpen)
            {
                FT_Close();
                deviceOpen = true;
                return true;
            }
            return false;
        }

        public void write(byte[] data, int start, int size)
        {
            usb_writeBlock(data, start, size);
        }

        public void sendChar(byte c)
        {
            usb_sendChar(c);
        }

        public int receiveChar()
        {
            return usb_receiveChar();
        }

        public int getLength()
        {
            return usb_getLength();
        }

        public string[] Open(int baud, string serialNo, string description)
        {
            if (serialNo == null)
            {
                serialNo = "";
            }
            if (description == null)
            {
                description = "USB <-> Serial";
            }
            uint num = listDevices();
            if (num == 0)
            {
                return null;
            }
            string[] array = new string[num];
            for (int i = 0; i < num; i++)
            {
                string text = listDeviceSerNo(i);
                string text2 = listDeviceDesc(i);
                array[i] = text + "," + text2;
                if (text != null && text.CompareTo(serialNo) == 0 && text2 != null && text2.CompareTo(description) == 0 && !deviceOpen)
                {
                    openDevice(i);
                }
            }
            if (deviceOpen)
            {
                FT_SetBaudRate((ulong)baud);
            }
            return array;
        }

        public void Close()
        {
            closeDevice();
        }

        public bool isDriverOkay()
        {
            return driverOkay;
        }

        public bool isAvailable()
        {
            if (deviceOpen)
            {
                return driverOkay;
            }
            return false;
        }
    }
}