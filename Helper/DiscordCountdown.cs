using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using static DSharpPlus.Formatter;

namespace DSharpBot.Helper
{
	internal static class DiscordCountdown
	{
		const string prefix = "Verbleibende Zeit";
		const int second = 1000;
		const int minute = second * 60;

		public static void Countdown(CommandContext ctx, int time, Func<bool>? cancel_event = null)
		{
			if (time < 0)
				time = 0;

			var msg = ctx.Channel.SendMessageAsync(time.Text()).Result;
			var timer = new System.Timers.Timer(minute);

			timer.Elapsed += (s, e) =>
			{
				if (--time < 1)
				{
					timer.Stop();
					return;
				}

				time.Update(msg);
			};

			timer.Start();

			Thread.Sleep(second);

			while(timer.Enabled)
			{
				if (cancel_event?.Invoke() ?? false)
					timer.Enabled = false;

				for (int i = 0; i < 10; i++)
					if (timer.Enabled)
						Thread.Sleep(second);
					else
						break;
			}

			msg.ModifyAsync($"{prefix} {Bold("Vorbei!")}");
		}

		private static void Update(this ref int countdown, DiscordMessage msg)
		{
			msg.ModifyAsync(countdown.Text()).Wait();
		}

		private static string Text(this int time)
		{
			return $"{prefix} {Bold($"{time} Minute{(time == 1 ? "" : "n")}")}";	
		}
	}
}
