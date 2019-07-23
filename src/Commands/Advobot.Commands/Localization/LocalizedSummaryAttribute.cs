using Discord.Commands;

namespace Advobot.CommandMarking.Localization
{
	public sealed class LocalizedSummaryAttribute : SummaryAttribute
	{
		public LocalizedSummaryAttribute(string resource) : base(strings.ResourceManager.GetString(resource)) { }
	}
}
