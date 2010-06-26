using System;
using System.IO;
using System.Text;

namespace Sirius
{
    public enum Format
    {
        Unknown, Pcd, Bra, Mem, Mbr, Mfc, Opr, FP
    }

    public struct AlphaOp
    {
        public string Mnemonic;
        public Format Type;

        public AlphaOp(string mnemonic)
        {
            Mnemonic = mnemonic;
            Type = Format.Unknown;
        }

        public AlphaOp(string mnemonic, Format type)
        {
            Mnemonic = mnemonic;
            Type = type;
        }
    }

    public class Alpha
    {
        private static string[] regnames = new[]
        {
            /* r00     */ "v0",
            /* r01-r08 */ "t0", "t1", "t2", "t3", "t4", "t5", "t6", "t7",
            /* r09-r14 */ "s0", "s1", "s2", "s3", "s4", "s5",
            /* r15     */ "fp",
            /* r16-r21 */ "a0", "a1", "a2", "a3", "a4", "a5",
            /* r22-r25 */ "t8", "t9", "t10", "t11",
            /* r26     */ "ra",
            /* r27     */ "t12",
            /* r28     */ "at",
            /* r29     */ "gp",
            /* r30     */ "sp",
            /* r31     */ "zero",
        };

        public static AlphaOp Disassemble(uint code)
        {
            int fpf = (int)((code >> 5) & 0x7ff), fpt = 0;
            string mne = null;
            switch (code >> 26)
            {
                case 0x00: return new AlphaOp("call_pal", Format.Pcd);
                case 0x01: return new AlphaOp("opc01");
                case 0x02: return new AlphaOp("opc02");
                case 0x03: return new AlphaOp("opc03");
                case 0x04: return new AlphaOp("opc04");
                case 0x05: return new AlphaOp("opc05");
                case 0x06: return new AlphaOp("opc06");
                case 0x07: return new AlphaOp("opc07");
                case 0x08: return new AlphaOp("lda", Format.Mem);
                case 0x09: return new AlphaOp("ldah", Format.Mem);
                case 0x0a: return new AlphaOp("ldbu", Format.Mem);
                case 0x0b: return new AlphaOp("ldq_u", Format.Mem);
                case 0x0c: return new AlphaOp("ldwu", Format.Mem);
                case 0x0d: return new AlphaOp("stw", Format.Mem);
                case 0x0e: return new AlphaOp("stb", Format.Mem);
                case 0x0f: return new AlphaOp("stq_u", Format.Mem);
                case 0x10:
                    switch ((code >> 5) & 0x7f)
                    {
                        case 0x00: return new AlphaOp("addl", Format.Opr);
                        case 0x02: return new AlphaOp("s4addl", Format.Opr);
                        case 0x09: return new AlphaOp("subl", Format.Opr);
                        case 0x0b: return new AlphaOp("s4subl", Format.Opr);
                        case 0x0f: return new AlphaOp("cmpbge", Format.Opr);
                        case 0x12: return new AlphaOp("s8addl", Format.Opr);
                        case 0x1b: return new AlphaOp("s8subl", Format.Opr);
                        case 0x1d: return new AlphaOp("cmpult", Format.Opr);
                        case 0x20: return new AlphaOp("addq", Format.Opr);
                        case 0x22: return new AlphaOp("s4addq", Format.Opr);
                        case 0x29: return new AlphaOp("subq", Format.Opr);
                        case 0x2b: return new AlphaOp("s4subq", Format.Opr);
                        case 0x2d: return new AlphaOp("cmpeq", Format.Opr);
                        case 0x32: return new AlphaOp("s8addq", Format.Opr);
                        case 0x3b: return new AlphaOp("s8subq", Format.Opr);
                        case 0x3d: return new AlphaOp("cmpule", Format.Opr);
                        case 0x40: return new AlphaOp("addl/v", Format.Opr);
                        case 0x49: return new AlphaOp("subl/v", Format.Opr);
                        case 0x4d: return new AlphaOp("cmplt", Format.Opr);
                        case 0x60: return new AlphaOp("addq/v", Format.Opr);
                        case 0x69: return new AlphaOp("subq/v", Format.Opr);
                        case 0x6d: return new AlphaOp("cmple", Format.Opr);
                    }
                    break;
                case 0x11:
                    switch ((code >> 5) & 0x7f)
                    {
                        case 0x00: return new AlphaOp("and", Format.Opr);
                        case 0x08: return new AlphaOp("bic", Format.Opr);
                        case 0x14: return new AlphaOp("cmovlbs", Format.Opr);
                        case 0x16: return new AlphaOp("cmovlbc", Format.Opr);
                        case 0x20: return new AlphaOp("bis", Format.Opr);
                        case 0x24: return new AlphaOp("cmoveq", Format.Opr);
                        case 0x26: return new AlphaOp("cmovne", Format.Opr);
                        case 0x28: return new AlphaOp("ornot", Format.Opr);
                        case 0x40: return new AlphaOp("xor", Format.Opr);
                        case 0x44: return new AlphaOp("cmovlt", Format.Opr);
                        case 0x46: return new AlphaOp("cmovge", Format.Opr);
                        case 0x48: return new AlphaOp("eqv", Format.Opr);
                        case 0x61: return new AlphaOp("amask", Format.Opr);
                        case 0x64: return new AlphaOp("cmovle", Format.Opr);
                        case 0x66: return new AlphaOp("cmovgt", Format.Opr);
                        case 0x6c: return new AlphaOp("implver", Format.Opr);
                    }
                    break;
                case 0x12:
                    switch ((code >> 5) & 0x7f)
                    {
                        case 0x02: return new AlphaOp("mskbl", Format.Opr);
                        case 0x06: return new AlphaOp("extbl", Format.Opr);
                        case 0x0b: return new AlphaOp("insbl", Format.Opr);
                        case 0x12: return new AlphaOp("mskwl", Format.Opr);
                        case 0x16: return new AlphaOp("extwl", Format.Opr);
                        case 0x1b: return new AlphaOp("inswl", Format.Opr);
                        case 0x22: return new AlphaOp("mskll", Format.Opr);
                        case 0x26: return new AlphaOp("extll", Format.Opr);
                        case 0x2b: return new AlphaOp("insll", Format.Opr);
                        case 0x30: return new AlphaOp("zap", Format.Opr);
                        case 0x31: return new AlphaOp("zapnot", Format.Opr);
                        case 0x32: return new AlphaOp("mskql", Format.Opr);
                        case 0x34: return new AlphaOp("srl", Format.Opr);
                        case 0x36: return new AlphaOp("extql", Format.Opr);
                        case 0x39: return new AlphaOp("sll", Format.Opr);
                        case 0x3b: return new AlphaOp("insql", Format.Opr);
                        case 0x3c: return new AlphaOp("sra", Format.Opr);
                        case 0x52: return new AlphaOp("mskwh", Format.Opr);
                        case 0x57: return new AlphaOp("inswh", Format.Opr);
                        case 0x5a: return new AlphaOp("extwh", Format.Opr);
                        case 0x62: return new AlphaOp("msklh", Format.Opr);
                        case 0x67: return new AlphaOp("inslh", Format.Opr);
                        case 0x6a: return new AlphaOp("extlh", Format.Opr);
                        case 0x72: return new AlphaOp("mskqh", Format.Opr);
                        case 0x77: return new AlphaOp("insqh", Format.Opr);
                        case 0x7a: return new AlphaOp("extqh", Format.Opr);
                    }
                    break;
                case 0x13:
                    switch ((code >> 5) & 0x7f)
                    {
                        case 0x00: return new AlphaOp("mull", Format.Opr);
                        case 0x20: return new AlphaOp("mulq", Format.Opr);
                        case 0x30: return new AlphaOp("umulh", Format.Opr);
                        case 0x40: return new AlphaOp("mull/v", Format.Opr);
                        case 0x60: return new AlphaOp("mulq/v", Format.Opr);
                    }
                    break;
                case 0x14:
                    {
                        switch (fpf)
                        {
                            case 0x004: return new AlphaOp("itofs", Format.FP);
                            case 0x014: return new AlphaOp("itoff", Format.FP);
                            case 0x024: return new AlphaOp("itoft", Format.FP);
                        }
                        switch (fpf & 0x3f)
                        {
                            case 0x00a: mne = "sqrtf"; fpt = 3; break;
                            case 0x00b: mne = "sqrts"; break;
                            case 0x02a: mne = "sqrtg"; fpt = 3; break;
                            case 0x02b: mne = "sqrtt"; break;
                        }
                        break;
                    }
                case 0x15:
                    {
                        switch (fpf)
                        {
                            case 0x03c: return new AlphaOp("cvtqf/c");
                            case 0x03e: return new AlphaOp("cvtqg/c");
                            case 0x0a5: return new AlphaOp("cmpgeq");
                            case 0x0a6: return new AlphaOp("cmpglt");
                            case 0x0a7: return new AlphaOp("cmpgle");
                            case 0x0bc: return new AlphaOp("cvtqf");
                            case 0x0be: return new AlphaOp("cvtqg");
                            case 0x4a5: return new AlphaOp("cmpgeq/s");
                            case 0x4a6: return new AlphaOp("cmpglt/s");
                            case 0x4a7: return new AlphaOp("cmpgle/s");
                        }
                        switch (fpf & 0x3f)
                        {
                            case 0x000: mne = "addf"; fpt = 3; break;
                            case 0x001: mne = "subf"; fpt = 3; break;
                            case 0x002: mne = "mulf"; fpt = 3; break;
                            case 0x003: mne = "divf"; fpt = 3; break;
                            case 0x01e: mne = "cvtdg"; fpt = 3; break;
                            case 0x020: mne = "addg"; fpt = 3; break;
                            case 0x021: mne = "subg"; fpt = 3; break;
                            case 0x022: mne = "mulg"; fpt = 3; break;
                            case 0x023: mne = "divg"; fpt = 3; break;
                            case 0x02c: mne = "cvtgf"; fpt = 3; break;
                            case 0x02d: mne = "cvtgd"; fpt = 3; break;
                            case 0x02f: mne = "cvtgq"; fpt = 4; break;
                        }
                        break;
                    }
                case 0x16:
                    {
                        switch (fpf)
                        {
                            case 0x0a4: return new AlphaOp("cmptun", Format.FP);
                            case 0x0a5: return new AlphaOp("cmpteq", Format.FP);
                            case 0x0a6: return new AlphaOp("cmptlt", Format.FP);
                            case 0x0a7: return new AlphaOp("cmptle", Format.FP);
                            case 0x2ac: return new AlphaOp("cvtst", Format.FP);
                            case 0x5a4: return new AlphaOp("cmptun/su", Format.FP);
                            case 0x5a5: return new AlphaOp("cmpteq/su", Format.FP);
                            case 0x5a6: return new AlphaOp("cmptlt/su", Format.FP);
                            case 0x5a7: return new AlphaOp("cmptle/su", Format.FP);
                            case 0x6ac: return new AlphaOp("cvtst/s", Format.FP);
                        }
                        switch (fpf & 0x3f)
                        {
                            case 0x000: mne = "adds"; break;
                            case 0x001: mne = "subs"; break;
                            case 0x002: mne = "muls"; break;
                            case 0x003: mne = "divs"; break;
                            case 0x020: mne = "addt"; break;
                            case 0x021: mne = "subt"; break;
                            case 0x022: mne = "mult"; break;
                            case 0x023: mne = "divt"; break;
                            case 0x02c: mne = "cvtts"; break;
                            case 0x02f: mne = "cvttq"; fpt = 2; break;
                            case 0x03c: mne = "cvtqs"; fpt = 1; break;
                            case 0x03e: mne = "cvtqt"; fpt = 1; break;
                        }
                        break;
                    }
                case 0x17:
                    switch ((code >> 5) & 0x7ff)
                    {
                        case 0x010: return new AlphaOp("cvtlq", Format.FP);
                        case 0x020: return new AlphaOp("cpys", Format.FP);
                        case 0x021: return new AlphaOp("cpysn", Format.FP);
                        case 0x022: return new AlphaOp("cpyse", Format.FP);
                        case 0x024: return new AlphaOp("mt_fpcr", Format.FP);
                        case 0x025: return new AlphaOp("mf_fpcr", Format.FP);
                        case 0x02a: return new AlphaOp("fcmoveq", Format.FP);
                        case 0x02b: return new AlphaOp("fcmovne", Format.FP);
                        case 0x02c: return new AlphaOp("fcmovlt", Format.FP);
                        case 0x02d: return new AlphaOp("fcmovge", Format.FP);
                        case 0x02e: return new AlphaOp("fcmovle", Format.FP);
                        case 0x02f: return new AlphaOp("fcmovgt", Format.FP);
                        case 0x030: return new AlphaOp("cvtql", Format.FP);
                        case 0x130: return new AlphaOp("cvtql/v", Format.FP);
                        case 0x530: return new AlphaOp("cvtql/sv", Format.FP);
                    }
                    break;
                case 0x18:
                    switch (code & 0xffff)
                    {
                        case 0x0000: return new AlphaOp("trapb", Format.Mfc);
                        case 0x0400: return new AlphaOp("excb", Format.Mfc);
                        case 0x4000: return new AlphaOp("mb", Format.Mfc);
                        case 0x4400: return new AlphaOp("wmb", Format.Mfc);
                        case 0x8000: return new AlphaOp("fetch", Format.Mfc);
                        case 0xa000: return new AlphaOp("fetch_m", Format.Mfc);
                        case 0xc000: return new AlphaOp("rpcc", Format.Mfc);
                        case 0xe000: return new AlphaOp("rc", Format.Mfc);
                        case 0xf000: return new AlphaOp("rs", Format.Mfc);
                        case 0xe800: return new AlphaOp("ecb", Format.Mfc);
                        case 0xf800: return new AlphaOp("wh64", Format.Mfc);
                        case 0xfc00: return new AlphaOp("wh64en", Format.Mfc);
                    }
                    break;
                case 0x19: return new AlphaOp("pal19");
                case 0x1a:
                    switch ((code >> 14) & 3)
                    {
                        case 0x0: return new AlphaOp("jmp", Format.Mbr);
                        case 0x1: return new AlphaOp("jsr", Format.Mbr);
                        case 0x2: return new AlphaOp("ret", Format.Mbr);
                        case 0x3: return new AlphaOp("jsr_coroutine", Format.Mbr);
                    }
                    break;
                case 0x1b: return new AlphaOp("pal1b");
                case 0x1c:
                    switch ((code >> 5) & 0x7f)
                    {
                        case 0x00: return new AlphaOp("sextb", Format.Opr);
                        case 0x01: return new AlphaOp("sextw", Format.Opr);
                        case 0x30: return new AlphaOp("ctpop", Format.Opr);
                        case 0x31: return new AlphaOp("perr", Format.Opr);
                        case 0x32: return new AlphaOp("ctlz", Format.Opr);
                        case 0x33: return new AlphaOp("cttz", Format.Opr);
                        case 0x34: return new AlphaOp("unpkbw", Format.Opr);
                        case 0x35: return new AlphaOp("unpkbl", Format.Opr);
                        case 0x36: return new AlphaOp("pkwb", Format.Opr);
                        case 0x37: return new AlphaOp("pklb", Format.Opr);
                        case 0x38: return new AlphaOp("minsb8", Format.Opr);
                        case 0x39: return new AlphaOp("minsw4", Format.Opr);
                        case 0x3a: return new AlphaOp("minub8", Format.Opr);
                        case 0x3b: return new AlphaOp("minuw4", Format.Opr);
                        case 0x3c: return new AlphaOp("maxub8", Format.Opr);
                        case 0x3d: return new AlphaOp("maxuw4", Format.Opr);
                        case 0x3e: return new AlphaOp("maxsb8", Format.Opr);
                        case 0x3f: return new AlphaOp("maxsw4", Format.Opr);
                        case 0x70: return new AlphaOp("ftoit", Format.Opr);
                        case 0x78: return new AlphaOp("ftois", Format.Opr);
                    }
                    break;
                case 0x1d: return new AlphaOp("pal1d");
                case 0x1e: return new AlphaOp("pal1e");
                case 0x1f: return new AlphaOp("pal1f");
                case 0x20: return new AlphaOp("ldf", Format.Mem);
                case 0x21: return new AlphaOp("ldg", Format.Mem);
                case 0x22: return new AlphaOp("lds", Format.Mem);
                case 0x23: return new AlphaOp("ldt", Format.Mem);
                case 0x24: return new AlphaOp("stf", Format.Mem);
                case 0x25: return new AlphaOp("stg", Format.Mem);
                case 0x26: return new AlphaOp("sts", Format.Mem);
                case 0x27: return new AlphaOp("stt", Format.Mem);
                case 0x28: return new AlphaOp("ldl", Format.Mem);
                case 0x29: return new AlphaOp("ldq", Format.Mem);
                case 0x2a: return new AlphaOp("ldl_l", Format.Mem);
                case 0x2b: return new AlphaOp("ldq_l", Format.Mem);
                case 0x2c: return new AlphaOp("stl", Format.Mem);
                case 0x2d: return new AlphaOp("stq", Format.Mem);
                case 0x2e: return new AlphaOp("stl_c", Format.Mem);
                case 0x2f: return new AlphaOp("stq_c", Format.Mem);
                case 0x30: return new AlphaOp("br", Format.Bra);
                case 0x31: return new AlphaOp("fbeq", Format.Bra);
                case 0x32: return new AlphaOp("fblt", Format.Bra);
                case 0x33: return new AlphaOp("fble", Format.Bra);
                case 0x34: return new AlphaOp("bsr", Format.Mbr);
                case 0x35: return new AlphaOp("fbne", Format.Bra);
                case 0x36: return new AlphaOp("fbge", Format.Bra);
                case 0x37: return new AlphaOp("fbgt", Format.Bra);
                case 0x38: return new AlphaOp("blbc", Format.Bra);
                case 0x39: return new AlphaOp("beq", Format.Bra);
                case 0x3a: return new AlphaOp("blt", Format.Bra);
                case 0x3b: return new AlphaOp("ble", Format.Bra);
                case 0x3c: return new AlphaOp("blbs", Format.Bra);
                case 0x3d: return new AlphaOp("bne", Format.Bra);
                case 0x3e: return new AlphaOp("bge", Format.Bra);
                case 0x3f: return new AlphaOp("bgt", Format.Bra);
            }
            if (mne != null)
            {
                var qua = GetQualifier(fpf, fpt);
                if (qua != null)
                    return new AlphaOp(mne + GetQualifier(fpf, fpt), Format.FP);
            }
            return new AlphaOp("???");
        }

