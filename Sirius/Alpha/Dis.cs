using System;
using System.IO;
using System.Text;

namespace Sirius
{
    public partial class Alpha
    {
        private ELF64 elf;
        private uint[] text_code;
        private Op[] text_op;

        public Alpha(ELF64 elf, byte[] data)
        {
            this.elf = elf;
            InitMemory(data);
            text_code = new uint[elf.Text.sh_size >> 2];
            text_op = new Op[text_code.Length];
        }

        public void Disassemble(StringBuilder sb)
        {
            var text = elf.Text;
            ulong addr = text.sh_addr, end = addr + text.sh_size, off = text.sh_offset;
            for (int p = 0; addr < end; p++, addr += 4, off += 4)
            {
                sb.AppendFormat("{0:x8}: ", off);
                if (off != addr) sb.AppendFormat("[{0:x8}] ", addr);
                text_op[p] = Disassemble(sb, addr, text_code[p] = Read32(addr));
                sb.AppendLine();
            }
        }

        public static Op Disassemble(uint code)
        {
            int op = (int)(code >> 26);
            switch (op)
            {
                case 0x00: return Op.Call_pal;
                case 0x01: return Op.Opc01;
                case 0x02: return Op.Opc02;
                case 0x03: return Op.Opc03;
                case 0x04: return Op.Opc04;
                case 0x05: return Op.Opc05;
                case 0x06: return Op.Opc06;
                case 0x07: return Op.Opc07;
                case 0x08: return Op.Lda;
                case 0x09: return Op.Ldah;
                case 0x0a: return Op.Ldbu;
                case 0x0b: return Op.Ldq_u;
                case 0x0c: return Op.Ldwu;
                case 0x0d: return Op.Stw;
                case 0x0e: return Op.Stb;
                case 0x0f: return Op.Stq_u;
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13: return subops[op][(code >> 5) & 0x7f];
                case 0x14:
                case 0x15:
                case 0x16:
                case 0x17: return subops[op][(code >> 5) & 0x7ff];
                case 0x18:
                    switch (code & 0xffff)
                    {
                        case 0x0000: return Op.Trapb;
                        case 0x0400: return Op.Excb;
                        case 0x4000: return Op.Mb;
                        case 0x4400: return Op.Wmb;
                        case 0x8000: return Op.Fetch;
                        case 0xa000: return Op.Fetch_m;
                        case 0xc000: return Op.Rpcc;
                        case 0xe000: return Op.Rc;
                        case 0xf000: return Op.Rs;
                        case 0xe800: return Op.Ecb;
                        case 0xf800: return Op.Wh64;
                        case 0xfc00: return Op.Wh64en;
                    }
                    break;
                case 0x19: return Op.Pal19;
                case 0x1a: return subops[op][(code >> 14) & 3];
                case 0x1b: return Op.Pal1b;
                case 0x1c: return subops[op][(code >> 5) & 0x7f];
                case 0x1d: return Op.Pal1d;
                case 0x1e: return Op.Pal1e;
                case 0x1f: return Op.Pal1f;
                case 0x20: return Op.Ldf;
                case 0x21: return Op.Ldg;
                case 0x22: return Op.Lds;
                case 0x23: return Op.Ldt;
                case 0x24: return Op.Stf;
                case 0x25: return Op.Stg;
                case 0x26: return Op.Sts;
                case 0x27: return Op.Stt;
                case 0x28: return Op.Ldl;
                case 0x29: return Op.Ldq;
                case 0x2a: return Op.Ldl_l;
                case 0x2b: return Op.Ldq_l;
                case 0x2c: return Op.Stl;
                case 0x2d: return Op.Stq;
                case 0x2e: return Op.Stl_c;
                case 0x2f: return Op.Stq_c;
                case 0x30: return Op.Br;
                case 0x31: return Op.Fbeq;
                case 0x32: return Op.Fblt;
                case 0x33: return Op.Fble;
                case 0x34: return Op.Bsr;
                case 0x35: return Op.Fbne;
                case 0x36: return Op.Fbge;
                case 0x37: return Op.Fbgt;
                case 0x38: return Op.Blbc;
                case 0x39: return Op.Beq;
                case 0x3a: return Op.Blt;
                case 0x3b: return Op.Ble;
                case 0x3c: return Op.Blbs;
                case 0x3d: return Op.Bne;
                case 0x3e: return Op.Bge;
                case 0x3f: return Op.Bgt;
            }
            return Op.___;
        }

