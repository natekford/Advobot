using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot
{
	public class Program
	{
		//Convert sync main to an async main.
		public static void Main(string[] args) =>
			new Program().Start().GetAwaiter().GetResult();

		private DiscordSocketClient client;
		private CommandHandler handler;

		public async Task Start()
		{
			//Define the DiscordSocketClient
			client = new DiscordSocketClient(new DiscordSocketConfig());

			client.Log += Log;

			//Login and connect to Discord.
			await client.LoginAsync(TokenType.Bot, "key");
			await client.ConnectAsync();

			var map = new DependencyMap();
			map.Add(client);

			handler = new CommandHandler();
			await handler.Install(map);

			//Block this program until it is closed.
			await Task.Delay(-1);
		}

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
	}
}
