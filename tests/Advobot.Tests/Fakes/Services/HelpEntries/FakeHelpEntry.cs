using System;
using System.Collections.Generic;
using Advobot.Services.GuildSettings;
using Advobot.Services.HelpEntries;
using Discord.Commands;

namespace Advobot.Tests.Fakes.Services.HelpEntries
{
	public sealed class FakeHelpEntry : IHelpEntry
	{
		public bool AbleToBeToggled { get; set; }
		public bool DefaultEnabled { get; set; }
		public string Id { get; set; }
		public string Summary { get; set; }
		public string? Category { get; set; }
		public IReadOnlyCollection<string> Aliases { get; set; }
		public IReadOnlyCollection<PreconditionAttribute> BasePerms { get; set; }
		public string Name { get; set; }

		public string ToString(IGuildSettings? settings, IFormatProvider? formatProvider)
			=> throw new NotImplementedException();
		public string ToString(IGuildSettings? settings, IFormatProvider? formatProvider, int commandIndex)
			=> throw new NotImplementedException();
		public string ToString(string format, IFormatProvider formatProvider)
			=> throw new NotImplementedException();
	}
}
