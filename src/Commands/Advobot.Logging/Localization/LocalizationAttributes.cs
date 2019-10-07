using System;

using Advobot.Localization;

namespace Advobot.Logging.Localization
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class LocalizedAliasAttribute : LocalizedAliasBaseAttribute
	{
		public LocalizedAliasAttribute(params string[] names)
			: base(names, Resources.Aliases.ResourceManager) { }
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class LocalizedCommandAttribute : LocalizedCommandBaseAttribute
	{
		public LocalizedCommandAttribute(string name)
			: base(name, Resources.Groups.ResourceManager) { }
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class LocalizedGroupAttribute : LocalizedGroupBaseAttribute
	{
		public LocalizedGroupAttribute(string name)
			: base(name, Resources.Groups.ResourceManager) { }
	}

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class LocalizedNameAttribute : LocalizedNameBaseAttribute
	{
		public LocalizedNameAttribute(string name)
			: base(name, Resources.Names.ResourceManager) { }
	}

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class LocalizedSummaryAttribute : LocalizedSummaryBaseAttribute
	{
		public LocalizedSummaryAttribute(string name)
			: base(name, Resources.Summaries.ResourceManager) { }
	}
}