using DSharpBot.Config;
using DSharpBot.Helper;
using DSharpPlus.Entities;
using System.Text.RegularExpressions;
using DSharpPlus.Commands;
using System.ComponentModel;
using static DSharpPlus.Formatter;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

namespace DSharpBot.Commands;

public partial class VoteCommands 
{
	[Command("ruined"), Description("Berühmtes Zitat")]
	public static async Task RuinedCommand(CommandContext ctx, [Parameter("Troll"), Description("Der User, der deinen Abend ruiniert hat")] DiscordMember? ruiner = null)
	{
		if (ruiner?.IsCurrent ?? false)
		{
			var kekwait = ctx.Guild!.GetEmojiByName("KEKWait");
			await ctx.RespondAsync($"Unmöglich {kekwait ?? ""}");
			return;
		}
		
		await ctx.RespondAsync($"""
			{Italic("Du")} hast mein ganzen Abend ruiniert {(ruiner is not null ? Bold(ruiner.Nickname) : "")} {ctx.GetEmojis().PeepoWTF}

			Wenn {Italic("du")} nicht mein Premade wärst, würde ich {Bold("dich")} reporten {ctx.GetEmojis().FeelsWeirdMan} ~Jonas 2021
			""");
	}

	[Command("uptime"), AllowedProcessors<SlashCommandProcessor>(), Description("Check out how long the bot is running")]
	public static async Task UptimeCommand(CommandContext ctx)
	{
		await ctx.RespondAsync($"Since: {Bold(Program.CreatedAt.ToString())}\nUptime: {Bold((DateTimeOffset.Now - Program.CreatedAt).ToFormattedRelativeTime())}");
	}

	[Command("countdown"), Description("Zählt eine Zeit runter")]
	public static async Task CountdownCommand(CommandContext ctx, [Parameter("Minuten")] int minutes)
	{
		static string text(int time) => $"Verbleibende Zeit: {DiscordCountdown.GetText(time)}";

		if (minutes > 15)
		{
			await ctx.RespondAsync($"Du brauchst Lord Plus, um einen längeren als 15 Minuten Countdown zu machen!");
			return;
		}

		if (minutes < 0)
			return;

		await ctx.RespondAsync(text(minutes));
		
		var msg = await ctx.GetResponseAsync();
		msg = await DiscordCountdown.Countdown(msg!, minutes, text);
		msg = await msg.ModifyAsync(Bold("Vorbei!"));
	}

