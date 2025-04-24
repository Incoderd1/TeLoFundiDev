using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Shared.Utils
{
    public static class DateTimeExtensions
    {
        public static DateTime? ToUtcSafe(this DateTime? date)
            => date?.Kind == DateTimeKind.Utc ? date : date?.ToUniversalTime();

        public static DateTime ToUtcSafe(this DateTime date)
            => date.Kind == DateTimeKind.Utc ? date : date.ToUniversalTime();
    }
}
