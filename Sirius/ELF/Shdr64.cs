using System;
using System.IO;
using System.Text;

namespace Sirius
{
    public class Shdr64
    {
        public uint sh_name, sh_type;
        public ulong sh_flags, sh_addr, sh_offset, sh_size;
        public uint sh_link, sh_info;
        public ulong sh_addralign, sh_entsize;

        public string Name { get; set; }

        public void Read(StringBuilder sb, BinaryReader br, ulong stroff)
        {
            sb.AppendFormat("{0:x8}: sh_name     : ", br.BaseStream.Position);
            sh_name = br.ReadUInt32();
            Name = ELF.ReadString(br, (long)(stroff + sh_name));
            sb.AppendFormat("{0:x8}", sh_name);
            if (!string.IsNullOrEmpty(Name)) sb.AppendFormat(" => {0}", Name);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: sh_type     : ", br.BaseStream.Position);
            sh_type = br.ReadUInt32();
            sb.AppendFormat("{0:x8}", sh_type);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: sh_flags    : ", br.BaseStream.Position);
            sh_flags = br.ReadUInt64();
            sb.AppendFormat("{0:x16}", sh_flags);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: sh_addr     : ", br.BaseStream.Position);
            sh_addr = br.ReadUInt64();
            sb.AppendFormat("{0:x16}", sh_addr);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: sh_offset   : ", br.BaseStream.Position);
            sh_offset = br.ReadUInt64();
            sb.AppendFormat("{0:x16}", sh_offset);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: sh_size     : ", br.BaseStream.Position);
            sh_size = br.ReadUInt64();
            sb.AppendFormat("{0:x16}", sh_size);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: sh_link     : ", br.BaseStream.Position);
            sh_link = br.ReadUInt32();
            sb.AppendFormat("{0:x8}", sh_link);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: sh_info     : ", br.BaseStream.Position);
            sh_info = br.ReadUInt32();
            sb.AppendFormat("{0:x8}", sh_info);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: sh_addralign: ", br.BaseStream.Position);
            sh_addralign = br.ReadUInt64();
            sb.AppendFormat("{0:x16}", sh_addralign);
            sb.AppendLine();

            sb.AppendFormat("{0:x8}: sh_entsize  : ", br.BaseStream.Position);
            sh_entsize = br.ReadUInt64();
            sb.AppendFormat("{0:x16}", sh_entsize);
            sb.AppendLine();
        }
    }
}
