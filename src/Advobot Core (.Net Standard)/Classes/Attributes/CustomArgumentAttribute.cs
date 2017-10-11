using System;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Only use on primitive non nullable types.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class CustomArgumentAttribute : Attribute
	{
		public CustomArgumentAttribute() { }
	}
}
