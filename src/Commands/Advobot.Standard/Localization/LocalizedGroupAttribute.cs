using System;

using Advobot.Localization;

namespace Advobot.Standard.Localization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class LocalizedGroupAttribute : LocalizedGroupBaseAttribute
	{
		public LocalizedGroupAttribute(string name)
			: base(name, Resources.Groups.ResourceManager) { }
	}
}