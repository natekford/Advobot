using System;

namespace Advobot.Attributes
{
	/// <summary>
	/// Indicates the category a command belongs to.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class CategoryAttribute : Attribute
	{
		/// <summary>
		/// The command category commands belong to.
		/// </summary>
		public string Category { get; }

		/// <summary>
		/// Creates an instance of <see cref="CategoryAttribute"/>.
		/// </summary>
		/// <param name="category"></param>
		public CategoryAttribute(string category)
		{
			Category = category;
		}
	}
}