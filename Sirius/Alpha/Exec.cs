using System;
using System.IO;
using System.Text;

namespace Sirius
{
    public partial class Alpha
    {
        private ulong pc;
        private ulong[] reg = new ulong[32];
        private double[] frg = new double[32];
        private StringBuilder output;

        private Exception Abort(string format, params object[] args)
        {
            return new Exception(
                string.Format("pc={0:x16}: ", pc - 4) +
                string.Format(format, args));
        }

        public void Exec(StringBuilder sb)
        {
            output = sb;

            Array.Clear(stack, 0, stack.Length);
            Array.Clear(reg, 0, reg.Length);
            Array.Clear(frg, 0, frg.Length);
            reg[(int)Regs.RA] = reg[(int)Regs.SP] = stackEnd;

            var text = elf.Text;
            ulong start = text.sh_addr, end = start + text.sh_size;
            pc = reg[(int)Regs.T12] = elf.e_entry; // t12 for gp

            output.AppendFormat("pc={0:x16}: 開始", pc);
            output.AppendLine();
            for (; ; )
            {
                if (pc == stackEnd) break;
                if (pc < start || pc >= end)
                    throw Abort("不正な実行アドレス");
                ExecStep((int)((pc - start) >> 2));
            }

            output.AppendLine();
            output.AppendLine("---");
            output.AppendLine("完了しました。");
        }

