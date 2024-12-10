using DSharpBot.Commands;
using DSharpBot.Config;
using DSharpBot.Helper;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Net.Mail;
using System.Threading.Channels;

namespace DSharpBot
{
	static class Program
	{
		public static DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;

		public static Configuration Config { get; set; } = ConfigFileService.Config;

		public static DiscordClient Client { get; private set; }

		public static List<(DiscordMessage message, Func<DiscordClient, MessageReactionAddedEventArgs, Task> handler, CancellationToken cancellation)> ReactionEvents { get; } = [];

		public static void OnReactionAdded(this DiscordMessage message, Func<DiscordClient, MessageReactionAddedEventArgs, Task> handler, out CancellationTokenSource cancellation)
		{
			cancellation = new CancellationTokenSource();

			ReactionEvents.Add((message, handler, cancellation.Token));
		}

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
			DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(Config.Bot.Token, DiscordIntents.All);

			builder.UseCommands((IServiceProvider serviceProvider, CommandsExtension extension) =>
			{
				extension.AddCommands([typeof(VoteCommands)]);
				extension.AddProcessor(new SlashCommandProcessor());
			});

			builder.ConfigureEventHandlers(events =>
			{
				#region KEKW Smoking
				events.HandleMessageReactionAdded(async (sender, args) =>
				{
					if (args.Message.Reactions.Where(x => x.Emoji.Name == "🚬").Any())
					{
						await args.Message.CreateReactionAsync(DiscordEmoji.FromName(sender, ":smoking:"));
					}

					ReactionEvents.RemoveAll(x =>
					{
						if (x.cancellation.IsCancellationRequested)
						{
							return true;
						}
						else if (x.message == args.Message)
						{
							x.handler.Invoke(sender, args);
						}

						return false;
					});
				});
				#endregion

				#region 691
				Dictionary<DiscordUser, DateTimeOffset> banned = [];

				events.HandleMessageCreated(async (sender, args) =>
				{
					if (args is { Author.IsCurrent: false, Channel.Name: "691" })
					{
						foreach (var ban in banned.Where(x => x.Value < DateTimeOffset.UtcNow))
						{
							banned.Remove(ban.Key);
						}

						if (banned.ContainsKey(args.Author))
						{
							await args.Message.DeleteAsync("You are still banned...");
						}
						else if (args.Message
							is { Attachments.Count: > 0 }
							or { MessageSnapshots: [.., { Message.Attachments.Count: > 0 }] })
						{
							int days = Random.Shared.Next(1, 31);

							await args.Message.RespondAsync($"For making this post, this user was banned for {days} days");

							banned.Add(args.Author, DateTimeOffset.UtcNow.AddDays(days));
						}
					}
				});
				#endregion
			});

			Client = builder.Build();
#if DEBUG
			await Client.ConnectAsync(new DiscordActivity("DEBUG MODE"));
#else
			await Client.ConnectAsync(new DiscordActivity("Rainbow Six Siege", DiscordActivityType.Playing));
#endif

			await Task.Delay(-1);
		}
	}
}
