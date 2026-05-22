using System.Globalization;

namespace UM_Project.Helpers
{
    public static class AlbanianFormat
    {
        private static readonly CultureInfo Sq = CultureInfo.GetCultureInfo("sq-AL");

        /// <summary>Example: E premte, maj 22, 2026</summary>
        public static string LongDate(DateTime date)
        {
            var day = date.ToString("dddd", Sq);
            if (!string.IsNullOrEmpty(day))
                day = char.ToUpper(day[0]) + day[1..];
            return $"{day}, {date.ToString("MMMM d, yyyy", Sq)}";
        }

        public static string ShortDate(DateTime date) => date.ToString("d MMM yyyy", Sq);

        public static string DayName(DateTime date)
        {
            var day = date.ToString("dddd", Sq);
            return string.IsNullOrEmpty(day) ? day : char.ToUpper(day[0]) + day[1..];
        }
    }
}
