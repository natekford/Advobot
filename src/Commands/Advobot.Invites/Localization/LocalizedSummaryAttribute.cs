using System;

using Advobot.Invites.Resources;
using Advobot.Localization;

namespace Advobot.Invites.Localization
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class LocalizedSummaryAttribute : LocalizedSummaryBaseAttribute
	{
		public LocalizedSummaryAttribute(string name)
			: base(name, Summaries.ResourceManager) { }
	}
}