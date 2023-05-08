using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Romi.Standard.IO
{
    public class OutBuffer
    {
        public static readonly OutBuffer Empty = new OutBuffer(0);

        private readonly MemoryStream _stream;

        public OutBuffer()
            : this(128)
        {
        }

        public OutBuffer(int capacity)
        {
            _stream = new MemoryStream(capacity);
        }

#if DESTRUCTOR
	        ~OutBuffer()
	        {
	            stream.Dispose();
	        }
#endif

        public static OutBuffer Wrap(byte[] buffer)
        {
            var ret = new OutBuffer();
            ret.EncodeBuffer(buffer);
            return ret;
        }

        public static OutBuffer Smart(string hex)
            => Wrap(HexConverter.ParseSmart(hex));

        public void Encode1(int value)
        {
            _stream.WriteByte((byte)value);
        }

        public void Encode1(Enum value)
        {
            Encode1(Convert.ToInt32(value));
        }

        public void Encode1(bool value)
        {
            _stream.WriteByte((byte)(value ? 1 : 0));
        }

        public void Encode1(byte value)
        {
            _stream.WriteByte(value);
        }

        public void Encode1(char value)
        {
            Encode1((byte)value);
        }

        public void Encode1(sbyte value)
        {
            _stream.WriteByte((byte)value);
        }

        public void Encode2(bool value)
        {
            Encode2(value ? 1 : 0);
        }

        public void Encode2(int value)
        {
            Encode2((short)value);
        }

        public void Encode2(short value)
        {
            EncodeBuffer(BitConverter.GetBytes(value));
        }

        public void Encode2(ushort value)
        {
            EncodeBuffer(BitConverter.GetBytes(value));
        }

        public void Encode2(Enum e)
        {
            Encode2(Convert.ToUInt16(e));
        }

        public void Encode4(int value)
        {
            EncodeBuffer(BitConverter.GetBytes(value));
        }

        public void Encode4(uint value)
        {
            EncodeBuffer(BitConverter.GetBytes(value));
        }

        public void Encode4(bool b)
        {
            Encode4(b ? 1 : 0);
        }

        [DebuggerNonUserCode]
        public void Encode4(Enum e)
        {
            try
            {
                Encode4(Convert.ToInt32(e));
            }
            catch
            {
                Encode4(Convert.ToUInt32(e));
            }
        }

        public void Encode8(long value)
        {
            EncodeBuffer(BitConverter.GetBytes(value));
        }

        public void Encode8(ulong value)
        {
            EncodeBuffer(BitConverter.GetBytes(value));
        }

        public void Encode1(bool? value) => Encode1(value ?? false);
        public void Encode1(int? value) => Encode1(value ?? 0);
        public void Encode1(byte? value) => Encode1(value ?? 0);
        public void Encode1(sbyte? value) => Encode1(value ?? 0);
        public void Encode2(short? value) => Encode2(value ?? 0);
        public void Encode2(ushort? value) => Encode2(value ?? 0);
        public void Encode4(int? value) => Encode4(value ?? 0);
        public void Encode4(uint? value) => Encode4(value ?? 0);
        public void Encode8(long? value) => Encode8(value ?? 0);
        public void Encode8(ulong? value) => Encode8(value ?? 0);
        

        public void EncodeBuffer(byte[] value, int length = 0)
        {
            int extraLength = 0;
            if (length != 0)
            {
                if (value.Length <= length)
                {
                    extraLength = length - value.Length;
                }
                else
                {
                    _stream.Write(value, 0, length);
                }
            }
            else
            {
                _stream.Write(value, 0, value.Length);
            }
            
            if (extraLength > 0)
            {
                for (var i = 0; i < extraLength; i++)
                    _stream.WriteByte(0x00);
                //_stream.SetLength(_stream.Length + extraLength);
                //Array.Fill(_stream.GetBuffer(), (byte)0x00, (int)_stream.Position, extraLength);
            }
        }

        public void EncodeBuffer(InBuffer iBuffer)
        {
            if (iBuffer != null)
            {
                EncodeBuffer(iBuffer.ToArray());
            }
        }

        public void EncodeBuffer(OutBuffer oBuffer)
        {
            if (oBuffer != null)
            {
                EncodeBuffer(oBuffer.ToArray());
            }
        }

        public void EncodeStr(string value, int length = 0)
        {
            bool nullTerminated = length < 0;
            if (value == null) value = "";
            byte[] array = Locales.DecodeString(value, Math.Abs(length));
            if (length == 0)
            {
                Encode2(array.Length);
            }
            EncodeBuffer(array, length);
            if (nullTerminated)
            {
                Encode1(0);
            }
        }

        public void EncodeDouble(double value)
        {
            Encode8(BitConverter.DoubleToInt64Bits(value));
        }

        public byte[] ToArray()
        {
            return _stream.ToArray();
        }

        public override string ToString()
        {
            return ToArray().ToString() ?? "";
        }
    }

}
