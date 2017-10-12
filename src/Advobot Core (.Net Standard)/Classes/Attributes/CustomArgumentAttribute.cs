using System;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Only use on primitive non nullable types.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class CustomArgumentAttribute : Attribute
	{
		/// <summary>
		/// Specifies the acceptable amount of objects in the params array.
		/// </summary>
		public readonly int Length;

		/// <summary>
		/// <paramref name="length"/>is used for params arguments.
		/// </summary>
		/// <param name="length"></param>
		public CustomArgumentAttribute(int length = 0)
		{
			Length = length;
		}
	}
}
