using Advobot.Commands.Resources;
using Discord.Commands;

namespace Advobot.Commands.Localization
{
	public sealed class LocalizedSummaryAttribute : SummaryAttribute
	{
		public LocalizedSummaryAttribute(string resource)
			: base(strings.ResourceManager.GetString(resource)) { }
	}
}
