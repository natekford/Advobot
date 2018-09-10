using System.Collections.Generic;
using Advobot.Interfaces;

namespace Advobot.Classes.EqualityComparers
{
	/// <summary>
	/// Compares two <see cref="INameable"/> and checks if they have the same name.
	/// </summary>
	public class NameableEqualityComparer : IEqualityComparer<INameable>
	{
		/// <summary>
		/// Default instance of this equality comparer.
		/// </summary>
		public static NameableEqualityComparer Default { get; } = new NameableEqualityComparer();

		/// <inheritdoc />
		public bool Equals(INameable x, INameable y) => x?.Name == y?.Name;
		/// <inheritdoc />
		public int GetHashCode(INameable obj) => obj.GetHashCode();
	}
}