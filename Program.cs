using DSharpBot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using System.Text.Json;

namespace DSharpBot
{
	static class Program
    {
        public static DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;

        public static Config.Config Config { get; set; } = ((Func<Config.Config>)(() => {
            Config.Config res;
            string configPath = "Config.json";
            string configJson;

            if (File.Exists(configPath))
                configJson = File.ReadAllText(configPath);
            else
                throw new Exception($"Configuration file \"{configPath}\" in \"{Directory.GetCurrentDirectory()}\" not found!");

            try
			{
                res = JsonSerializer.Deserialize<Config.Config>(configJson)  ?? throw new Exception("Failed to process configuration file");
            }
            catch (Exception ex)
			{
				throw new("Failed to process configuration file");
            }
            
            return res;
        }))();

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
				StringPrefixes = new[] { "!" },
			});

			commands.RegisterCommands<VoteCommands>();

			await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
