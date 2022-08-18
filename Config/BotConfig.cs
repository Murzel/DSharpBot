using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpBot.Config
{
	public class BotConfig
	{
		public ulong Id { get; init; }
		public string Mention { get => $"<@{Id}>"; }

		public string Token { get; init; }
	}
}
