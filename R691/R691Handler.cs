using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DSharpBot.R691;

internal static class R691Handler
{
	public static SqliteConnection DbConnection = new("Data Source=R691.db");

	private static DbContextOptions<R691Context>? DbOptions;

	public static R691Context GetDbContext() => new(DbOptions!);

	public static async Task Init()
	{
		using var dbContext = new R691Context();

		dbContext.Database.EnsureCreated();

		DbOptions = new DbContextOptionsBuilder<R691Context>()
			.UseSqlite(DbConnection)
			.Options;

		await DbConnection.OpenAsync(Program.CancellationToken);
	}

	public const string TargetChannelName =
#if !DEBUG
		"691";
#else
		"terminal";
#endif

	private async static Task<bool> EvaluateBan(this DiscordMessage message, R691Context dbContext)
	{
		if (!message.ContainsMedia())
			return false;

		if (await message.Author!.IsBanned(dbContext))
		{
			await message.DeleteAsync("User is still banned...").ConfigureAwait(false);
		}
		else
		{
			await message.BanAuthor(dbContext);
		}

		return true;
	}

	public async static Task OnMessageCreated(DiscordClient sender, MessageCreatedEventArgs args)
	{
		using var dbContext = GetDbContext();

		if (args is { Author.IsCurrent: false, Channel.Name: TargetChannelName })
		{
			DiscordMessage msg = args.Message;

			if (await msg.EvaluateBan(dbContext))
				return;

			// Bei einem Link wird die Vorschau von Discord nicht direkt geladen
			// Daher wird hier nochmal die Nachricht verzögert geprüft
			for (int i = 0; i < 4; i++)
			{
				await Task.Delay(1_000);

				msg = await msg.Channel!.GetMessageAsync(args.Message.Id);

				if (await msg.EvaluateBan(dbContext))
					return;
			}
		}
	}

	#region Helper
	private static async Task<bool> IsBanned(this DiscordUser user, R691Context dbContext)
	{
		return await dbContext.Banned
			.Where(Banned => Banned.Id == user.Id)
			.AnyAsync();
	}

	private static bool ContainsMedia(this DiscordMessage msg) => msg
		is { Attachments.Count: > 0 }
		or { Embeds: [.., { Type: "image" }] }
		or
		{
			MessageSnapshots: [..,
				{ Message.Attachments.Count: > 0 }
			 or { Message.Embeds.Count: > 0 }]
		};

	private static async Task<bool> BanAuthor(this DiscordMessage msg, R691Context dbContext)
	{
		DateTime until;
		string timeWithUnit;

		if (msg.Author is null)
			return false;

#if !DEBUG
		var hours = Random.Shared.Next(12, 7 /* days */ * 24 + 1);
		var days = hours / 24;

		hours %= 24;

		until = DateTime.UtcNow.AddHours(hours);

		timeWithUnit = days switch
		{
			0 => "",
			1 => "1 day",
			_ => $"{days} days"
		};

		if (timeWithUnit != string.Empty && hours > 0)
		{
			timeWithUnit += " and ";
		}

		timeWithUnit += hours switch
		{
			0 => "",
			1 => $"1 hour",
			_ => $"{hours} hours"
		};
#else
		var seconds = 10;

		until = DateTime.UtcNow.AddSeconds(seconds);
		timeWithUnit = $"{seconds} seconds";
#endif

		await dbContext.Banned.AddAsync(new()
		{
			Id = msg.Author.Id,
			Until = until
		});
		await dbContext.SaveChangesAsync();

		await msg.RespondAsync($"For making this post, this user was banned for {timeWithUnit}");
		return true;
	}
	#endregion

	public static async Task Handle691Unban(this DiscordClient client)
	{
		while (true)
		{
			if (Program.CancellationToken.IsCancellationRequested)
				return;

			using var dbContext = GetDbContext();

			var users = await dbContext.Banned
				.Where(x => x.Until < DateTime.UtcNow)
				.ToListAsync();

			foreach (var user in users)
			{
				var dUser = await client.GetUserAsync(user.Id);

				var res = await dbContext.Banned
					.Where(x => x.Id == user.Id)
					.ExecuteDeleteAsync();

				if (res > 0) await dUser
					.SendMessageAsync("You're now unbanned from 691 and can submit stupid posts again 🚬")
					.ConfigureAwait(false);
			}

			await Task.Delay(10_000, Program.CancellationToken);
		}
	}
}
