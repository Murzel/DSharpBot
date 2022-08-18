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

	[Command("ruined")]
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

	[Command("rr")]
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

	[Command("random")]
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
			else if (new Regex(@"^-?[0-9]{1,9}?-?-?[0-9]{1,9}$").IsMatch(liste[i]))
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

	[Command("ping")]
	public async Task PingCommand(CommandContext ctx)
	{
		await ctx.RespondAsync($"Bot is running since: {Bold(Program.CreatedAt.ToString())}");
	}

	[Command("uptime")]
	public async Task UptimeCommand(CommandContext ctx)
	{
		await ctx.RespondAsync($"Uptime: {Bold((DateTimeOffset.Now - Program.CreatedAt).ToFormattedRelativeTime())}");
	}
}
