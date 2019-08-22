using System.Collections.Generic;
using Advobot.Services.HelpEntries;

namespace Advobot.Tests.Fakes.Services.HelpEntries
{
	public sealed class FakeHelpEntry : IModuleHelpEntry
	{
		public bool AbleToBeToggled { get; set; }
		public bool EnabledByDefault { get; set; }
		public string Id { get; set; }
		public string? Category { get; set; }
		public IReadOnlyList<string> Aliases { get; set; }
		public IReadOnlyList<IPrecondition> Preconditions { get; set; }
		public IReadOnlyList<ICommandHelpEntry> Commands { get; set; }
		public string Name { get; set; }
		public string Summary { get; set; }
	}
}
