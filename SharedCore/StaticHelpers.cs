using System;
using System.Globalization;

namespace SharedCore
{
    public static class StaticHelpers
    {
        public static string CheckLogTimeStampOverdue(string data, int overdue, DateTime dateTimeNow)
        {
            string _timeFormat = "MM/dd/yy HH:mm:ss";
            int _overdueIndex = 0;
            int _overdueLineLendth = 0;
            var _thirtyDayAgoDate = dateTimeNow.AddDays(overdue);
            //var _thirtyDayAgoDate = dateTimeNow.AddMinutes(overdue);
            var _overdueDate = _thirtyDayAgoDate;

            if(string.IsNullOrEmpty(data))
            {
                return string.Empty;
            }

            var linesArray = data.Split('\n');

            for (int i = linesArray.Length - 1; i >= 0; i--) 
            {
                var line = linesArray[i];
                if(!string.IsNullOrEmpty(line))
                {
                    var dateString = line.Substring(0, _timeFormat.Length);
                    var newDateString = dateString.Replace(".", "/");
                    var dateTime = DateTime.ParseExact(newDateString, _timeFormat, CultureInfo.InvariantCulture);

                    if (dateTime <= _thirtyDayAgoDate)
                    {
                        _overdueIndex = data.IndexOf(dateTime.ToString(_timeFormat), StringComparison.CurrentCulture);
                        _overdueDate = dateTime;
                        _overdueLineLendth = line.Length;
                        break;
                    }
                }
                else
                {
                    continue;
                }
            }   
            var count = _overdueIndex + _overdueLineLendth;
            var newStr = data.Remove(0, count);

            return newStr;
        }
    }
}
