using DSharpBot.Commands;
using DSharpBot.Config;
using DSharpBot.R691;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpBot;

public static class Program
{
	public static DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;

	public static Configuration Config { get; set; } = ConfigFileService.Config;

	public static DiscordClient Client { get; private set; } = null!;

	public static List<(DiscordMessage message, Func<DiscordClient, MessageReactionAddedEventArgs, Task> handler, CancellationToken cancellation)> ReactionEvents { get; } = [];

	private static readonly CancellationTokenSource CancellationTokenSource = new();

	public static readonly CancellationToken CancellationToken = CancellationTokenSource.Token;

	public static void OnReactionAdded(this DiscordMessage message, Func<DiscordClient, MessageReactionAddedEventArgs, Task> handler, out CancellationTokenSource cancellation)
	{
		cancellation = new CancellationTokenSource();

		ReactionEvents.Add((message, handler, cancellation.Token));
	}

	static async Task Main()
	{
		_ = CreatedAt;

		AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

		await R691Handler.Init();
		await Program.CreateHost();
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
				if (args.Message.Reactions.Any(x => x.Emoji.Name == "🚬"))
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
			events.HandleMessageCreated(R691Handler.OnMessageCreated);
			#endregion
		});

		Client = builder.Build();

#if DEBUG
		await Client.ConnectAsync(new DiscordActivity("DEBUG MODE"));
#else
		await Client.ConnectAsync(new DiscordActivity("Rainbow Six Siege", DiscordActivityType.Playing));
#endif

		_ = Client.Handle691Unban();

		await Task.Delay(-1);
	}

	public static void OnProcessExit(object? sender, EventArgs e)
	{
		CancellationTokenSource.Cancel();
		R691Handler.DbConnection.Dispose();
	}
}
