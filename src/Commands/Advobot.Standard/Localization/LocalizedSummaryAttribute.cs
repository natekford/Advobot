using System;
using Advobot.Localization;
using Advobot.Standard.Resources;

namespace Advobot.Standard.Localization
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class LocalizedSummaryAttribute : LocalizedSummaryBaseAttribute
	{
		public LocalizedSummaryAttribute(string name)
			: base(name, Summaries.ResourceManager) { }
	}
}
