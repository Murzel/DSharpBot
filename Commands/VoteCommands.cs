using DSharpBot.Helper;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Text.RegularExpressions;
using static DSharpPlus.Formatter;

namespace DSharpBot.Commands;

public class VoteCommands : BaseCommandModule
{
	[Command("ruined")]
	public async Task RuinedCommand(CommandContext ctx)
	{
		var weirdChamp = ctx.Guild.GetEmojiByName("weirdChamp");

		await ctx.Channel.SendMessageAsync($"\"Du hast mein ganzen Abend ruiniert! Wenn *du* nicht mein Premade wärst, würde ich **dich** reporten {weirdChamp}\" ~Jonas 2021");
	}

	[Command("ruined"), Description("Do a statement and make clear the fucker ruined your day")]
	public async Task RuinedCommand(CommandContext ctx, DiscordMember member)
	{
		if (member.Id == Program.Config.Bot.Id)
		{
			var kekwait = ctx.Guild.GetEmojiByName("KEKWait");
			await ctx.Channel.SendMessageAsync($"Unmöglich {kekwait ?? ""}");
			return;
		}

		var weirdChamp = ctx.Guild.GetEmojiByName("weirdChamp");
		var peepoWtf = ctx.Guild.GetEmojiByName("peepoWtf");

		await ctx.Channel.SendMessageAsync($"{Italic("Du")} hast mein ganzen Abend ruiniert {Bold(Sanitize(member.Nickname))} {peepoWtf}\n\nWenn {Italic("du")} nicht mein Premade wärst, würde ich {Bold("dich")} reporten {weirdChamp}");
	}

	[Command("rr"), Description("Russian Roulette. Test your luck :)")]
	public async Task RrCommand(CommandContext ctx)
	{
		var rand = Random.Shared.Next(1, 100);

		var kekw = ctx.Guild.GetEmojiByName("KEKW");
		var PauseChamp = ctx.Guild.GetEmojiByName("PauseChamp");

		if (rand < 11)
			await ctx.RespondAsync($"Du wärst gestorben {kekw}");
		else
			await ctx.RespondAsync($"Du hast überlebt {PauseChamp}...");
	}

	[Command("tuhh"), Description("The truth hurts sometimes...")]
	public async Task TuhhCommand(CommandContext ctx)
	{
		var weirdChamp = ctx.Guild.GetEmojiByName("weirdChamp");
		await ctx.RespondAsync($"Tuhh ist Schmutz {weirdChamp}");
	}

	[Command("random"), Description("Get a random item chosen of the list")]
	[Aliases("rand")]
	public async Task RandomCommand(CommandContext ctx, params string[] liste)
	{
		if (!liste.Any())
		{
			var weirdChamp = ctx.Guild.GetEmojiByName("weirdChamp");

			await ctx.RespondAsync($"Du musst schon Elemente angeben, damit ich ein zufälliges davon bestimmen kann {Bold("du")} Pappnase {weirdChamp}");
			return;
		}

		for (int i = 0; i < liste.Length; i++)
		{
			if (liste[i] == Program.Config.Bot.Mention)
			{
				// If the bot is an element, the bot has to be chosen,
				// because he is in fact the chosen one, so no other can be...
				liste = new string[] { liste[i] };
				break;
			}
			else if (new Regex(@"^-?[0-9]{1,9}?--?[0-9]{1,9}$").IsMatch(liste[i]))
			{
				// If two numbers seperated by a dash, get a random number between those.
				// A number can have at max 9 digits, so it can be parsed as an integer val.
				#region logik

				char[] current = liste[i].Take(1).
					  Concat(liste[i].Skip(1)
									 .TakeWhile(str => str != '-'))
					 .ToArray();
				int leftNumber = int.Parse(current);

				current = liste[i].Skip(current.Length + 1).ToArray();

				int rightNumber = int.Parse(current);

				if (leftNumber < rightNumber)
				{
					liste[i] = Random.Shared.Next(leftNumber, rightNumber + 1).ToString();
				}
				else
				{
					liste[i] = Random.Shared.Next(rightNumber, leftNumber + 1).ToString();
				}

				#endregion
			}
		}

		var pos = new Random().Next(0, liste.Length);

		await ctx.RespondAsync($"Ich wähle {Bold(liste[pos])}");
	}

