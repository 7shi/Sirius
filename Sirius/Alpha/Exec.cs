using System;
using System.IO;
using System.Text;

namespace Sirius
{
    public partial class Alpha
    {
        private ulong[] reg = new ulong[32];
        private ulong pc;
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
                            case Op.Blbc:
                                if (((ushort)reg[ra]) == 0) pc = addr;
                                return;
                            case Op.Beq:
                                if (reg[ra] == 0) pc = addr;
                                return;
                            case Op.Blt:
                                if (((long)reg[ra]) < 0) pc = addr;
                                return;
                            case Op.Ble:
                                if (((long)reg[ra]) <= 0) pc = addr;
                                return;
                            case Op.Blbs:
                                if (((ushort)reg[ra]) != 0) pc = addr;
                                return;
                            case Op.Bne:
                                if (reg[ra] != 0) pc = addr;
                                return;
                            case Op.Bge:
                                if (((long)reg[ra]) >= 0) pc = addr;
                                return;
                            case Op.Bgt:
                                if (((long)reg[ra]) > 0) pc = addr;
                                return;
                        }
                        break;
                    }
                case Format.Mem:
                    {
                        int ra = (int)((code >> 21) & 31);
                        int rb = (int)((code >> 16) & 31);
                        ulong disp = code & 0xffff;
                        if (disp >= 0x8000) disp = disp - 0x10000;
                        switch (op)
                        {
                            case Op.Lda:
                                if (ra != 31)
                                {
                                    if (rb == 31)
                                        reg[ra] = disp; // mov
                                    else
                                        reg[ra] = reg[rb] + disp;
                                    //output.AppendFormat("[{0}:{1:x8}]", regname[ra], reg[ra]);
                                }
                                return;
                            case Op.Ldah:
                                if (ra != 31)
                                {
                                    if (rb == 31)
                                        reg[ra] = disp << 16; // movh
                                    else
                                        reg[ra] = reg[rb] + (disp << 16);
                                    //output.AppendFormat("[{0}:{1:x8}]", regname[ra], reg[ra]);
                                }
                                return;
                            case Op.Ldq:
                                if (ra != 31) reg[ra] = Read64(reg[rb] + disp);
                                //output.AppendFormat("[{0}:{1:x8}]", regname[ra], reg[ra]);
                                return;
                            case Op.Ldq_u:
                                if (ra == 31 && disp == 0)
                                {
                                    // unop
                                }
                                else if (ra != 31)
                                    reg[ra] = Read64(reg[rb] + disp);
                                //output.AppendFormat("[{0}:{1:x8}]", regname[ra], reg[ra]);
                                return;
                            case Op.Ldl:
                                if (ra != 31) reg[ra] = Read32(reg[rb] + disp);
                                return;
                            case Op.Ldwu:
                                if (ra != 31) reg[ra] = Read16(reg[rb] + disp);
                                return;
                            case Op.Ldbu:
                                if (ra != 31) reg[ra] = Read8(reg[rb] + disp);
                                return;
                            case Op.Stq:
                                Write64(reg[rb] + disp, reg[ra]);
                                return;
                            case Op.Stl:
                                Write32(reg[rb] + disp, (uint)reg[ra]);
                                return;
                            case Op.Stw:
                                Write16(reg[rb] + disp, (ushort)reg[ra]);
                                return;
                            case Op.Stb:
                                Write8(reg[rb] + disp, (byte)reg[ra]);
                                return;
                        }
                        break;
                    }
                case Format.Mbr:
                    {
                        int ra = (int)((code >> 21) & 31);
                        int rb = (int)((code >> 16) & 31);
                        int disp = (int)(code & 0x3fff);
                        switch (op)
                        {
                            case Op.Jmp:
                            case Op.Jsr:
                            case Op.Ret:
                            case Op.Jsr_coroutine:
                                {
                                    ulong va = reg[rb];
                                    if (ra != 31) reg[ra] = pc;
                                    pc = va;
                                    return;
                                }
                        }
                        break;
                    }
                case Format.Opr:
                    {
                        int ra = (int)((code >> 21) & 31);
                        int rb = -1;
                        int rc = (int)(code & 31);
                        byte lit = 0;
                        if ((code & 0x1000) == 0)
                            rb = (int)((code >> 16) & 31);
                        else
                            lit = (byte)((code >> 13) & 0xff);
                        switch (op)
                        {
                            case Op.Bis:
                                if (ra == 31)
                                {
                                    if (rb == 31 && rc == 31)
                                    {
                                        // nop
                                    }
                                    else if (rb == 31)
                                        reg[rc] = 0; // clr
                                    else if (rb == -1)
                                        reg[rc] = lit; // mov
                                    else
                                        reg[rc] = reg[rb]; // mov
                                }
                                else if (rb == -1)
                                    reg[rc] = lit | reg[rb];
                                else
                                    reg[rc] = reg[ra] | reg[rb];
                                return;
                            case Op.Sextb:
                                reg[rc] = (ulong)(long)(sbyte)(byte)reg[rb];
                                return;
                            case Op.Sextw:
                                reg[rc] = (ulong)(long)(short)(ushort)reg[rb];
                                return;
                        }
                        break;
                    }
            }
            throw Abort("未実装: {0}", GetMnemonic(op));
        }
    }
}