        private static string GetQualifier(int f, int t)
        {
            string qua = "", uv = t == 2 || t == 4 ? "v" : "u";
            switch (f & 0x700)
            {
                case 0x100:
                    if (t == 1) return null;
                    qua = uv;
                    break;
                case 0x400:
                    if (t != 3 && t != 4) return null;
                    qua = "s";
                    break;
                case 0x500:
                    if (t == 1) return null;
                    qua = "s" + uv;
                    break;
                case 0x700:
                    if (t == 3) return null;
                    qua = "s" + uv + "i";
                    break;
            }
            switch (f & 0xc0)
            {
                case 0x00:
                    qua += "c";
                    break;
                case 0x40:
                    if (t == 3 || t == 4) return null;
                    qua += "m";
                    break;
                case 0xc0:
                    if (t == 3 || t == 4) return null;
                    qua += "d";
                    break;
            }
            if (qua != "") return "/" + qua;
            return "";
        }

        public static void Disassemble(StringBuilder sb, ulong addr, uint code)
        {
            var aop = Disassemble(code);
            int op = (int)(code >> 26);
            var mne = aop.Mnemonic;
            var type = aop.Type;
            sb.AppendFormat("{0:x8} => {1:x2}", code, op);
            switch (type)
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
                            ra, disp, mne, regnames[ra], sdisp);
                        if (ra == 31 && mne == "br")
                            sb.AppendFormat(" => br {0}", sdisp);
                        break;
                    }
                case Format.Mem:
                case Format.Mbr:
                    {
                        int ra = (int)((code >> 21) & 31);
                        int rb = (int)((code >> 16) & 31);
                        int disp;
                        string args;
                        if (type == Format.Mem)
                        {
                            disp = (int)(code & 0xffff);
                            sb.Append("     ");
                            args = disp < 0x8000
                                ? string.Format("{0:x}({1})", disp, regnames[rb])
                                : string.Format("-{0:x}({1})", 0x10000 - disp, regnames[rb]);
                        }
                        else
                        {
                            disp = (int)((code & 0x3fff) << 2);
                            sb.AppendFormat(".{0:x}   ", (code >> 14) & 3);
                            args = disp < 0x2000
                                ? string.Format("{0:x}({1})", disp, regnames[rb])
                                : string.Format("-{0:x}({1})", 0x4000 - disp, regnames[rb]);
                        }
                        sb.AppendFormat(" r{0:00} r{1:00} {2:x4}", ra, rb, disp);
                        sb.AppendFormat(" => {0,-7} {1},", mne, regnames[ra]);
                        sb.Append(args);
                        if (rb == 31 && mne == "lda")
                            sb.AppendFormat(" => mov {0:x},{1}", disp, regnames[ra]);
                        else if (rb == 31 && mne == "ldah")
                            sb.AppendFormat(" => mov {0:x}0000,{1}", disp, regnames[ra]);
                        else if (ra == 31)
                        {
                            if (disp == 0 && mne == "ldq_u")
                                sb.Append(" => unop");
                            else
                            {
                                var pse = "";
                                switch (mne)
                                {
                                    case "ldl": pse = "prefetch"; break;
                                    case "ldq": pse = "prefetch_en"; break;
                                    case "lds": pse = "prefetch_m"; break;
                                    case "ldt": pse = "prefetch_men"; break;
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
                            code & 0xffff, ra, rb, mne, regnames[ra], regnames[rb]);
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
                            arg2 = regnames[rb];
                        }
                        else
                        {
                            arg2 = string.Format("{0:x2}", (code >> 13) & 0xff);
                            sb.AppendFormat(" r{0:00} {1} r{2:00}  ", ra, arg2, rc);
                        }
                        sb.AppendFormat(" => {0,-7} {1},{2},{3}",
                            mne, regnames[ra], arg2, regnames[rc]);
                        if (ra == 31)
                        {
                            string pse = "";
                            switch (mne)
                            {
                                case "bis":
                                    if (rb == 31 && rc == 31)
                                        sb.Append(" => nop");
                                    else if (rb == 31)
                                        sb.AppendFormat(" => clr {0}", regnames[rc]);
                                    else
                                        pse = "mov";
                                    break;
                                case "addl": pse = "sextl"; break;
                                case "ornot": pse = "not"; break;
                                case "subl": pse = "negl"; break;
                                case "subl/v": pse = "negl/v"; break;
                                case "subq": pse = "negq"; break;
                                case "subq/v": pse = "negq/v"; break;
                            }
                            if (pse != "")
                                sb.AppendFormat(" => {0} {1},{2}", pse, arg2, regnames[rc]);
                        }
                        break;
                    }
                case Format.FP:
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
                            switch (mne)
                            {
                                case "cpys":
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
                                case "subf": pse = "negf"; break;
                                case "subf/s": pse = "negf/s"; break;
                                case "subg": pse = "negg"; break;
                                case "subg/s": pse = "negg/s"; break;
                                case "subs": pse = "negs"; break;
                                case "subs/su": pse = "negs/su"; break;
                                case "subs/sui": pse = "negs/sui"; break;
                                case "subt": pse = "negt"; break;
                                case "subt/su": pse = "negt/su"; break;
                                case "subt/sui": pse = "negt/sui"; break;
                            };
                        if (pse == "" && fa == fb)
                            switch (mne)
                            {
                                case "cpys": pse = "fmov"; break;
                                case "cpysn": pse = "fneg"; break;
                            }
                        if (pse == "" && fa == fb && fb == fc)
                            switch (mne)
                            {
                                case "mf_fpcr":
                                case "mt_fpcr":
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
        }
    }
}