	[Command("votekick"), Description("Time outs a member for 5 minutes after a democratic referendum")]
	public async Task TimeoutCommand(CommandContext ctx, DiscordMember member)
	{
		var time = 10; // seconds
		await ctx.RespondAsync($"User {member.Mention} has been timed out for {time} seconds");
		await member.TimeoutAsync(DateTimeOffset.Now.AddSeconds(time), "KEKWait");
	}

	[Command("ping"), Description("Check out how long the bot needs to responds")]
	public async Task PingCommand(CommandContext ctx)
	{
		await ctx.RespondAsync($"Message received and replied at {Bold(DateTimeOffset.Now.ToString())}");
	}

	[Command("uptime"), Description("Check out how long the bot is running.")]
	public async Task UptimeCommand(CommandContext ctx)
	{
		await ctx.RespondAsync($"Since: {Bold(Program.CreatedAt.ToString())}\nUptime: {Bold((DateTimeOffset.Now - Program.CreatedAt).ToFormattedRelativeTime())}");
	}

	[Command("spam")]
	public async Task SpamCommand(CommandContext ctx, DiscordMember member)
	{
		await SpamCommand(ctx, member, 5);
	}

	[Command("spam")]
	public async Task SpamCommand(CommandContext ctx, DiscordMember member, int count)
	{
		if (count > 10)
		{
			await ctx.RespondAsync($"Du brauchst Lord Plus, um mehr als 10 mal zu spammen.");
			return;
		}

		for (int i = 0; i < count; i++)
		{
			await ctx.Channel.SendMessageAsync($"Komm endlich {member.Mention} lol {Random.Shared.Next(1200, 9999)}");
			await Task.Delay(1000);
		}
	}

	[Command("countdown")]
	public async Task CountdownCommand(CommandContext ctx, int minutes)
	{
		if (minutes > 60)
		{
			await ctx.RespondAsync($"Du brauchst Lord Plus, um einen längeren als 60 Minuten Countdown zu machen!");
			return;
		}

		if(minutes < 0)
			return;
		
		DiscordCountdown.Countdown(ctx, minutes);
	}

	[Command("anwesenheit")]
	public async Task AttendanceCheckCommand(CommandContext ctx, params DiscordMember[] challengee)
	{
		await AttendanceCheckCommand(ctx, "jetzt", challengee);
	}

