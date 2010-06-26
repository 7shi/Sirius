using System;
using System.IO;
using System.Text;

namespace Sirius
{
    public class Phdr64
    {
        public uint p_type, p_flags;
        public ulong p_offset, p_vaddr, p_paddr;
        public ulong p_filesz, p_memsz, p_align;

        public void Read(StringBuilder sb, BinaryReader br)
        {
            sb.AppendFormat("{0:x8}: p_type      : ", br.BaseStream.Position);
            p_type = br.ReadUInt32();
            sb.AppendFormat("{0:x8}", p_type);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: p_flags     : ", br.BaseStream.Position);
            p_flags = br.ReadUInt32();
            sb.AppendFormat("{0:x8}", p_flags);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: p_offset    : ", br.BaseStream.Position);
            p_offset = br.ReadUInt64();
            sb.AppendFormat("{0:x16}", p_offset);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: p_vaddr     : ", br.BaseStream.Position);
            p_vaddr = br.ReadUInt64();
            sb.AppendFormat("{0:x16}", p_vaddr);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: p_paddr     : ", br.BaseStream.Position);
            p_paddr = br.ReadUInt64();
            sb.AppendFormat("{0:x16}", p_paddr);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: p_filesz    : ", br.BaseStream.Position);
            p_filesz = br.ReadUInt64();
            sb.AppendFormat("{0:x16}", p_filesz);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: p_memsz     : ", br.BaseStream.Position);
            p_memsz = br.ReadUInt64();
            sb.AppendFormat("{0:x16}", p_memsz);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: p_align     : ", br.BaseStream.Position);
            p_align = br.ReadUInt64();
            sb.AppendFormat("{0:x16}", p_align);
            sb.AppendLine();
        }
    }
}
