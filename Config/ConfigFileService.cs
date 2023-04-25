using System.Text.Json;

namespace DSharpBot.Config
{
	public static class ConfigFileService
	{
		public const string FILENAME = "Config.json";

		public static string Directory { get; } = Path.GetDirectoryName(Environment.ProcessPath) ?? Environment.CurrentDirectory;

		public static string FullPath { get; } = Path.Join(Directory, FILENAME);

		public static Configuration Config { 
			get 
			{
				Configuration result;
				string configJson;

				if (!File.Exists(FullPath))
					throw new Exception($"Configuration file \"{FILENAME}\" in \"{Directory}\" not found!");

				configJson = File.ReadAllText(FullPath);

				try
				{
					result = JsonSerializer.Deserialize<Configuration>(configJson) ?? throw new Exception($"Couldn't read the configuration file \"{FILENAME}\" in \"{Directory}\".");
				}
				catch (Exception ex)
				{
					throw new Exception($"Couldn't read the configuration file \"{FILENAME}\" in \"{Directory}\".", ex);
				}

				return result;
			} 
		}
	}
}