	[Command("anwesenheit")]
	public async Task AttendanceCheckCommand(CommandContext ctx, string timeAsString, params DiscordMember[] challengee)
	{
		if (challengee.Length > 25)
		{ // Mehr als 5 Minuten
			await ctx.RespondAsync($"Du brauchst Lord Plus, um mehr als 25 Leute herausfordern!");
			return;
		}

		var challengee_ids = challengee.Select(x => x.Id).ToArray();
		ulong[] challengers_ids = Array.Empty<ulong>();
		DiscordUser[] challengers = Array.Empty<DiscordUser>();
		DiscordUser[] winners = Array.Empty<DiscordUser>();
		DiscordUser[] losers = Array.Empty<DiscordUser>();

		#region Emojies
		var thumbsup = DiscordEmoji.FromName(ctx.Client, ":thumbsup:");
		var pauseChamp = ctx.Guild.GetEmojiByName("PauseChamp") ?? DiscordEmoji.FromName(ctx.Client, ":see_no_evil:");
		var weirdChamp = ctx.Guild.GetEmojiByName("weirdChamp") ?? DiscordEmoji.FromName(ctx.Client, ":clown:");
		var kekwait = ctx.Guild.GetEmojiByName("KEKWait") ?? DiscordEmoji.FromName(ctx.Client, ":open_mouth:");
		var chad = ctx.Guild.GetEmojiByName("chad") ?? DiscordEmoji.FromName(ctx.Client, ":ok_hand:");
		var poggers = ctx.Guild.GetEmojiByName("POGGERS") ?? DiscordEmoji.FromName(ctx.Client, ":smiley:");
		var sadge = ctx.Guild.GetEmojiByName("Sadge") ?? DiscordEmoji.FromName(ctx.Client, ":smiling_face_with_tear:");
		#endregion

		DiscordMessage? announcement_msg = null;

		timeAsString = timeAsString.ToLower();

		if (timeAsString == "jetzt") 
		{
			challengers = challengee;
			challengers_ids = challengers.Select(x => x.Id)
										 .ToArray();
		}
		else if (Regex.IsMatch(timeAsString, "^[0-2]?[0-9]:[0-5][0-9]$")) 
		{
			var time = ((Func<DateTime>)(() =>
			{
				var arg_time = timeAsString.Split(":");

				var result = DateTime.Today.AddHours(int.Parse(arg_time[0]))
										   .AddMinutes(int.Parse(arg_time[1]));

				if (result < DateTime.Now.AddMinutes(9))
					result = result.AddDays(1);

				return result;
			}))();

			announcement_msg = await ctx.Channel.SendMessageAsync($"Ihr wurdet zu der LOL-Challenge eingeladen: " +
				string.Join(", ", challengee.Select(x => x.Mention)) + $" {pauseChamp}...\n\n" +
				$"Herausforderung ist {Bold($"{(time.Day == DateTime.Today.Day ? "Heute" : "Morgen")} um {time:HH:mm}")} online zu sein.\n\n" +
				$"{Italic($"Um die Herausforderung anzunehmen, müsst Ihr innerhalb von 5 Minuten auf diese Nachricht mit {thumbsup} reagieren")}");

			await announcement_msg.CreateReactionAsync(thumbsup);

			DiscordCountdown.Countdown(ctx, 5, () =>
			{
				challengers = announcement_msg.GetReactionsAsync(thumbsup).Result
												.Where(x => challengee_ids.Contains(x.Id))
												.ToArray();

				return challengers.Length == challengee_ids.Length;
			});

			if (!challengers.Any())
			{
				await announcement_msg.RespondAsync("Niemand hat die Herausforderung angeommen...");
				return;
			}
			
			await announcement_msg.RespondAsync($"Teilnehmende: {string.Join(", ", challengers.Select(x => x.Mention))}\n\n");
			challengers_ids = challengers.Select(x => x.Id).ToArray();

			Task.Delay(time - DateTime.Now).Wait();
		}
		else return;

		DiscordMessage? attendanceCheck_msg;

		if (announcement_msg is null)
			attendanceCheck_msg = await ctx.Channel.SendMessageAsync($"Anwesenheitscheck {pauseChamp}...\n\n{Italic($"Reagiere auf diese Nachricht mit {chad}!")}");
		else
			attendanceCheck_msg = await announcement_msg.RespondAsync($"Anwesenheitscheck {pauseChamp}...\n\n{Italic($"Reagiere auf diese Nachricht mit {chad}!")}");
		
		await attendanceCheck_msg.CreateReactionAsync(chad);
		DiscordCountdown.Countdown(ctx, 5, () =>
		{
			winners = attendanceCheck_msg.GetReactionsAsync(chad).Result
										 .Where(x => challengers_ids.Contains(x.Id))
										 .ToArray();

			return winners.Length == challengers_ids.Length;
		});

		losers = challengers.Where(x => !winners.Select(y => y.Id).Contains(x.Id)).ToArray();

		if (winners.Length == 0)
		{
			if (announcement_msg is not null)
				await ctx.RespondAsync($"Ihr seid alles scheiß losers.. \n\nWIE SCHAFFT ES NICHT EINER VON EUCH?!?! {weirdChamp}");
			else
				await ctx.RespondAsync($"Echt keiner da? {weirdChamp}");
		}
		else if (winners.Length == challengers_ids.Length)
		{
			if (announcement_msg is not null)
				await ctx.RespondAsync($"Ihr habt es echt alle geschafft {kekwait}");
			else
				await ctx.RespondAsync($"Alle da {poggers}");
		}
		else
		{
			if (announcement_msg is not null)
				await ctx.RespondAsync($"Geschafft: {string.Join(", ", winners.Select(x => x.Mention))}\n" +
					$"Loser: { string.Join(", ", losers.Select(x => x.Mention))}");
			else
				await ctx.RespondAsync($"Nice {poggers}: {string.Join(", ", winners.Select(x => x.Mention))}\n" +
					$"Wo seid ihr {sadge}: {string.Join(", ", losers.Select(x => x.Mention))}");
		}
	}
}
