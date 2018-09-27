using System;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Specifies what type this type reader targets.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public class TypeReaderTargetTypeAttribute : Attribute
	{
		/// <summary>
		/// The type this type reader targets.
		/// </summary>
		public Type TargetType { get; }

		/// <summary>
		/// Creates an instance of <see cref="TypeReaderTargetTypeAttribute"/>.
		/// </summary>
		/// <param name="type"></param>
		public TypeReaderTargetTypeAttribute(Type type)
		{
			TargetType = type;
		}
	}
}
