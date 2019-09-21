using Discord;

namespace Advobot.Tests.Fakes.Discord
{
	public sealed class FakeVoiceRegion : IVoiceRegion
	{
		public string Id { get; set; }
		public bool IsCustom { get; set; }
		public bool IsDeprecated { get; set; }
		public bool IsOptimal { get; set; }
		public bool IsVip { get; set; }
		public string Name { get; set; }
	}
}