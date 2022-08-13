using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpBot.Helper;

namespace DSharpBot.Commands;

public class VoteCommands : BaseCommandModule
{
	[Command("ruined")]
	public async Task RuinedCommand(CommandContext ctx)
	{
		var weirdChamp = ctx.Guild.Emojis.Where(x => x.Value.Name == "weirdChamp")
										 .Select(x => x.Value).FirstOrDefault();
		var peepoWtf = ctx.Guild.Emojis.Where(x => x.Value.Name == "peepoWtf")
									   .Select(x => x.Value).FirstOrDefault();

		await ctx.Channel.SendMessageAsync($"\"Du hast mein ganzen Abend ruiniert! Wenn *du* nicht mein Premade wärst, würde ich **dich** reporten {weirdChamp}\" ~Jonas 2021");
	}

	[Command("ruined")]
	public async Task RuinedCommand(CommandContext ctx, DiscordMember member)
	{
		if (member.Id == Program.MyMemberId)
		{
			var kekwait = ctx.Guild.Emojis.Where(x => x.Value.Name == "KEKWait")
										  .Select(x => x.Value).FirstOrDefault();
			await ctx.Channel.SendMessageAsync($"Unmöglich {kekwait ?? ""}");
			return;
		}

		var weirdChamp = ctx.Guild.Emojis.Where(x => x.Value.Name == "weirdChamp")
										 .Select(x => x.Value).FirstOrDefault();
		var peepoWtf = ctx.Guild.Emojis.Where(x => x.Value.Name == "peepoWtf")
									   .Select(x => x.Value).FirstOrDefault();

		await ctx.Channel.SendMessageAsync($"*Du* hast mein ganzen Abend ruiniert **{member.Nickname}** {peepoWtf}\n\nWenn *du* nicht mein Premade wärst, würde ich **dich** reporten {weirdChamp}");
	}

	[Command("votekick")]
	public async Task TimeoutCommand(CommandContext ctx, DiscordMember member)
	{
		var time = 10; // seconds
		await ctx.RespondAsync($"User {member.Mention} has been timed out for {time} seconds");
		await member.TimeoutAsync(DateTimeOffset.Now.AddSeconds(time), "KEKWait");
	}

	[Command("ping")]
	public async Task PingCommand(CommandContext ctx)
	{
		await ctx.RespondAsync($"Bot is running since: **{Program.CreatedAt}**");
	}

	[Command("uptime")]
	public async Task UptimeCommand(CommandContext ctx)
	{
		await ctx.RespondAsync($"Uptime: **{(DateTimeOffset.Now - Program.CreatedAt).ToFormattedRelativeTime()}**");
	}
}
