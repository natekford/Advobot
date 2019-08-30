using System;

using Advobot.Localization;

namespace Advobot.Standard.Localization
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class LocalizedSummaryAttribute : LocalizedSummaryBaseAttribute
	{
		public LocalizedSummaryAttribute(string name)
			: base(name, Resources.Summaries.ResourceManager) { }
	}
}