	[Command("anwesenheit"), Description("Prüft, ob jemand da ist")]
	public static async Task AttendanceCheckCommand(CommandContext ctx, [Parameter("Uhrzeit"), Description("Im Format hh:mm z.B. 6:00")] string timeAsString = "jetzt")
	{
		if (timeAsString is null || timeAsString.Equals("jetzt", StringComparison.CurrentCultureIgnoreCase))
		{
			await ctx.RespondAsync($"Jooo {ctx.GetEmojis().Smoking}..., jemand da?");

			var msg = await ctx.GetResponseAsync();

			await msg!.CreateReactionAsync(ctx.GetEmojis().ThumbsUp);
			return;
		}
		else if (TimeRegex().IsMatch(timeAsString))
		{
			const int wait = 5;
			DiscordMessage msg;
			Dictionary<DiscordUser, bool> challengers = [];

			bool Update()
			{
				var all = ctx.Guild!.Channels
					.Where(x => x.Value.Type == DiscordChannelType.Voice)
					.SelectMany(x => x.Value.Users)
					.ToList();

				foreach (var challenger in challengers.Keys)
				{
					challengers[challenger] = all.Contains(challenger);
				}

				return challengers.Values.All(x => x);
			}

			// Lese den Zeitpunkt der Anwesenheit aus
			DateTime time;
			{
				var arg_time = timeAsString.Split(":");

				time = DateTime.Today
					.AddHours(int.Parse(arg_time[0]))
					.AddMinutes(int.Parse(arg_time[1]));

				if (time < DateTime.Now.AddMinutes(1))
					time = time.AddDays(1);

			}

			// Command antworten
			await ctx.RespondAsync(
				$$$"""
				Herausforderung ist {{{Bold($"{(time.Day == DateTime.Today.Day ? "Heute" : "Morgen")} um {time:HH:mm}")}}} da zu sein {{{ctx.GetEmojis().PauseChamp}}}

				{{{Italic($"Um die Herausforderung anzunehmen, müsst Ihr auf diese Nachricht mit {ctx.GetEmojis().ThumbsUp} reagieren")}}}
				""");

			msg = (await ctx.GetResponseAsync())!;

			// Teilnahme starten
			msg.OnReactionAdded(async (sender, e) =>
			{
				if (msg.Author == e.User) return; // Der Bot selbst soll nicht mit aufgenommen werden
				if (challengers.ContainsKey(e.User)) return; // Ist bereits in der Challange

				if(challengers.TryAdd(e.User, false))
				{
					await Program.Client.ReconnectAsync();
					await msg.RespondAsync($"{e.User.Mention} ist dabei!");
				}				
			}, out var token);
			await msg.CreateReactionAsync(ctx.GetEmojis().ThumbsUp);
			
			// Bis zum Zeitpunkt abwarten...
			await Task.Delay(time - DateTime.Now);
			await Program.Client.ReconnectAsync();

			// Teilnahme beenden
			token.Cancel();

			if (challengers.Count == 0)
			{
				// Wenn es keine Teilnehmer gibt...
				// Einfach so tun, als ob nix wäre 
				return;
			}

			// Prüfe, ob alle da sind
			var allWon = Update();

			if (!allWon)
			{
				// Warte 5 Minuten 
				string text(int time) => $"""
					Anwesenheitscheck {ctx.GetEmojis().PauseChamp}...

					Verbleibende Zeit: {DiscordCountdown.GetText(time)}
					""";

				var msg2 = await msg.RespondAsync(text(wait));
				await msg2.CreateReactionAsync(ctx.GetEmojis().Chad);
				msg2 = await DiscordCountdown.Countdown(msg2!, wait, text, () => 
				{
					// Wenn alle da sind, muss nicht mehr gewartet werden
					allWon = Update();

					return allWon;
				});
				await msg2.DeleteAsync();

				if (!allWon)
                {
					// Wenn der Timer nicht vorher beendet wurde, weil alle da waren
					// dann muss nochmal die Anwesenheit geprüft werden
					allWon = Update();
                }
            }
			
			// Auswertung
			if (allWon)
			{
				await msg.RespondAsync($"Ihr habt es echt alle geschafft {ctx.GetEmojis().KEKWait}");
			}
			else if (challengers.Values.All(x => !x))
			{  
				await msg.RespondAsync($"Ihr seid alles scheiß losers.. \n\nWIE SCHAFFT ES NICHT EINER VON EUCH?!?! {ctx.GetEmojis().Madge}");
			}
			else
			{  
				await msg.RespondAsync(
					$"Geschafft: {string.Join(", ", challengers.Where(x => x.Value).Select(x => x.Key.Mention))} und natürlich ich selbst {ctx.GetEmojis().Chad}\n" +
					$"Loser: {string.Join(", ", challengers.Where(x => !x.Value).Select(x => x.Key.Mention))}... {ctx.GetEmojis().Smoking}");	
			}
		}
		else
		{
			await ctx.RespondAsync($"Was willst du eig. von mir... dafaq {ctx.GetEmojis().Madge}");
			return;
		};
	}

	[GeneratedRegex("^[0-2]?[0-9]:[0-5][0-9]$")]
	private static partial Regex TimeRegex();
}

public static class EmojisExtensions
{
	public static Emojis GetEmojis(this CommandContext ctx) => new()
	{
		CTX = ctx
	};
}

public class Emojis
{
	public required CommandContext CTX { get; init; }

	public DiscordEmoji ThumbsUp => DiscordEmoji.FromName(CTX.Client, ":thumbsup:");

	public DiscordEmoji Smoking => DiscordEmoji.FromName(CTX.Client, ":smoking:");

	public DiscordEmoji PauseChamp => CTX.Guild!.GetEmojiByName("PauseChamp") ?? DiscordEmoji.FromName(CTX.Client, ":see_no_evil:");

	public DiscordEmoji FeelsWeirdMan => CTX.Guild!.GetEmojiByName("FeelsWeirdMan") ?? DiscordEmoji.FromName(CTX.Client, ":clown:");

	public DiscordEmoji KEKWait => CTX.Guild!.GetEmojiByName("KEKWait") ?? DiscordEmoji.FromName(CTX.Client, ":open_mouth:");

	public DiscordEmoji Chad => CTX.Guild!.GetEmojiByName("chad") ?? DiscordEmoji.FromName(CTX.Client, ":ok_hand:");

	public DiscordEmoji Poggers => CTX.Guild!.GetEmojiByName("POGGERS") ?? DiscordEmoji.FromName(CTX.Client, ":smiley:");

	public DiscordEmoji Sadge => CTX.Guild!.GetEmojiByName("Sadge") ?? DiscordEmoji.FromName(CTX.Client, ":smiling_face_with_tear:");

	public DiscordEmoji Madge => CTX.Guild!.GetEmojiByName("Madge") ?? DiscordEmoji.FromName(CTX.Client, ":angry:");

	public DiscordEmoji PeepoWTF => CTX.Guild!.GetEmojiByName("peepoWtf") ?? DiscordEmoji.FromName(CTX.Client, ":rage:");
}