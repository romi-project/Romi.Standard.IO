using System;
using System.Drawing;
using System.IO;

namespace Romi.Standard.IO
{
    public class InBuffer
    {
        private readonly MemoryStream _stream;
        private readonly byte[] _tempSpace = new byte[8];

        public InBuffer(byte[] buffer)
        {
            _stream = new MemoryStream(buffer, false);
        }

#if DESTRUCTOR
	        ~InBuffer()
	        {
	            stream.Dispose();
	        }
#endif

        private void Read(int count)
        {
            int n;
            if (count == 1)
            {
                n = _stream.ReadByte();
                if (n == -1)
                {
                    throw new EndOfStreamException();
                }
                _tempSpace[0] = (byte)n;
            }
            else if (count > 1)
            {
                int offset = 0;
                while (offset < count)
                {
                    n = _stream.Read(_tempSpace, offset, count);
                    if (n == 0)
                    {
                        throw new EndOfStreamException();
                    }
                    offset += n;
                }
            }
        }

        public byte Decode1()
        {
            Read(1);
            return _tempSpace[0];
        }

        public bool Decode1B()
        {
            return Decode1() != 0;
        }

        public sbyte Decode1S()
        {
            return (sbyte)Decode1();
        }

        public T Decode1Enum<T>() where T : struct, IConvertible
        {
            return (T)Enum.ToObject(typeof(T), Decode1S());
        }

        public short Decode2()
        {
            Read(2);
            return (short)(_tempSpace[0] | _tempSpace[1] << 8);
        }

        public ushort Decode2U()
        {
            return (ushort)Decode2();
        }

        public T Decode2Enum<T>() where T : struct, IConvertible
        {
            return (T)Enum.ToObject(typeof(T), Decode2());
        }

        public int Decode4P()
        {
            return Decode4() & int.MaxValue;
        }

        public int Decode4()
        {
            Read(4);
            return _tempSpace[0] | _tempSpace[1] << 8 | _tempSpace[2] << 16 | _tempSpace[3] << 24;
        }

        public uint Decode4U()
        {
            return (uint)Decode4();
        }

        public bool Decode4B()
        {
            return Decode4() != 0;
        }

        public T Decode4Enum<T>() where T : struct, IConvertible
        {
            return (T)Enum.ToObject(typeof(T), Decode4());
        }

        public T Decode4UEnum<T>() where T : struct, IConvertible
        {
            return (T)Enum.ToObject(typeof(T), Decode4U());
        }

        public long Decode8()
        {
            return (long)Decode8U();
        }

        public long Decode8P()
        {
            return Decode8() & long.MaxValue;
        }

        public ulong Decode8U()
        {
            Read(8);
            uint lo = (uint)(_tempSpace[0] | _tempSpace[1] << 8 |
                             _tempSpace[2] << 16 | _tempSpace[3] << 24);
            uint hi = (uint)(_tempSpace[4] | _tempSpace[5] << 8 |
                             _tempSpace[6] << 16 | _tempSpace[7] << 24);
            return ((ulong)hi) << 32 | lo;
        }

        public byte[] DecodeBuffer(int count)
        {
            byte[] buffer = new byte[count];
            int n;
            int offset = 0;
            while (offset < count)
            {
                n = _stream.Read(buffer, offset, count);
                if (n == 0)
                {
                    throw new EndOfStreamException("파일의 끝입니다.");
                }
                offset += n;
            }
            return buffer;
        }

        public string DecodeStr(int length = 0)
        {
            bool nullTerminated = length < 0;
            if (length == 0)
            {
                length = Decode2();
            }
            byte[] value = DecodeBuffer(Math.Abs(length));
            if (nullTerminated)
            {
                Decode1();
            }
            string result = Locales.EncodeString(value);
            return result;
        }

        public double DecodeDouble()
        {
            return BitConverter.Int64BitsToDouble(Decode8());
        }

        public Point DecodePoint()
        {
            return new Point(Decode2(), Decode2());
        }

        public byte[] ToArray(bool fromBegin = true)
        {
            if (fromBegin)
                return _stream.ToArray();
            else
            {
                var tempStream = new MemoryStream();
                _stream.CopyTo(tempStream);
                return tempStream.ToArray();
            }
        }

        public int Remaining => (int)(_stream.Length - _stream.Position);

        public string ToStringAll()
        {
            return HexConverter.ToString(ToArray(true));
        }

        public override string ToString()
        {
            return HexConverter.ToString(ToArray(false));
        }
    }

}