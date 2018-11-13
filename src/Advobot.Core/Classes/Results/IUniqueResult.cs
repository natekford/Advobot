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
		/// <summary>
		/// Whether this has already been logged.
		/// </summary>
		bool AlreadyLogged { get; }

		/// <summary>
		/// Sets <see cref="AlreadyLogged"/> to true.
		/// </summary>
		void MarkAsLogged();
	}
}