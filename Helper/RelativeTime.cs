using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpBot.Helper
{
	public static class RelativeTime
	{
		public static string ToFormattedRelativeTime(this TimeSpan timespan)
		{
			var result = "";

			if (timespan.Days > 0)
			{
				result += $"{timespan.Days} day{(timespan.Days == 1 ? "" : "s")}";

				if (timespan.Hours > 0)
					result += $" and {timespan.Hours} hour{(timespan.Hours == 1 ? "" : "s")}";

				return result;
			}

			if (timespan.Hours > 0)
			{
				result += $"{timespan.Hours} hour{(timespan.Hours == 1 ? "" : "s")}";

				if (timespan.Minutes > 0)
					result += $" and {timespan.Minutes} minute{(timespan.Minutes == 1 ? "" : "s")}";

				return result;
			}

			if (timespan.Minutes > 0)
			{
				result += $"{timespan.Minutes} minute{(timespan.Minutes == 1 ? "" : "s")}";

				if (timespan.Seconds > 0)
					result += $" and {timespan.Seconds} second{(timespan.Seconds == 1 ? "" : "s")}";

				return result;
			}

			return result + $"{(int) timespan.TotalSeconds} second{((int) timespan.TotalSeconds == 1 ? "" : "s")}";
		}
	}
}
