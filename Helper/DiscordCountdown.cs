using DSharpPlus.Commands;
using DSharpPlus.Entities;
using static DSharpPlus.Formatter;

namespace DSharpBot.Helper
{
	internal static class DiscordCountdown
	{
		const string prefix = "Verbleibende Zeit:";
		const int second = 1000;
		const int minute = second * 60;

		public static async Task<DiscordMessage> Countdown(DiscordMessage msg, int time, Func<bool>? e = null)
		{
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

			await Task.Delay(second);

			while (timer.Enabled)
			{
				if (e?.Invoke() ?? false)
					timer.Enabled = false;

				for (int i = 0; i < 10; i++)
					if (timer.Enabled)
						await Task.Delay(second);
					else
						break;
			}

			e?.Invoke();
			await msg.ModifyAsync($"{prefix} {Bold("Vorbei!")}");

			return msg;
		}

		public static async Task<DiscordMessage> Countdown(CommandContext ctx, int time, Func<bool>? e = null)
		{
			await ctx.RespondAsync(time.Text());
			var msg = await ctx.GetResponseAsync();

			return await Countdown(msg!, time, e);
		}

		public static string GetInitText(int time) => time.Text();

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
