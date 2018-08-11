using System;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Indicates that the targetted module requires the specified type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public sealed class RequiredServiceAttribute : Attribute
	{
		/// <summary>
		/// The type of service this module requires.
		/// </summary>
		public Type ServiceType { get; }

		/// <summary>
		/// Creates an instance of <see cref="RequiredServiceAttribute"/>.
		/// </summary>
		/// <param name="serviceType"></param>
		public RequiredServiceAttribute(Type serviceType)
		{
			ServiceType = serviceType;
		}
	}
}