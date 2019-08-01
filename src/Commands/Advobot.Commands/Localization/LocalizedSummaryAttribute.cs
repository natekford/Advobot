using System;
using System.Globalization;
using Advobot.Commands.Resources;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Commands.Localization
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class LocalizedSummaryAttribute : SummaryAttribute
	{
		public LocalizedSummaryAttribute(string name)
			: base(Summaries.ResourceManager.GetString(name))
		{
			ConsoleUtils.DebugWrite($"Current culture: {CultureInfo.CurrentCulture}");
		}
	}
}
