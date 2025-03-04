using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class BitFlag<T>
        where T : Enum
    {
        public T Value { get; set; }

        public override string ToString()
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            string ret = "";
            int count = 0;
            foreach (var v in values)
            {
                if (Value.HasFlag(v))
                {
                    var name = Enum.GetName(typeof(T), v);
                    Debug.Assert(name != null);
                    if (count == 0)
                        ret = name;
                    else
                        ret += $" | {name}";
                    count++;
                }
            }
            if (count == 0)
            {
                var name = Enum.GetName(typeof(T), 0);
                Debug.Assert(name != null);
                return name;
            }

            return ret;
        }

        public BitFlag(T v)
        {
            Value = v;
        }
    }

    public class BitFlag
    {
        public abstract class Field
        {
            public abstract List<string> GetValues(uint x);
            public abstract uint GetMask();
        }

        public class BoolField : Field
        {
            private string _name;
            private int _shift;

            public override uint GetMask() => BitUtils.GetMask(_shift, 1);

            public override List<string> GetValues(uint x) =>
                (x & GetMask()) == 0u ? new List<string>() : new List<string>() { _name };

            public BoolField(string name, int shift) => (_name, _shift) = (name, shift);
        }

        public class EnumField : Field
        {
            private Dictionary<uint, string> _values;
            private int _shift;
            private int _len;
            private bool _preShifted;

            public override uint GetMask() => BitUtils.GetMask(_shift, _len);

            public override List<string> GetValues(uint x)
            {
                uint masked = (x & GetMask()) >> (_preShifted ? 0 : _len);
                return new()
                {
                    _values.ContainsKey(masked) ? _values[masked] : $"0x{masked:X}<<{_shift}",
                };
            }

            public EnumField(
                int shift,
                int len,
                Dictionary<uint, string> values,
                bool preShifted = false
            ) => (_shift, _len, _values, _preShifted) = (shift, len, values, preShifted);

            public static EnumField FromEnum<T>(int shift, int len, bool preShifted = false)
                where T : struct, Enum
            {
                Dictionary<uint, string> values = new Dictionary<uint, string>();
                foreach (var v in Enum.GetValues<T>())
                    values.Add(Convert.ToUInt32(v), v.ToString());
                return new EnumField(shift, len, values, preShifted);
            }

            public static EnumField FromBits(int shift, params string[] values)
            {
                Dictionary<uint, string> dict = new Dictionary<uint, string>();
                int bits = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    if ((1 << (bits + 1) & i) != 0)
                        bits++;

                    dict.Add((uint)i, values[i]);
                }
                return new EnumField(shift, bits, dict);
            }
        }

        private Field[] _fields;

        public List<string> GetFlags(uint x)
        {
            List<string> flags = new List<string>();

            Array.ForEach(_fields, f => flags.AddRange(f.GetValues(x)));

            return flags;
        }

        public BitFlag(params Field[] fields) => _fields = fields;

        public string ToString(uint x) => string.Join("|", GetFlags(x));
    }

    public static class BitUtils
    {
        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining
        )]
        public static uint GetBits(uint x, int shift, int len) =>
            (x & GetMask(shift, len)) >> shift;

        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining
        )]
        public static uint GetMask(int shift, int len) => ((1u << len) - 1u) << shift;

        private static List<string> GetFlags<T>(uint x, out uint outX, uint mask = uint.MaxValue)
            where T : struct, Enum => GetFlags(new List<T>(Enum.GetValues<T>()), x, out outX, mask);

        private static List<string> GetFlags<T>(
            List<T> values,
            uint x,
            out uint outX,
            uint mask = uint.MaxValue
        )
            where T : struct, Enum
        {
            Dictionary<string, uint> dict = new();
            values.ForEach(v => dict.Add(v.ToString(), Convert.ToUInt32(v)));
            return GetFlags(dict, x, out outX, mask);
        }

        private static List<string> GetFlags(
            Dictionary<string, uint> flags,
            uint x,
            out uint outX,
            uint mask = uint.MaxValue
        )
        {
            outX = x;
            List<string> ret = new List<string>();
            foreach (var flag in flags)
            {
                if ((outX & mask & flag.Value) != 0)
                {
                    ret.Add(flag.Key);
                    outX |= ~flag.Value;
                }
            }

            if (flags.ContainsValue(0))
                ret.Add(flags.First(x => x.Value == 0).Key);

            return ret;
        }

        public static List<string> GetFlagsSet(
            Dictionary<string, uint> flags,
            uint x,
            uint mask = uint.MaxValue
        )
        {
            var values = GetFlags(flags, x, out uint res, mask);
            if (res != 0)
                values.Add($"0x{res:X}");

            return values;
        }

        public static List<string> GetFlagsSet<T>(uint x, uint mask = uint.MaxValue)
            where T : struct, Enum
        {
            var values = GetFlags<T>(x, out uint res, mask);
            if (res != 0)
                values.Add($"0x{res:X}");

            return values;
        }

        public static string ParseFlag<T>(uint x, uint mask = uint.MaxValue)
            where T : struct, Enum
        {
            var flags = GetFlagsSet<T>(x, mask);
            return string.Join("|", flags);
        }
    }
}