        private void ExecStep(int p)
        {
            pc += 4;
            var code = text_code[p];
            var op = text_op[p];
            switch (formats[((int)op) >> 16])
            {
                case Format.Bra:
                    {
                        int ra = (int)((code >> 21) & 31);
                        uint disp = code & 0x001fffff;
                        ulong addr = disp < 0x00100000
                            ? pc + disp * 4
                            : pc - (0x00200000 - disp) * 4;
                        switch (op)
                        {
                            case Op.Br:
                            case Op.Bsr:
                                if (ra != 31) reg[ra] = pc;
                                pc = addr;
                                return;
                            case Op.Blbc: if (((ushort)reg[ra]) == 0) pc = addr; return;
                            case Op.Beq: if (reg[ra] == 0) pc = addr; return;
                            case Op.Blt: if (((long)reg[ra]) < 0) pc = addr; return;
                            case Op.Ble: if (((long)reg[ra]) <= 0) pc = addr; return;
                            case Op.Blbs: if (((ushort)reg[ra]) != 0) pc = addr; return;
                            case Op.Bne: if (reg[ra] != 0) pc = addr; return;
                            case Op.Bge: if (((long)reg[ra]) >= 0) pc = addr; return;
                            case Op.Bgt: if (((long)reg[ra]) > 0) pc = addr; return;
                        }
                        break;
                    }
                case Format.Mem:
                    {
                        int ra = (int)((code >> 21) & 31);
                        ulong vb = reg[(code >> 16) & 31];
                        ulong disp = code & 0xffff;
                        if (disp >= 0x8000) disp -= 0x10000;
                        switch (op)
                        {
                            case Op.Stt: WriteDouble(vb + disp, frg[ra]); return;
                            case Op.Stq: Write64(vb + disp, reg[ra]); return;
                            case Op.Stq_u: Write64((vb + disp) & ~7UL, reg[ra]); return;
                            case Op.Stl: Write32(vb + disp, (uint)reg[ra]); return;
                            case Op.Stw: Write16(vb + disp, (ushort)reg[ra]); return;
                            case Op.Stb: Write8(vb + disp, (byte)reg[ra]); return;
                        }
                        if (ra == 31) return;
                        switch (op)
                        {
                            case Op.Lda: reg[ra] = vb + disp; return;
                            case Op.Ldah: reg[ra] = vb + (disp << 16); return;
                            case Op.Ldq: reg[ra] = Read64(vb + disp); return;
                            case Op.Ldq_u: reg[ra] = Read64((vb + disp) & ~7UL); return;
                            case Op.Ldl: reg[ra] = (ulong)(int)Read32(vb + disp); return;
                            case Op.Ldwu: reg[ra] = (ulong)(short)Read16(vb + disp); return;
                            case Op.Ldbu: reg[ra] = (ulong)(sbyte)Read8(vb + disp); return;
                        }
                        break;
                    }
                case Format.Mbr:
                    {
                        int ra = (int)((code >> 21) & 31);
                        var vb = reg[(int)((code >> 16) & 31)];
                        switch (op)
                        {
                            case Op.Jmp:
                            case Op.Jsr:
                            case Op.Ret:
                            case Op.Jsr_coroutine:
                                if (ra != 31) reg[ra] = pc;
                                pc = vb;
                                return;
                        }
                        break;
                    }
                case Format.Opr:
                    {
                        ulong va = reg[(code >> 21) & 31];
                        ulong vb = ((code & 0x1000) == 0)
                            ? reg[(code >> 16) & 31]
                            : (code >> 13) & 0xff;
                        int rc = (int)(code & 31);
                        if (rc == 31) return;
                        int m = ((int)vb & 7) << 3;
                        uint val = (uint)va, vbl = (uint)vb;
                        switch (op)
                        {
                            case Op.Bis: reg[rc] = va | vb; return;
                            case Op.Bic: reg[rc] = va & ~vb; return;
                            case Op.And: reg[rc] = va & vb; return;
                            case Op.Xor: reg[rc] = va ^ vb; return;
                            case Op.Ornot: reg[rc] = va | ~vb; return;
                            case Op.Eqv: reg[rc] = va ^ ~vb; return;
                            case Op.Zap: reg[rc] = va & mask[vb & 255]; return;
                            case Op.Zapnot: reg[rc] = va & ~mask[vb & 255]; return;
                            case Op.Addq: reg[rc] = va + vb; return;
                            case Op.Subq: reg[rc] = va - vb; return;
                            case Op.Mulq: reg[rc] = (ulong)((long)va * (long)vb); return;
                            case Op.Umulh:
                                if (va == 0 || vb == 0)
                                    reg[rc] = 0;
                                else
                                {
                                    ulong xh = va >> 32, xl = va & 0xffffffff;
                                    ulong yh = vb >> 32, yl = vb & 0xffffffff;
                                    ulong a = xh * yl, ah = a >> 32, al = a & 0xffffffff;
                                    ulong b = xl * yh, bh = b >> 32, bl = b & 0xffffffff;
                                    reg[rc] = ((((xl * yl) >> 32) + al + bl) >> 32) + ah + bh + xh * yh;
                                }
                                return;
                            case Op.S4addq: reg[rc] = (va << 2) + vb; return;
                            case Op.S8addq: reg[rc] = (va << 3) + vb; return;
                            case Op.S4subq: reg[rc] = (va << 2) - vb; return;
                            case Op.S8subq: reg[rc] = (va << 3) - vb; return;
                            case Op.Sextb: reg[rc] = (ulong)(sbyte)(byte)vb; return;
                            case Op.Sextw: reg[rc] = (ulong)(short)(ushort)vb; return;
                            case Op.Sll: reg[rc] = va << (int)vb; return;
                            case Op.Srl: reg[rc] = va >> (int)vb; return;
                            case Op.Sra: reg[rc] = (ulong)(((long)va) >> (int)vb); return;
                            case Op.Cmpeq: reg[rc] = va == vb ? 1LU : 0LU; return;
                            case Op.Cmple: reg[rc] = (long)va <= (long)vb ? 1LU : 0LU; return;
                            case Op.Cmplt: reg[rc] = (long)va < (long)vb ? 1LU : 0LU; return;
                            case Op.Cmpule: reg[rc] = va <= vb ? 1LU : 0LU; return;
                            case Op.Cmpult: reg[rc] = va < vb ? 1LU : 0LU; return;
                            case Op.Cmoveq: if (va == 0) reg[rc] = vb; return;
                            case Op.Cmovge: if ((long)va >= 0) reg[rc] = vb; return;
                            case Op.Cmovgt: if ((long)va > 0) reg[rc] = vb; return;
                            case Op.Cmovlbc: if ((ushort)va == 0) reg[rc] = vb; return;
                            case Op.Cmovlbs: if ((ushort)va != 0) reg[rc] = vb; return;
                            case Op.Cmovle: if ((long)va <= 0) reg[rc] = vb; return;
                            case Op.Cmovlt: if ((long)va < 0) reg[rc] = vb; return;
                            case Op.Cmovne: if (va != 0) reg[rc] = vb; return;

                            case Op.Mskbl: reg[rc] = va & ~(0xffUL << m); return;
                            case Op.Mskwl: reg[rc] = va & ~(0xffffUL << m); return;
                            case Op.Mskll: reg[rc] = va & ~(0xffffffffUL << m); return;
                            case Op.Mskql: reg[rc] = va & ~(ulong.MaxValue << m); return;
                            case Op.Mskwh: reg[rc] = va & ~(0xffffUL >> (64 - m)); return;
                            case Op.Msklh: reg[rc] = va & ~(0xffffffffUL >> (64 - m)); return;
                            case Op.Mskqh: reg[rc] = va & ~(ulong.MaxValue >> (64 - m)); return;
                            case Op.Insbl: reg[rc] = (va & 0xff) << m; return;
                            case Op.Inswl: reg[rc] = (va & 0xffff) << m; return;
                            case Op.Insll: reg[rc] = (va & 0xffffffff) << m; return;
                            case Op.Insql: reg[rc] = (va << m); return;
                            case Op.Inswh: reg[rc] = (va & 0xffff) >> (64 - m); return;
                            case Op.Inslh: reg[rc] = (va & 0xffffffff) >> (64 - m); return;
                            case Op.Insqh: reg[rc] = (va >> (64 - m)); return;
                            case Op.Extbl: reg[rc] = (va >> m) & 0xff; return;
                            case Op.Extwl: reg[rc] = (va >> m) & 0xffff; return;
                            case Op.Extll: reg[rc] = (va >> m) & 0xffffffff; return;
                            case Op.Extql: reg[rc] = (va >> m); return;
                            case Op.Extwh: reg[rc] = (va << (64 - m)) & 0xffff; return;
                            case Op.Extlh: reg[rc] = (va << (64 - m)) & 0xffffffff; return;
                            case Op.Extqh: reg[rc] = (va << (64 - m)); return;

                            case Op.Addl: reg[rc] = (ulong)(int)(val + vbl); return;
                            case Op.Subl: reg[rc] = (ulong)(int)(val - vbl); return;
                            case Op.Mull: reg[rc] = (ulong)((int)val * (int)vbl); return;
                            case Op.S4addl: reg[rc] = (ulong)(int)((val << 2) + vbl); return;
                            case Op.S8addl: reg[rc] = (ulong)(int)((val << 3) + vbl); return;
                            case Op.S4subl: reg[rc] = (ulong)(int)((val << 2) - vbl); return;
                            case Op.S8subl: reg[rc] = (ulong)(int)((val << 3) - vbl); return;
                        }
                        break;
                    }
            }
            throw Abort("未実装: {0}", GetMnemonic(op));
        }
    }
}
