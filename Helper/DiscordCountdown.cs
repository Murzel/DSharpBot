using DSharpPlus.Commands;
using DSharpPlus.Entities;
using static DSharpPlus.Formatter;

namespace DSharpBot.Helper
{
	internal static class DiscordCountdown
	{
		const int Second = 1000;
		const int Minute = Second * 60;

		public static async Task<DiscordMessage> Countdown(DiscordMessage msg, int time, Func<int, string> text, Func<bool>? cancel = null)
		{
			using var timer = new System.Timers.Timer(Minute);

			cancel ??= (() => false);

			timer.Elapsed += async (s, e) =>
			{
				if (--time < 1)
				{
					timer.Stop();
					return;
				}

				msg = await msg.ModifyAsync(text(time));
			};

			timer.Start();

			await Task.Delay(Second);

			while (timer.Enabled)
			{
				// Prüfe alle 10 Sekunden, ob die Abbruchbedingung erfüllt worden ist
				timer.Enabled = !cancel.Invoke();

				for (int i = 0; i < 10; i++)
					if (timer.Enabled) await Task.Delay(Second);
					else break;
			}

			return msg;
		}

		public static string GetText(int time) => $"{Bold($"{time} Minute{(time == 1 ? "" : "n")}")}";
	}
}
