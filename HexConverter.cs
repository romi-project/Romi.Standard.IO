using Romi.Standard.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Romi.Standard.IO
{
    public static class HexConverter
    {
        private enum SmartOpCodes
        {
            Null, Byte, Word, Dword, Qword, Double, LString, String, Buffer
        }

        public static string ToString(byte[] buffer)
        {
            return BitConverter.ToString(buffer).Replace('-', ' ');
        }

        public static string ToStringNoSpace(byte[] buffer)
        {
            return BitConverter.ToString(buffer).Replace("-", "");
        }

        public static byte[] Parse(string s)
        {
            int length = s.Length;
            var stream = new MemoryStream();
            int index = 0;
            int value = -1;
            while (index < length)
            {
                int digit = -1;
                while (digit == -1 && index < length)
                {
                    char c = s[index];
                    if (c >= '0' && c <= '9')
                    {
                        digit = c - '0';
                    }
                    else if (c >= 'a' && c <= 'f')
                    {
                        digit = c - 'a' + 10;
                    }
                    else if (c >= 'A' && c <= 'F')
                    {
                        digit = c - 'A' + 10;
                    }
                    else
                    {
                        digit = -1;
                    }
                    index++;
                }
                if (digit != -1)
                {
                    if (value == -1)
                    {
                        value = digit << 4;
                    }
                    else
                    {
                        value |= digit & 0xF;
                        stream.WriteByte((byte)value);
                        value = -1;
                    }
                }
            }
            if (value != -1)
            {
                throw new ArgumentException(nameof(s));
            }
            return stream.ToArray();
        }

        public static bool TryParse(string s, out byte[] value)
        {
            try
            {
                value = Parse(s);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        public static byte[] ParseSmart(string s)
        {
            const string commentPattern = @"\/\*.*?\*\/";
            const string smartPattern = @"(.)[{](.*?)[}]";
            const string rawPattern = @"[0-9a-fA-F]{2}";
            const string pattern = @"((" + commentPattern + @"\s*)*((" + smartPattern + @")|(" + rawPattern + @"))\s*)+";
            Match match = Regex.Match(s, pattern);
            if (match.Success)
            {
                var oBuffer = new OutBuffer();
                for (int ctr = 0; ctr < match.Groups[3].Captures.Count; ctr++)
                {
                    string input = match.Groups[3].Captures[ctr].Value;

                    if (Regex.IsMatch(input, smartPattern))
                    {
                        Match smartMatch = Regex.Match(input, smartPattern);
                        string opcode = smartMatch.Groups[1].Value;
                        SmartOpCodes op;
                        switch (opcode)
                        {
                            case "1":
                                op = SmartOpCodes.Byte;
                                break;
                            case "2":
                                op = SmartOpCodes.Word;
                                break;
                            case "4":
                                op = SmartOpCodes.Dword;
                                break;
                            case "8":
                                op = SmartOpCodes.Qword;
                                break;
                            case "D":
                                op = SmartOpCodes.Double;
                                break;
                            case "L":
                                op = SmartOpCodes.LString;
                                break;
                            case "S":
                                op = SmartOpCodes.String;
                                break;
                            case "B":
                                op = SmartOpCodes.Buffer;
                                break;
                            default:
                                op = 0;
                                break;
                        }
                        string arg = smartMatch.Groups[2].Value;
                        if (op != 0)
                        {
                            switch (op)
                            {
                                case SmartOpCodes.Byte:
                                    oBuffer.Encode1(byte.Parse(arg));
                                    break;
                                case SmartOpCodes.Word:
                                    oBuffer.Encode2(short.Parse(arg));
                                    break;
                                case SmartOpCodes.Dword:
                                    oBuffer.Encode4(Int32.Parse(arg));
                                    break;
                                case SmartOpCodes.Qword:
                                    oBuffer.Encode8(long.Parse(arg));
                                    break;
                                case SmartOpCodes.Double:
                                    oBuffer.EncodeDouble(double.Parse(arg));
                                    break;
                                case SmartOpCodes.LString:
                                    oBuffer.EncodeStr(arg);
                                    break;
                                case SmartOpCodes.String:
                                    string[] str = arg.Split(new char[] { ':' }, 2);
                                    oBuffer.EncodeStr(str[0], Int32.Parse(str[1]));
                                    break;
                                case SmartOpCodes.Buffer:
                                    string[] buf = arg.Split(new char[] { ':' }, 2);
                                    oBuffer.EncodeBuffer(ParseSmart(buf[0]), Int32.Parse(buf[1]));
                                    break;
                            }
                        }
                    }
                    else if (Regex.IsMatch(input, rawPattern))
                    {
                        byte raw = Convert.ToByte(input, 16);
                        oBuffer.Encode1(raw);
                    }
                }
                return oBuffer.ToArray();
            }
            throw new ArgumentException(nameof(s));
        }

        public static bool TryParseSmart(string s, out byte[] value)
        {
            try
            {
                value = ParseSmart(s);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }
    }

}
