// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using static System.DayOfWeek;

namespace RapiBarFetch;

internal static class HolidayHelper
{
    public static HashSet<DateOnly> GetHolidays()
    {
        var dates = new List<DateOnly>();

        for (var year = Known.MinTradeDate.Year; year <= DateTime.Today.Year; year++)
            dates.AddRange(GetHolidaysForYear(year));

        return new HashSet<DateOnly>(dates);
    }

    private static List<DateOnly> GetHolidaysForYear(int year)
    {
        var holidays = new List<DateOnly>();

        static DateOnly Adjust(DateOnly holiday)
        {
            if (holiday.DayOfWeek == Saturday)
                return holiday.AddDays(-1);
            else if (holiday.DayOfWeek == Sunday)
                return holiday.AddDays(1);
            else
                return holiday;
        }

        DateOnly EasterSunday()
        {
            int day = 0;
            int month = 0;

            int g = year % 19;
            int c = year / 100;
            int h = (c - c / 4 - (8 * c + 13) / 25 + 19 * g + 15) % 30;
            int i = h - h / 28 * (1 - h / 28 * (29 / (h + 1)) * ((21 - g) / 11));

            day = i - ((year + year / 4 + i + 2 - c + c / 4) % 7) + 28;

            month = 3;

            if (day > 31)
            {
                month++;

                day -= 31;
            }

            return new DateOnly(year, month, day);
        }

        DateOnly GoodFriday() => EasterSunday().AddDays(-2);

        static DateOnly GetMonday(DateOnly date, int factor)
        {
            var dayOfWeek = date.DayOfWeek;

            while (dayOfWeek != Monday)
            {
                date = date.AddDays(factor);

                dayOfWeek = date.DayOfWeek;
            }

            return date;
        }

        DateOnly MartinLutherKingDay()
        {
            var day = (from d in Enumerable.Range(1, 31)
                       where new DateTime(year, 1, d).DayOfWeek == Monday
                       select d).ElementAt(2);

            return new DateOnly(year, 1, day);
        }

        DateOnly PresidentsDay()
        {
            var day = (from d in Enumerable.Range(1, 28)
                       where new DateTime(year, 2, d).DayOfWeek == Monday
                       select d).ElementAt(2);

            return new DateOnly(year, 2, day);
        }

        DateOnly ThanksgivingDay()
        {
            var day = (from d in Enumerable.Range(1, 30)
                       where new DateTime(year, 11, d).DayOfWeek == Thursday
                       select d).ElementAt(3);

            return new DateOnly(year, 11, day);
        }

        // New Years Day
        holidays.Add(Adjust(new DateOnly(year, 1, 1)));

        // Martin Luther King Day - Third Monday in January
        holidays.Add(MartinLutherKingDay());

        // President's Day - Third Monday in February
        holidays.Add(PresidentsDay());

        // Good Friday - First Friday Before Easter Sunday
        holidays.Add(GoodFriday());

        // Memorial Day -- Last Monday in May 
        holidays.Add(GetMonday(new DateOnly(year, 5, 31), -1));

        // Juneteenth
        if (year >= 2022)
            holidays.Add(Adjust(new DateOnly(year, 6, 19)));

        // Independence Day
        holidays.Add(Adjust(new DateOnly(year, 7, 4)));

        // Labor Day -- 1st Monday in September 
        holidays.Add(GetMonday(new DateOnly(year, 9, 1), 1));

        // Thanksgiving Day -- 4th Thursday in November 
        holidays.Add(ThanksgivingDay());

        // Day After Thanksgiving Day 
        holidays.Add(ThanksgivingDay().AddDays(1));

        // Christmas Day 
        holidays.Add(Adjust(new DateOnly(year, 12, 25)));

        // Next New Years Day
        var nextYearNewYears = Adjust(new DateOnly(year + 1, 1, 1));

        if (nextYearNewYears.Year == year)
            holidays.Add(nextYearNewYears);

        return holidays.Where(h => h >= Known.MinTradeDate).ToList();
    }
}