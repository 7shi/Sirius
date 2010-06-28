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

        private ulong memoryStart, memoryEnd;
        private byte[] memory;

        private void InitMemory(byte[] data)
        {
            memoryStart = elf.Start;
            memoryEnd = elf.End;
            memory = new byte[memoryEnd - memoryStart];
            foreach (var sh in elf.Headers)
                Array.Copy(data, (int)sh.sh_offset,
                    memory, (int)(sh.sh_addr - memoryStart), (int)sh.sh_size);
        }

        private struct MemoryPtr
        {
            public byte[] Buf;
            public int Ptr;

            public MemoryPtr(byte[] buf, int ptr)
            {
                Buf = buf;
                Ptr = ptr;
            }
        }

        private MemoryPtr GetPtr(ulong addr, ulong size)
        {
            if (addr >= memoryStart && addr <= memoryEnd - size)
                return new MemoryPtr(memory, (int)(addr - memoryStart));
            else if (addr >= stackStart && addr <= stackEnd - size)
                return new MemoryPtr(stack, (int)(addr - stackStart));
            throw Abort("不正なアドレス: {0:x16}", addr);
        }

        public void Write64(ulong addr, ulong v)
        {
            var mp = GetPtr(addr, 8);
            Array.Copy(BitConverter.GetBytes(v), 0, mp.Buf, mp.Ptr, 8);
        }

        public void Write32(ulong addr, uint v)
        {
            var mp = GetPtr(addr, 4);
            Array.Copy(BitConverter.GetBytes(v), 0, mp.Buf, mp.Ptr, 4);
        }

        public void Write16(ulong addr, ushort v)
        {
            var mp = GetPtr(addr, 2);
            Array.Copy(BitConverter.GetBytes(v), 0, mp.Buf, mp.Ptr, 2);
        }

        public void Write8(ulong addr, byte v)
        {
            if (addr == 0x10000000)
            {
                output.Append((char)v);
                return;
            }
            var mp = GetPtr(addr, 8);
            mp.Buf[mp.Ptr] = v;
        }

        public ulong Read64(ulong addr)
        {
            var mp = GetPtr(addr, 8);
            return BitConverter.ToUInt64(mp.Buf, mp.Ptr);
        }

        public uint Read32(ulong addr)
        {
            var mp = GetPtr(addr, 4);
            return BitConverter.ToUInt32(mp.Buf, mp.Ptr);
        }

        public ushort Read16(ulong addr)
        {
            var mp = GetPtr(addr, 2);
            return BitConverter.ToUInt16(mp.Buf, mp.Ptr);
        }

        public byte Read8(ulong addr)
        {
            var mp = GetPtr(addr, 8);
            return mp.Buf[mp.Ptr];
        }
    }
}
