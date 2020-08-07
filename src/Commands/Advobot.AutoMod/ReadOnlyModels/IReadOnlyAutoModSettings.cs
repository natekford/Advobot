using System;
using System.Threading.Tasks;

using Advobot.Databases.Relationships;

using Discord;

namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlyAutoModSettings : IGuildChild
	{
		bool CheckDuration { get; }
		TimeSpan Duration { get; }
		bool IgnoreAdmins { get; }
		bool IgnoreHigherHierarchy { get; }

		ValueTask<bool> ShouldScanMessageAsync(IMessage message, TimeSpan ts);
	}
}