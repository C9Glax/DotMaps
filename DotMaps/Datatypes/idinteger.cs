using System;

namespace DotMaps.Datatypes
{
    public struct idinteger
    {
        byte[] value;
        public idinteger(ulong val)
        {
            if (val >= (1 << 40))
                throw new ArgumentOutOfRangeException("Value too large.");
            this.value = BitConverter.GetBytes(val);
        }

        public static explicit operator idinteger(ulong value)
        {
            return new idinteger(value);
        }

        public static implicit operator ulong(idinteger me)
        {
            return BitConverter.ToUInt64(me.value, -24);
        }

        public static idinteger Parse(object val)
        {
            Type valtype = val.GetType();
            if(valtype == typeof(ulong))
            {
                if ((ulong)val >= (1 << 40))
                    throw new ArgumentOutOfRangeException("Value too large.");
                return new idinteger((ulong)val);
            }else if(valtype == typeof(string))
            {
                ulong converted = Convert.ToUInt64(val);
                if (converted >= (1 << 40))
                    throw new ArgumentOutOfRangeException("Value too large.");
                return new idinteger(converted);
            }
            throw new ArgumentException("Value can not be parsed.");
        }

        public override string ToString()
        {
            return BitConverter.ToUInt64(this.value, -24).ToString();
        }
    }
}
