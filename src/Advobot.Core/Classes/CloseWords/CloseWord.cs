using System.Diagnostics;

using Advobot.Interfaces;

namespace Advobot.Classes.CloseWords
{
	/// <summary>
	/// Holds an object which has a name and text and its closeness.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public sealed class CloseWord<T> where T : INameable
	{
		/// <summary>
		/// How close the name is to the search term.
		/// </summary>
		public int Closeness { get; set; }

		/// <summary>
		/// The object this is coming from.
		/// </summary>
		public T Value { get; set; }

		/// <summary>
		/// The name of the object.
		/// </summary>
		public string Name => Value.Name;

		private string DebuggerDisplay
			=> $"Name = {Name}, Closeness = {Closeness}";

		/// <summary>
		/// Initializes the object with the supplied values.
		/// </summary>
		/// <param name="closeness"></param>
		/// <param name="value"></param>
		public CloseWord(int closeness, T value)
		{
			Closeness = closeness;
			Value = value;
		}
	}
}