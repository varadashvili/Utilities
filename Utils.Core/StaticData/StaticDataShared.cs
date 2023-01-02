using System;
using System.Collections.Generic;
using System.Text;

using NodaTime;

namespace Utils.Core.StaticData
{
    public static class StaticDataShared
    {
        public static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static readonly DateTime? Jan1st1980 = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        internal static DateTimeZone TbilisiDateTimeZone { get { return DateTimeZoneProviders.Tzdb["Asia/Tbilisi"]; } }

        public static List<string> GeorgianSymbols = new List<string> { "ა", "ბ", "ც", "დ", "ე", "ფ", "გ", "ჰ", "ი", "ჯ", "კ", "ლ",
            "მ", "ნ", "ო", "პ", "ქ", "რ", "ს", "ტ", "უ", "ვ", "წ", "ხ", "ყ", "ზ", "თ", "ღ", "შ", "ჟ", "ძ", "ჩ", "ჭ" };

        public static List<string> EnglishSymbols = new List<string> { "a", "b", "ts", "d", "e", "f", "g", "h", "i", "j", "k", "l",
            "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "ts", "kh", "k", "z", "t", "gh", "sh", "dj", "dz", "ch", "ch" };
    }
}
