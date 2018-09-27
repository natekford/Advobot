namespace Advobot.Classes.CloseWords
{
	/// <summary>
	/// Holds an object which has a name and text and its closeness.
	/// </summary>
	public sealed class CloseWord
	{
		/// <summary>
		/// How close the name is to the search term.
		/// </summary>
		public int Closeness { get; set; }
		/// <summary>
		/// The name of the object.
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// The text of the object.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Initializes the object. Parameterless constructor is used for the database.
		/// </summary>
		public CloseWord() { }
		/// <summary>
		/// Initializes the object with the supplied values.
		/// </summary>
		/// <param name="closeness"></param>
		/// <param name="name"></param>
		/// <param name="text"></param>
		public CloseWord(int closeness, string name, string text)
		{
			Closeness = closeness;
			Name = name;
			Text = text;
		}
	}
}