namespace DSharpBot.Config
{
	public class BotConfig
	{
		public ulong Id { get; init; }
		public string Mention { get => $"<@{Id}>"; }

		public string Token { get; init; }

		public string CommandPrefix { get; init; } = "!"; 
	}
}
