using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class EncodingExtensions
    {
        public static Encoding CodeSet = Encoding.UTF8;

        public static byte[] GetRaw(this string str)
        {
            return CodeSet.GetBytes(str);
        }

        public static int GetCharLen(this byte[] dat)
        {
            int strlen = 0;
            for (strlen = 0; strlen < dat.Length && dat[strlen] != 0; strlen++)
                ;
            return strlen;
        }
        public static string GetString(this byte[] raw)
        {
            return CodeSet.GetString(raw, 0 , raw.GetCharLen());
        }
    }
}
