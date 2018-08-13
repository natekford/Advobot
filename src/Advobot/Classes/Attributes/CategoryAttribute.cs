using System;
using System.Linq;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Specifies the category this module belongs to.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class CategoryAttribute : Attribute
	{
		/// <summary>
		/// The category this module belongs to.
		/// </summary>
		public string Category { get; }

		/// <summary>
		/// Creates an instance of <see cref="CategoryAttribute"/>.
		/// </summary>
		/// <param name="type"></param>
		public CategoryAttribute(Type type)
		{
			Category = type.Name.Split('.').Last();
		}
		/// <summary>
		/// Creates an instance of <see cref="CategoryAttribute"/>.
		/// </summary>
		/// <param name="name"></param>
		public CategoryAttribute(string name)
		{
			Category = name;
		}
	}
}