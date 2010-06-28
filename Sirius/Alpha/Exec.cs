using System;
using System.IO;
using System.Text;

namespace Sirius
{
    public partial class Alpha
    {
        private const ulong execEnd = stackEnd + 4;

        private ulong[] reg = new ulong[32];
        private ulong pc;
        private StringBuilder output;

        private Exception Abort(string format, params object[] args)
        {
            return new Exception(
                string.Format("pc={0:x16}: ", pc) +
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
            pc = elf.e_entry;

            output.AppendFormat("pc={0:x16}: 開始", pc);
            output.AppendLine();
            for (; ; )
            {
                if (pc == execEnd) break;
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
            var code = text_code[p];
            var op = text_op[p];
            var type = formats[((int)op) >> 16];
            switch (type)
            {
                case Format.Mem:
                case Format.Mbr:
                    {
                        var ra = (int)((code >> 21) & 31);
                        var rb = (int)((code >> 16) & 31);
                        ulong disp;
                        if (type == Format.Mem)
                        {
                            disp = code & 0xffff;
                            if (disp >= 0x8000) disp = disp - 0x10000;
                        }
                        else
                        {
                            disp = (code & 0x3fff) << 2;
                            if (disp >= 0x2000) disp = disp - 0x4000;
                        }
                        switch (op)
                        {
                            case Op.Lda:
                                if (ra != 31)
                                {
                                    if (rb == 31)
                                        reg[ra] = disp; // mov
                                    else
                                        reg[ra] = reg[rb] + disp;
                                }
                                pc += 4;
                                return;
                            case Op.Ldah:
                                if (ra != 31)
                                {
                                    if (rb == 31)
                                        reg[ra] = disp << 16; // movh
                                    else
                                        reg[ra] = reg[rb] + (disp << 16);
                                }
                                pc += 4;
                                return;
                            case Op.Ldq:
                                if (ra != 31) reg[ra] = Read64(reg[rb] + disp);
                                pc += 4;
                                return;
                            case Op.Ldq_u:
                                if (ra == 31 && disp == 0)
                                {
                                    // unop
                                }
                                else if (ra != 31)
                                    reg[ra] = Read64(reg[rb] + disp);
                                pc += 4;
                                return;
                            case Op.Ldl:
                                if (ra != 31) reg[ra] = Read32(reg[rb] + disp);
                                pc += 4;
                                return;
                            case Op.Ldwu:
                                if (ra != 31) reg[ra] = Read16(reg[rb] + disp);
                                pc += 4;
                                return;
                            case Op.Ldbu:
                                if (ra != 31) reg[ra] = Read8(reg[rb] + disp);
                                pc += 4;
                                return;
                            case Op.Stq:
                                Write64(reg[rb] + disp, reg[ra]);
                                pc += 4;
                                return;
                            case Op.Stl:
                                Write32(reg[rb] + disp, (uint)reg[ra]);
                                pc += 4;
                                return;
                            case Op.Stw:
                                Write16(reg[rb] + disp, (ushort)reg[ra]);
                                pc += 4;
                                return;
                            case Op.Stb:
                                Write8(reg[rb] + disp, (byte)reg[ra]);
                                pc += 4;
                                return;
                            case Op.Jmp:
                            case Op.Jsr:
                            case Op.Ret:
                            case Op.Jsr_coroutine:
                                {
                                    ulong va = reg[rb] + disp;
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
                                pc += 4;
                                return;
                        }
                        break;
                    }
            }
            throw Abort("未実装: {0}", GetMnemonic(op));
        }
    }
}
