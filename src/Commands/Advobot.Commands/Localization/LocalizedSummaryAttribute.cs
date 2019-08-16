using System;
using System.Globalization;
using Advobot.Commands.Resources;
using Advobot.Localization;
using AdvorangesUtils;

namespace Advobot.Commands.Localization
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class LocalizedSummaryAttribute : LocalizedSummaryBaseAttribute
	{
		public LocalizedSummaryAttribute(string name)
			: base(name, Summaries.ResourceManager)
		{
			ConsoleUtils.DebugWrite($"Current culture: {CultureInfo.CurrentCulture}");
		}
	}
}
