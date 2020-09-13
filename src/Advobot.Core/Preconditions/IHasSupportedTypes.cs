using System;
using System.Collections.Generic;

namespace Advobot.Preconditions
{
	/// <summary>
	/// Indicates the precondition only supports specific types. (Generic attributes would be helpful)
	/// </summary>
	public interface IHasSupportedTypes
	{
		/// <summary>
		/// The types which are supported.
		/// </summary>
		IEnumerable<Type> SupportedTypes { get; }
	}
}