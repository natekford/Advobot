using Advobot.Core.Interfaces;

namespace Advobot.Core.Classes.CloseWords
{
	/// <summary>
	/// Holds an object which has a name and text and its closeness.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public struct CloseWord<T> where T : IDescription
	{
		public T Word { get; }
		public int Closeness { get; }

		public CloseWord(T word, int closeness)
		{
			Word = word;
			Closeness = closeness;
		}
	}
}
