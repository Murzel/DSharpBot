using DSharpPlus.Entities;

namespace DSharpBot.Helper
{
	public static class DiscordGuildExtension
	{
		public static DiscordEmoji? GetEmojiByName(this DiscordGuild guild, string emojiName)
		{
			return guild.Emojis.Where(x => x.Value.Name == emojiName)
							   .Select(x => x.Value).FirstOrDefault(); ;
		}
	}
}
