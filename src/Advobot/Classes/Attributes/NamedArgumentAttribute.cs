using System;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Only use on primitives (nullable allowed) or enums.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class NamedArgumentAttribute : Attribute
	{
		/// <summary>
		/// Specifies the acceptable amount of objects in the params array.
		/// </summary>
		public int Length { get; }

		/// <summary>
		/// Creates an instance of <see cref="NamedArgumentAttribute"/>.
		/// </summary>
		/// <param name="length"></param>
		public NamedArgumentAttribute(int length = 0)
		{
			Length = length;
		}
	}
}
