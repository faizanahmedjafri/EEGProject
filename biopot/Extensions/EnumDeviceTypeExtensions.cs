using System;
using biopot.Enums;

namespace biopot.Extensions
{
    public static class EnumDeviceTypeExtensions
    {
        public static string GetName(this EDeviceType type)
        {
            if (type == EDeviceType.EEGorEMG)
                return "EEG/EMG";
            else if (type == EDeviceType.BioImpedance)
                return "Bio Impedance";
            else
                return "Accelerometer";
        }
        public static string GetName(this Enum val)
        {
            return Enum.GetName(val.GetType(), val);
        }

        public static Tenum GetEnumValue<Tenum>(this string enumStr) where Tenum : struct
        {
            var ret = default(Tenum);
            if (Enum.TryParse<Tenum>(enumStr, out ret))
                return ret;

            return ret;
        }
    }
}