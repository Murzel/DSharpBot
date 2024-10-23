using DSharpPlus.Entities;

namespace DSharpBot.Config
{
	public class BotConfig
	{
		public ulong Id { get; init; }
		public string Mention { get => $"<@{Id}>"; }

		public string Token { get; init; }
	}
}
