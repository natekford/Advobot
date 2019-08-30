using System;

using Advobot.Localization;

namespace Advobot.Standard.Localization
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class LocalizedAliasAttribute : LocalizedAliasBaseAttribute
	{
		public LocalizedAliasAttribute(params string[] names)
			: base(names, Resources.Aliases.ResourceManager) { }
	}
}