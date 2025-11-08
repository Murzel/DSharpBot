using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace DSharpBot.NoClient;

internal static class NoService
{
	private static HttpClient Client { get; set; } = new HttpClient(
		new SocketsHttpHandler()
		{
			PooledConnectionLifetime = TimeSpan.FromMinutes(5)
		})
	{
		BaseAddress = new Uri("https://naas.isalman.dev/")
	};

	private class NoDto
	{
		[JsonPropertyName("reason")]
		public string Reason { get; set; } = string.Empty;
	}

	public static async Task<string> GetNoReasonAsync()
	{
		try
		{
			var no = await Client.GetFromJsonAsync<NoDto>("no");

			return no!.Reason;
		}
		catch (Exception)
		{
			return "Ähm ja ne, nö, nö, nöööööö. nope";
		}
	}
}
