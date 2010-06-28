using System;
using System.IO;
using System.Text;

namespace Sirius
{
    public class ELF64 : ELF
    {
        public byte[] e_ident = new byte[EI_NIDENT];
        public ushort e_type, e_machine;
        public uint e_version;
        public ulong e_entry, e_phoff, e_shoff;
        public uint e_flags;
        public ushort e_ehsize, e_phentsize, e_phnum, e_shentsize, e_shnum, e_shstrndx;

        public Shdr64 Text { get; set; }
        public byte[] textbuf;

        public void Read(StringBuilder sb, BinaryReader br)
        {
            sb.AppendFormat("{0:x8}: e_ident[EI_MAG0   ]: ", br.BaseStream.Position);
            e_ident[EI_MAG0] = br.ReadByte();
            sb.AppendFormat("{0:x2}", e_ident[EI_MAG0]);
            sb.AppendLine();
            if (e_ident[EI_MAG0] != 0x7f) throw new Exception("EI_MAG0 != 0x7f");

            sb.AppendFormat("{0:x8}: e_ident[EI_MAG1   ]: ", br.BaseStream.Position);
            e_ident[EI_MAG1] = br.ReadByte();
            sb.AppendFormat("'{0}'", (char)e_ident[EI_MAG1]);
            sb.AppendLine();
            if (e_ident[EI_MAG1] != 'E') throw new Exception("EI_MAG1 != 'E'");

            sb.AppendFormat("{0:x8}: e_ident[EI_MAG2   ]: ", br.BaseStream.Position);
            e_ident[EI_MAG2] = br.ReadByte();
            sb.AppendFormat("'{0}'", (char)e_ident[EI_MAG2]);
            sb.AppendLine();
            if (e_ident[EI_MAG2] != 'L') throw new Exception("EI_MAG2 != 'L'");

            sb.AppendFormat("{0:x8}: e_ident[EI_MAG3   ]: ", br.BaseStream.Position);
            e_ident[EI_MAG3] = br.ReadByte();
            sb.AppendFormat("'{0}'", (char)e_ident[EI_MAG3]);
            sb.AppendLine();
            if (e_ident[EI_MAG3] != 'F') throw new Exception("EI_MAG3 != 'F'");

            sb.AppendFormat("{0:x8}: e_ident[EI_CLASS  ]: ", br.BaseStream.Position);
            e_ident[EI_CLASS] = br.ReadByte();
            sb.AppendFormat("{0:x2}", e_ident[EI_CLASS]);
            sb.AppendLine();
            if (e_ident[EI_CLASS] != ELFCLASS64) throw new Exception("EI_CLASS != ELFCLASS64");

            sb.AppendFormat("{0:x8}: e_ident[EI_DATA   ]: ", br.BaseStream.Position);
            e_ident[EI_DATA] = br.ReadByte();
            sb.AppendFormat("{0:x2}", e_ident[EI_DATA]);
            sb.AppendLine();
            if (e_ident[EI_DATA] != ELFDATA2LSB) throw new Exception("EI_DATA != ELFDATA2LSB");

            sb.AppendFormat("{0:x8}: e_ident[EI_VERSION]: ", br.BaseStream.Position);
            e_ident[EI_VERSION] = br.ReadByte();
            sb.AppendFormat("{0:x2}", e_ident[EI_VERSION]);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: e_ident[EI_PAD    ]:", br.BaseStream.Position);
            for (int i = EI_PAD; i < EI_NIDENT; i++)
            {
                sb.AppendFormat(" ");
                e_ident[i] = br.ReadByte();
                sb.AppendFormat("{0:x2}", e_ident[i]);
            }
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: e_type      : ", br.BaseStream.Position);
            e_type = br.ReadUInt16();
            sb.AppendFormat("{0:x4}", e_type);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: e_machine   : ", br.BaseStream.Position);
            e_machine = br.ReadUInt16();
            sb.AppendFormat("{0:x4}", e_machine);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: e_version   : ", br.BaseStream.Position);
            e_version = br.ReadUInt32();
            sb.AppendFormat("{0:x8}", e_version);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: e_entry     : ", br.BaseStream.Position);
            e_entry = br.ReadUInt64();
            sb.AppendFormat("{0:x16}", e_entry);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: e_phoff     : ", br.BaseStream.Position);
            e_phoff = br.ReadUInt64();
            sb.AppendFormat("{0:x16}", e_phoff);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: e_shoff     : ", br.BaseStream.Position);
            e_shoff = br.ReadUInt64();
            sb.AppendFormat("{0:x16}", e_shoff);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: e_flags     : ", br.BaseStream.Position);
            e_flags = br.ReadUInt32();
            sb.AppendFormat("{0:x8}", e_flags);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: e_ehsize    : ", br.BaseStream.Position);
            e_ehsize = br.ReadUInt16();
            sb.AppendFormat("{0:x4}", e_ehsize);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: e_phentsize : ", br.BaseStream.Position);
            e_phentsize = br.ReadUInt16();
            sb.AppendFormat("{0:x4}", e_phentsize);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: e_phnum     : ", br.BaseStream.Position);
            e_phnum = br.ReadUInt16();
            sb.AppendFormat("{0:x4}", e_phnum);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: e_shentsize : ", br.BaseStream.Position);
            e_shentsize = br.ReadUInt16();
            sb.AppendFormat("{0:x4}", e_shentsize);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: e_shnum     : ", br.BaseStream.Position);
            e_shnum = br.ReadUInt16();
            sb.AppendFormat("{0:x4}", e_shnum);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: e_shstrndx  : ", br.BaseStream.Position);
            e_shstrndx = br.ReadUInt16();
            sb.AppendFormat("{0:x4}", e_shstrndx);
            ulong stroff = 0;
            if (e_shstrndx != 0)
            {
                br.BaseStream.Position = (long)(e_shoff + (ulong)(e_shstrndx * e_shentsize) + 24);
                stroff = br.ReadUInt64();
                sb.AppendFormat(" => {0:x16}", stroff);
            }
            sb.AppendLine();

            if (e_phoff != 0)
            {
                br.BaseStream.Position = (long)e_phoff;
                for (int i = 0; i < e_phnum; i++)
                {
                    sb.AppendLine();
                    new Phdr64().Read(sb, br);
                }
            }

            if (e_shoff != 0)
            {
                br.BaseStream.Position = (long)e_shoff;
                for (int i = 0; i < e_shnum; i++)
                {
                    sb.AppendLine();
                    var sh = new Shdr64();
                    sh.Read(sb, br, stroff);
                    if (sh.Name == ".text")
                    {
                        Text = sh;
                        var pos = br.BaseStream.Position;
                        br.BaseStream.Position = (long)sh.sh_offset;
                        textbuf = br.ReadBytes((int)sh.sh_size);
                        br.BaseStream.Position = pos;
                    }
                }
            }
        }

        public void Disassemble(StringBuilder sb)
        {
            if (Text == null) return;

            switch (e_machine)
            {
                case EM_ALPHA_EXP:
                    ulong addr = Text.sh_addr, off = Text.sh_offset;
                    for (int i = 0; i < (int)Text.sh_size; i += 4, addr += 4, off += 4)
                    {
                        sb.AppendFormat("{0:x8}: ", off);
                        if (off != addr) sb.AppendFormat("[{0:x8}] ", addr);
                        Alpha.Disassemble(sb, addr, BitConverter.ToUInt32(textbuf, i));
                        sb.AppendLine();
                    }
                    break;
                default:
                    throw new Exception("Alpha以外はサポートされていません。");
            }
        }
    }
}
