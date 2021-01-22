using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.WechatSDK.Common.Helpers
{
    public static class DateTimeHelper
    {
        /// <summary>Unix起始时间</summary>
        public static readonly DateTime BaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>转换微信DateTime时间到C#时间</summary>
        /// <param name="dateTimeFromXml">微信DateTime</param>
        /// <returns></returns>
        public static DateTime GetDateTimeFromXml(long dateTimeFromXml)
        {
            DateTime dateTime = DateTimeHelper.BaseTime;
            dateTime = dateTime.AddSeconds((double) dateTimeFromXml);
            return dateTime.ToLocalTime();
        }

        /// <summary>转换微信DateTime时间到C#时间</summary>
        /// <param name="dateTimeFromXml">微信DateTime</param>
        /// <returns></returns>
        public static DateTime GetDateTimeFromXml(string dateTimeFromXml) => DateTimeHelper.GetDateTimeFromXml(long.Parse(dateTimeFromXml));

        /// <summary>获取Unix时间戳</summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long GetUnixDateTime(DateTime dateTime) => (long) (dateTime.ToUniversalTime() - DateTimeHelper.BaseTime).TotalSeconds;
    }
}
