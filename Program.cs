using DSharpBot.Commands;
using DSharpBot.Config;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using System.Text.Json;

namespace DSharpBot
{
	static class Program
    {
        public static DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;

        public static Configuration Config { get; set; } = ConfigFileService.Config;

		static void Main(string[] args)
        {
            _ = CreatedAt;

            CreateHost().GetAwaiter().GetResult();
        }

        static async Task CreateHost()
        {
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = Config.Bot.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All
            });

			var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
			{
				StringPrefixes = new[] { Config.Bot.CommandPrefix },
			});

			commands.RegisterCommands<VoteCommands>();

			await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
