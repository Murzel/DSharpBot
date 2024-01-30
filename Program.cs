using DSharpBot.Commands;
using DSharpBot.Config;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Reflection;

namespace DSharpBot
{
	static class Program
	{
		public static DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;

		public static Configuration Config { get; set; } = ConfigFileService.Config;

		static void Main(string[] args)
		{
			_ = CreatedAt;

			Program
				.CreateHost()
				.GetAwaiter()
				.GetResult();
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

			#region KEKW Smoking
			discord.MessageReactionAdded += async (sender, args) =>
			{
				if (args.Message.Reactions.Where(x => x.Emoji.Name == "🚬").Any())
				{
					await args.Message.CreateReactionAsync(DiscordEmoji.FromName(sender, ":smoking:"));
				}

				await Task.CompletedTask;
			};
			#endregion

			#region 691
			List<(ulong user, DateTimeOffset until)> banned = new();

			discord.MessageCreated += async (sender, args) =>
			{
				if (!args.Author.IsCurrent && args.Channel.Name == "691")
				{
					banned.RemoveAll(x => x.until < DateTimeOffset.UtcNow);

					if (banned.Any(x => x.user == args.Author.Id))
					{
						await args.Message.DeleteAsync("You are still banned...");
					}
					else if (args.Message.Attachments.Count > 0)
					{
						int minutes = Random.Shared.Next(1, 59);

						await args.Message.RespondAsync($"For making this post, this user was banned for {minutes} minutes");

						banned.Add((args.Author.Id, DateTimeOffset.UtcNow.AddMinutes(minutes)));
					}
				}

				await Task.CompletedTask;
			};
			#endregion

			commands.RegisterCommands<VoteCommands>();

			await discord.ConnectAsync();

			discord.Ready += async (client, args) =>
			{
				await discord.UpdateStatusAsync(
					new DiscordActivity("Rainbow Siege 6", ActivityType.Playing)
				);

				await Task.CompletedTask;
			};

			await Task.Delay(-1);
		}
	}
}
