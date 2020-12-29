using System;

namespace MVCApp.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime GetDate() => DateTime.Now.Date;
        public DateTime GetDateTime() => DateTime.Now;

        public DateTime GetWeekStart()
        {
            DateTime today = GetDate();
            int diff = DayOfWeek.Monday - today.DayOfWeek;
            return today.AddDays(diff).Date;
        }

        public DateTime GetWeekEnd() => GetWeekStart().AddDays(6);
    }

    public interface IDateTimeService
    {
        DateTime GetDate();
        DateTime GetDateTime();
        DateTime GetWeekStart();
        DateTime GetWeekEnd();
    }
}
