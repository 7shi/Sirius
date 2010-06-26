using System;
using System.IO;
using System.Text;

namespace Sirius
{
    public class ELF
    {
        public const int EI_MAG0 = 0;
        public const int EI_MAG1 = 1;
        public const int EI_MAG2 = 2;
        public const int EI_MAG3 = 3;
        public const int EI_CLASS = 4;
        public const int EI_DATA = 5;
        public const int EI_VERSION = 6;
        public const int EI_PAD = 7;
        public const int EI_NIDENT = 16;

        public const int ELFCLASSNONE = 0;
        public const int ELFCLASS32 = 1;
        public const int ELFCLASS64 = 2;

        public const int ELFDATANONE = 0;
        public const int ELFDATA2LSB = 1;
        public const int ELFDATA2MSB = 2;

        public const int EM_ALPHA_EXP = 0x9026;

        public static string ReadString(BinaryReader br, long off)
        {
            var sb = new StringBuilder();
            long pos = br.BaseStream.Position;
            br.BaseStream.Position = off;
            byte b;
            while ((b = br.ReadByte()) != 0) sb.Append((char)b);
            br.BaseStream.Position = pos;
            return sb.ToString();
        }
    }
}
