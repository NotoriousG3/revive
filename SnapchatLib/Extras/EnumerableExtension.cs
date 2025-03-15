using SnapProto.Snapchat.Search;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SnapchatLib.Extras
{
    public static class EnumerableExtension
    {
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }

        static Random _R = new Random();
        public static T RandomEnumValue<T>()
        {
            var v = Enum.GetValues(typeof(T));
            return (T)v.GetValue(_R.Next(v.Length));
        }

        public static SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign RandomEnumValueHappeningNowHoroscope_AstrologicalSign()
        {
            SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign HappeningNowHoroscopeAstrologicalSign;
            do
            {
                HappeningNowHoroscopeAstrologicalSign = EnumerableExtension.RandomEnumValue<SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign>();
            } while (HappeningNowHoroscopeAstrologicalSign == SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Unset);
            return HappeningNowHoroscopeAstrologicalSign;
        }
    }
}