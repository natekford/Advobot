using System.Collections.Generic;
using Advobot.Interfaces;

namespace Advobot.Classes.EqualityComparers
{
	/// <summary>
	/// Compares two <see cref="ITargetsUser"/> and checks if they have the same user id.
	/// </summary>
	public class TargetsUserEqualityComparer : IEqualityComparer<ITargetsUser>
	{
		/// <summary>
		/// Default instance of this equality comparer.
		/// </summary>
		public static TargetsUserEqualityComparer Default { get; } = new TargetsUserEqualityComparer();

		/// <inheritdoc />
		public bool Equals(ITargetsUser x, ITargetsUser y) => x?.UserId == y?.UserId;
		/// <inheritdoc />
		public int GetHashCode(ITargetsUser obj) => obj.GetHashCode();
	}
}