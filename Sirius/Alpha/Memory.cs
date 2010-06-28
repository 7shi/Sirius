using System;
using System.IO;
using System.Text;

namespace Sirius
{
    public partial class Alpha
    {
        private const ulong stackStart = 0x00f00000;
        private const ulong stackSize = 1024 * 1024; // 1MB
        private const ulong stackEnd = stackStart + stackSize;

        private byte[] stack = new byte[stackSize]; // 00f00000-01000000

        public void CheckAddr(ulong addr, ulong size)
        {
            if (addr < stackStart || addr > stackEnd - size)
                throw Abort("不正なアドレス: {0:x16}", addr);
        }

        public void Write64(ulong addr, ulong v)
        {
            CheckAddr(addr, 8);
            Array.Copy(BitConverter.GetBytes(v), 0, stack, (int)(addr - stackStart), 8);
        }

        public void Write32(ulong addr, uint v)
        {
            CheckAddr(addr, 4);
            Array.Copy(BitConverter.GetBytes(v), 0, stack, (int)(addr - stackStart), 4);
        }

        public void Write16(ulong addr, ushort v)
        {
            CheckAddr(addr, 2);
            Array.Copy(BitConverter.GetBytes(v), 0, stack, (int)(addr - stackStart), 2);
        }

        public void Write8(ulong addr, byte v)
        {
            if (addr == 0x10000000)
            {
                output.Append((char)v);
                return;
            }
            CheckAddr(addr, 1);
            stack[addr - stackStart] = v;
        }

        public ulong Read64(ulong addr)
        {
            CheckAddr(addr, 8);
            return BitConverter.ToUInt64(stack, (int)(addr - stackStart));
        }

        public uint Read32(ulong addr)
        {
            CheckAddr(addr, 4);
            return BitConverter.ToUInt32(stack, (int)(addr - stackStart));
        }

        public ushort Read16(ulong addr)
        {
            CheckAddr(addr, 2);
            return BitConverter.ToUInt16(stack, (int)(addr - stackStart));
        }

        public byte Read8(ulong addr)
        {
            CheckAddr(addr, 1);
            return stack[addr - stackStart];
        }
    }
}
