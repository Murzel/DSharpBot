using DSharpBot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;

namespace DSharpBot
{
    class Program
    {
        public static DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;
        public static ulong MyMemberId { get; } = 219828093072834561;

        static void Main(string[] args)
        {
            _ = CreatedAt;
            CreateHost().GetAwaiter().GetResult();
        }

        static async Task CreateHost()
        {
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = "NzcyNjA3NTYzODMzMjc4NDY2.G90sMW.h7ggShcEwqx76eD9UUzHITBj1bIbM5wClZI4j8",
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All
            });

			var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
			{
				StringPrefixes = new[] { "!" },
			});

			commands.RegisterCommands<VoteCommands>();

			await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
