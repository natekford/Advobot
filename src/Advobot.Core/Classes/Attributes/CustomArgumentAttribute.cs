using System;

namespace Advobot.Core.Classes.Attributes
{
	/// <summary>
	/// Only use on primitives (nullable allowed) or enums.
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
			this.Length = length;
		}
	}
}
