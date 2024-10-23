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
		
		await ctx.RespondAsync(
			$"""
			{Italic("Du")} hast mein ganzen Abend ruiniert {(ruiner is not null ? Bold(ruiner.Nickname) : "")} {ctx.GetEmojis().PeepoWTF}

			Wenn {Italic("du")} nicht mein Premade wärst, würde ich {Bold("dich")} reporten {ctx.GetEmojis().FeelsWeirdMan} ~Jonas 2021
			"""
		);
	}

	[Command("uptime"), AllowedProcessors<SlashCommandProcessor>(), Description("Check out how long the bot is running")]
	public static async Task UptimeCommand(CommandContext ctx)
	{
		await ctx.RespondAsync($"Since: {Bold(Program.CreatedAt.ToString())}\nUptime: {Bold((DateTimeOffset.Now - Program.CreatedAt).ToFormattedRelativeTime())}");
	}

	[Command("countdown"), Description("Zählt eine Zeit runter")]
	public static async Task CountdownCommand(CommandContext ctx, [Parameter("Minuten")] int minutes)
	{
		if (minutes > 15)
		{
			await ctx.RespondAsync($"Du brauchst Lord Plus, um einen längeren als 15 Minuten Countdown zu machen!");
			return;
		}

		if (minutes < 0)
			return;

		await DiscordCountdown.Countdown(ctx, minutes);
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
			DateTime time;
			{
				var arg_time = timeAsString.Split(":");

				time = DateTime.Today
					.AddHours(int.Parse(arg_time[0]))
					.AddMinutes(int.Parse(arg_time[1]));

				if (time < DateTime.Now.AddMinutes(9))
					time = time.AddDays(1);

			}

			await ctx.RespondAsync(
				$$$"""
				Herausforderung ist {{{Bold($"{(time.Day == DateTime.Today.Day ? "Heute" : "Morgen")} um {time:HH:mm}")}}} da zu sein {{{ctx.GetEmojis().PauseChamp}}}

				{{{Italic($"Um die Herausforderung anzunehmen, müsst Ihr innerhalb von {wait} Minuten auf diese Nachricht mit {ctx.GetEmojis().ThumbsUp} reagieren")}}}
				""");

			msg = (await ctx.GetResponseAsync())!;
			
			await msg.CreateReactionAsync(ctx.GetEmojis().ThumbsUp);
			await (await DiscordCountdown.Countdown(await ctx.FollowupAsync(DiscordCountdown.GetInitText(wait)), wait))
				.DeleteAsync();

			var challengers = msg
				.GetReactionsAsync(ctx.GetEmojis().ThumbsUp)
				.ToBlockingEnumerable()
				.Where(x => x.Id != msg.Author!.Id) // Den TheLord-Bot rausfiltern
				.ToDictionary(x => x, x => false);

			if (challengers.Count == 0)
			{
				await ctx.FollowupAsync($"Niemand hat die Herausforderung angeommen... {ctx.GetEmojis().KEKWait}");
				return;
			}

			await ctx.FollowupAsync($"Teilnehmende: {string.Join(", ", challengers.Keys.Select(x => x.Mention))} und ich bin auch dabei {ctx.GetEmojis().Smoking}");
			await Task.Delay(time - DateTime.Now);
			
			msg = await ctx.FollowupAsync($"Anwesenheitscheck {ctx.GetEmojis().PauseChamp}...\n\n{Italic($"Reagiere auf diese Nachricht mit {ctx.GetEmojis().Chad}!")}");

			await msg.CreateReactionAsync(ctx.GetEmojis().Chad);
			IReadOnlyList<DiscordUser> winners = [];

			await DiscordCountdown.Countdown(ctx, 5, () =>
			{
				foreach (DiscordUser reactor in msg.GetReactionsAsync(ctx.GetEmojis().Chad).ToBlockingEnumerable().Intersect(challengers.Keys))
				{
					challengers[reactor] = true;
				}

				return challengers.Values.All(x => x);
			});

			if (challengers.Values.All(x => x))
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
					$"Geschafft: {string.Join(", ", challengers.Where(x => x.Value).Select(x => x.Key.Mention))} und natürlich ich selbst lol {ctx.GetEmojis().Smoking}\n" +
					$"Loser: {string.Join(", ", challengers.Where(x => !x.Value).Select(x => x.Key.Mention))}");	
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