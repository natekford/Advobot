using System;

using Advobot.Localization;

namespace Advobot.Standard.Localization
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class LocalizedCommandAttribute : LocalizedCommandBaseAttribute
	{
		public LocalizedCommandAttribute(string name)
			: base(name, Resources.Groups.ResourceManager) { }
	}
}