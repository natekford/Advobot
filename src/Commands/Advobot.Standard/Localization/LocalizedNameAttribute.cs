using System;

using Advobot.Localization;

namespace Advobot.Standard.Localization
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class LocalizedNameAttribute : LocalizedNameBaseAttribute
	{
		public LocalizedNameAttribute(string name)
			: base(name, Resources.Names.ResourceManager) { }
	}
}