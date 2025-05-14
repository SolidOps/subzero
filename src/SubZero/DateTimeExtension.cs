using System;
using System.Globalization;

namespace SolidOps.SubZero
{
    public static class DateTimeExtension
    {
        public static DateTime Previous(this DateTime dateTime, DayOfWeek dayOfWeek, bool includeCurrentDay = true)
        {
            int delta = dayOfWeek - dateTime.DayOfWeek;
            if (delta > 0 || (delta == 0 && !includeCurrentDay))
                delta -= 7;
            return dateTime.AddDays(delta);
        }

        public static DateTime Next(this DateTime dateTime, DayOfWeek dayOfWeek, bool includeCurrentDay = false)
        {
            int delta = dayOfWeek - dateTime.DayOfWeek;
            if (delta < 0 || (delta == 0 && !includeCurrentDay))
                delta += 7;
            return dateTime.AddDays(delta);
        }

        // This presumes that weeks start with Monday.
        // Week 1 is the 1st week of the year with a Thursday in it.
        public static int GetIso8601WeekOfYear(this DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static DateTime OrMax(this DateTime aDate, DateTime otherDate)
        {
            if (aDate > otherDate)
                return aDate;
            else
                return otherDate;
        }

        public static DateTime OrMin(this DateTime aDate, DateTime otherDate)
        {
            if (aDate < otherDate)
                return aDate;
            else
                return otherDate;
        }
    }
}