        public static string GetMnemonic(Op op)
        {
            return op.ToString().ToLower().Replace("__", "/");
        }

        public static Op Disassemble(StringBuilder sb, ulong addr, uint code)
        {
            var op = Disassemble(code);
            int opc = (int)(code >> 26);
            var mne = GetMnemonic(op);
            sb.AppendFormat("{0:x8} => {1:x2}", code, opc);
            switch (formats[opc])
            {
                default:
                    sb.AppendFormat("                   => {0}", mne);
                    break;
                case Format.Pcd:
                    {
                        uint pal = code & 0x03ffffff;
                        sb.AppendFormat("      {0:x8}     => {1,-7} {0:x8}", pal, mne);
                        break;
                    }
                case Format.Bra:
                    {
                        int ra = (int)((code >> 21) & 31);
                        uint disp = code & 0x001fffff;
                        var sdisp = disp < 0x00100000
                            ? string.Format("{0:x8}", addr + disp * 4 + 4)
                            : string.Format("{0:x8}", addr - (0x00200000 - disp) * 4 + 4);
                        sb.AppendFormat("      r{0:00} {1:x8} => {2,-7} {3},{4}",
                            ra, disp, mne, regname[ra], sdisp);
                        if (ra == 31 && op == Op.Br)
                            sb.AppendFormat(" => br {0}", sdisp);
                        break;
                    }
                case Format.Mem:
                    {
                        int ra = (int)((code >> 21) & 31);
                        int rb = (int)((code >> 16) & 31);
                        int disp = (int)(code & 0xffff);
                        var args = disp < 0x8000
                            ? string.Format("{0:x}", disp)
                            : string.Format("-{0:x}", 0x10000 - disp);
                        if (rb != 31) args += "(" + regname[rb] + ")";
                        sb.AppendFormat("     ", (code >> 14) & 3);
                        sb.AppendFormat(" r{0:00} r{1:00} {2:x4}", ra, rb, disp);
                        sb.AppendFormat(" => {0,-7} {1},", mne, regname[ra]);
                        sb.Append(args);
                        if (ra == 31)
                        {
                            if (disp == 0 && op == Op.Ldq_u)
                                sb.Append(" => unop");
                            else
                            {
                                var pse = "";
                                switch (op)
                                {
                                    case Op.Ldl: pse = "prefetch"; break;
                                    case Op.Ldq: pse = "prefetch_en"; break;
                                    case Op.Lds: pse = "prefetch_m"; break;
                                    case Op.Ldt: pse = "prefetch_men"; break;
                                }
                                if (pse != "")
                                    sb.AppendFormat("{0} {1}", pse, args);
                            }
                        }
                        break;
                    }
                case Format.Mfc:
                    {
                        int ra = (int)((code >> 21) & 31);
                        int rb = (int)((code >> 16) & 31);
                        sb.AppendFormat(".{0:x4} r{1:00} r{2:00}      => {3,-7} {4},{5}",
                            code & 0xffff, ra, rb, mne, regname[ra], regname[rb]);
                        break;
                    }
                case Format.Mbr:
                    {
                        int ra = (int)((code >> 21) & 31);
                        int rb = (int)((code >> 16) & 31);
                        int disp = (int)(code & 0x3fff);
                        sb.AppendFormat(".{0:x}   ", (code >> 14) & 3);
                        sb.AppendFormat(" r{0:00} r{1:00} {2:x4}", ra, rb, disp);
                        sb.AppendFormat(" => {0,-7} {1},({2}),{3:x4}",
                            mne, regname[ra], regname[rb], disp);
                        break;
                    }
                case Format.Opr:
                    {
                        int ra = (int)((code >> 21) & 31);
                        int rb = -1;
                        int rc = (int)(code & 31);
                        sb.AppendFormat(".{0:x2}  ", (code >> 5) & 0x7f);
                        string arg2;
                        if ((code & 0x1000) == 0)
                        {
                            rb = (int)((code >> 16) & 31);
                            sb.AppendFormat(" r{0:00} r{1:00} r{2:00} ", ra, rb, rc);
                            arg2 = regname[rb];
                        }
                        else
                        {
                            arg2 = string.Format("{0:x2}", (code >> 13) & 0xff);
                            sb.AppendFormat(" r{0:00}  {1} r{2:00} ", ra, arg2, rc);
                        }
                        sb.AppendFormat(" => {0,-7} {1},{2},{3}",
                            mne, regname[ra], arg2, regname[rc]);
                        if (ra == 31)
                        {
                            string pse = "";
                            switch (op)
                            {
                                case Op.Bis:
                                    if (rb == 31 && rc == 31)
                                        sb.Append(" => nop");
                                    else if (rb == 31)
                                        sb.AppendFormat(" => clr {0}", regname[rc]);
                                    else
                                        pse = "mov";
                                    break;
                                case Op.Addl: pse = "sextl"; break;
                                case Op.Ornot: pse = "not"; break;
                                case Op.Subl: pse = "negl"; break;
                                case Op.Subl__v: pse = "negl/v"; break;
                                case Op.Subq: pse = "negq"; break;
                                case Op.Subq__v: pse = "negq/v"; break;
                            }
                            if (pse != "")
                                sb.AppendFormat(" => {0} {1},{2}", pse, arg2, regname[rc]);
                        }
                        break;
                    }
                case Format.F_P:
                    {
                        int fa = (int)((code >> 21) & 31);
                        int fb = (int)((code >> 16) & 31);
                        int fc = (int)(code & 31);
                        sb.AppendFormat(".{0:x3} ", (code >> 5) & 0x7ff);
                        sb.AppendFormat(" f{0:00} f{1:00} f{2:00}  => {3,-7} f{0:00},f{1:00},f{2:00}",
                            fa, fb, fc, mne);
                        int pst = 2;
                        var pse = "";
                        if (fa == 31)
                            switch (op)
                            {
                                case Op.Cpys:
                                    if (fb == 31 && fc == 31)
                                    {
                                        pst = 0;
                                        pse = "fnop";
                                    }
                                    else if (fb == 31)
                                    {
                                        pst = 1;
                                        pse = "fclr";
                                    }
                                    else
                                        pse = "fabs";
                                    break;
                                case Op.Subf: pse = "negf"; break;
                                case Op.Subf__s: pse = "negf/s"; break;
                                case Op.Subg: pse = "negg"; break;
                                case Op.Subg__s: pse = "negg/s"; break;
                                case Op.Subs: pse = "negs"; break;
                                case Op.Subs__su: pse = "negs/su"; break;
                                case Op.Subs__sui: pse = "negs/sui"; break;
                                case Op.Subt: pse = "negt"; break;
                                case Op.Subt__su: pse = "negt/su"; break;
                                case Op.Subt__sui: pse = "negt/sui"; break;
                            };
                        if (pse == "" && fa == fb)
                            switch (op)
                            {
                                case Op.Cpys: pse = "fmov"; break;
                                case Op.Cpysn: pse = "fneg"; break;
                            }
                        if (pse == "" && fa == fb && fb == fc)
                            switch (op)
                            {
                                case Op.Mf_fpcr:
                                case Op.Mt_fpcr:
                                    pst = 1;
                                    pse = mne;
                                    break;
                            }
                        if (pse != "")
                            switch (pst)
                            {
                                case 0:
                                    sb.AppendFormat(" => {0}", pse);
                                    break;
                                case 1:
                                    sb.AppendFormat(" => {0} f{1:00}", pse, fc);
                                    break;
                                case 2:
                                    sb.AppendFormat(" => {0} f{1:00},f{2:00}", pse, fb, fc);
                                    break;
                            }
                        break;
                    }
            }
            return op;
        }
    }
}
