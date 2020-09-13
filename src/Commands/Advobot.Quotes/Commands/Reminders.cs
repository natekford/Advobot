using System;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Modules;
using Advobot.Quotes.Localization;
using Advobot.Quotes.ParameterPreconditions;
using Advobot.Quotes.Resources;

using Discord.Commands;

using static Advobot.Quotes.Responses.Reminders;

namespace Advobot.Quotes.Commands
{
	[Category(nameof(Reminders))]
	public sealed class Reminders : ModuleBase
	{
		[LocalizedGroup(nameof(Groups.RemindMe))]
		[LocalizedAlias(nameof(Aliases.RemindMe))]
		[LocalizedSummary(nameof(Summaries.RemindMe))]
		[Meta("3cedf19e-7a4d-47c0-ac2f-1c39a92026ec", IsEnabled = true)]
		public sealed class RemindMe : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command(
				[RemindTime]
				int minutes,
				[Remainder]
				string message)
			{
				var time = TimeSpan.FromMinutes(minutes);
				//TODO: actually implement
				//Timers.Add(new TimedMessage(time, Context.User, message));
				return AddedRemind(minutes);
			}
		}
	}
}