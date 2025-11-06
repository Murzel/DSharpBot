using DSharpBot.Commands;
using DSharpBot.Config;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Collections.Concurrent;

namespace DSharpBot;

public static class Program
{
	public static DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;

	public static Configuration Config { get; set; } = ConfigFileService.Config;

	public static DiscordClient Client { get; private set; }

	public static List<(DiscordMessage message, Func<DiscordClient, MessageReactionAddedEventArgs, Task> handler, CancellationToken cancellation)> ReactionEvents { get; } = [];

	public static ConcurrentDictionary<DiscordUser, DateTimeOffset> Banned691 = [];

	private static readonly CancellationTokenSource CancellationTokenSource = new();

	private static readonly CancellationToken CancellationToken = CancellationTokenSource.Token;

	public static void OnReactionAdded(this DiscordMessage message, Func<DiscordClient, MessageReactionAddedEventArgs, Task> handler, out CancellationTokenSource cancellation)
	{
		cancellation = new CancellationTokenSource();

		ReactionEvents.Add((message, handler, cancellation.Token));
	}

	static void Main(string[] args)
	{
		_ = CreatedAt;
		_ = Handle691Unban();

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
			events.HandleMessageCreated(async (sender, args) =>
			{
				if (args is { Author.IsCurrent: false, Channel.Name: "691" })
				{
					if (Banned691.ContainsKey(args.Author))
					{
						await args.Message.DeleteAsync("User is still banned...").ConfigureAwait(false);
						return;
					}

					if (args.Message.ContainsMedia())
					{
						await Ban691User(args.Message);
					}
					else
					{
						// Bei einem Link wird die Vorschau von Discord nicht direkt geladen
						// Daher wird hier nochmal die Nachricht verzögert geprüft
						for (int i = 0; i < 3; i++)
						{
							await Task.Delay(1_000 * i);

							var reload_msg = await args.Message.Channel!.GetMessageAsync(args.Message.Id);

							if (reload_msg.ContainsMedia())
							{
								await Ban691User(reload_msg);
								break;
							}
						}
					}
				}
			});
#endregion
		});

		AppDomain.CurrentDomain.ProcessExit += new EventHandler((sender, e) => CancellationTokenSource.Cancel());

		Client = builder.Build();
#if DEBUG
		await Client.ConnectAsync(new DiscordActivity("DEBUG MODE"));
#else
		await Client.ConnectAsync(new DiscordActivity("Rainbow Six Siege", DiscordActivityType.Playing));
#endif

		await Task.Delay(-1);
	}

	private static bool ContainsMedia(this DiscordMessage msg) => msg
		is { Attachments.Count: > 0 }
		or { Embeds.Count: > 0 }
		or
		{
			MessageSnapshots: [..,
				{ Message.Attachments.Count: > 0 }
			 or { Message.Embeds.Count: > 0 }]
		};

	private static async Task<bool> Ban691User(DiscordMessage msg)
	{
		var time = Random.Shared.Next(1, 8);
		DateTimeOffset until;
		string timeWithUnit;

		if (msg.Author is null)
			return false;

#if !DEBUG
		until = DateTimeOffset.UtcNow.AddDays(time);
		timeWithUnit = $"{time} day";
#else
		until = DateTimeOffset.UtcNow.AddSeconds(time);
		timeWithUnit = $"{time} second";
#endif

		if (time > 1)
		{
			timeWithUnit += "s";
		}

		if (!Banned691.TryAdd(msg.Author, until))
			return false;
		
		await msg.RespondAsync($"For making this post, this user was banned for {timeWithUnit}");
		return true;
	}

	private static async Task Handle691Unban()
	{
		while (true)
		{
			if (CancellationToken.IsCancellationRequested) 
				return;

			foreach (var ban in Banned691.Where(x => x.Value < DateTimeOffset.UtcNow))
			{
				if (Banned691.TryRemove(ban.Key, out _)) await ban.Key
					.SendMessageAsync("You're now unbanned from 691 and can submit stupid posts again 🚬")
					.ConfigureAwait(false);
			}

			await Task.Delay(10_000, CancellationToken);
		}
	}
}
