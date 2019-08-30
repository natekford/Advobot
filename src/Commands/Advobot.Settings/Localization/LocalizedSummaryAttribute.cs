using System;

using Advobot.Localization;
using Advobot.Settings.Resources;

namespace Advobot.Settings.Localization
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class LocalizedSummaryAttribute : LocalizedSummaryBaseAttribute
	{
		public LocalizedSummaryAttribute(string name)
			: base(name, Summaries.ResourceManager) { }
	}
}