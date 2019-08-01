using System;

namespace Advobot.Attributes
{
	/// <summary>
	/// Used for a localized summary.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class LocalizedSummaryBaseAttribute : Attribute
	{
		/// <summary>
		/// The name of the summary to use for localization.
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// The localized summary.
		/// </summary>
		public abstract string Summary { get; }

		/// <summary>
		/// Creates an instance of <see cref="LocalizedSummaryBaseAttribute"/>.
		/// </summary>
		/// <param name="name"></param>
		public LocalizedSummaryBaseAttribute(string name)
		{
			Name = name;
		}
	}
}
