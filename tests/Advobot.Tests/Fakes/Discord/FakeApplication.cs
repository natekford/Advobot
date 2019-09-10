using System;

using Advobot.Tests.Fakes.Discord.Users;

using Discord;

namespace Advobot.Tests.Fakes.Discord
{
	public sealed class FakeApplication : FakeSnowflake, IApplication
	{
		public string Description { get; set; } = "This is a fake application.";
		public ulong Flags { get; set; } = 0;
		public string IconUrl { get; set; } = "";
		public string Name { get; set; } = "FakeBot";
		public IUser Owner { get; set; } = new FakeUser();
		public string[] RPCOrigins { get; set; } = Array.Empty<string>();
	}
}