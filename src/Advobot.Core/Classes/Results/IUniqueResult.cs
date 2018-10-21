using System;

namespace Advobot.Classes.Results
{
	/// <summary>
	/// A result which has a unique id.
	/// </summary>
	public interface IUniqueResult
	{
		/// <summary>
		/// The id of this result.
		/// </summary>
		Guid Guid { get; }
	}
}