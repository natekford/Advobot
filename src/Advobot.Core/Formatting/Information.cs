using Advobot.Utilities;

namespace Advobot.Formatting
{
	/// <summary>
	/// Holds a title and value.
	/// </summary>
	public sealed class Information
	{
		/// <summary>
		/// The name of this information.
		/// </summary>
		public string Title { get; }
		/// <summary>
		/// What to put in between <see cref="Title"/> and <see cref="Value"/>
		/// </summary>
		public string Joiner { get; }
		/// <summary>
		/// The value of this information.
		/// </summary>
		public string Value { get; }

		/// <summary>
		/// Creates an instance of <see cref="Information"/>.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="value"></param>
		/// <param name="joiner"></param>
		public Information(string title, string value, string joiner)
		{
			Title = title;
			Joiner = joiner;
			Value = value;
		}

		/// <inheritdoc />
		public override string ToString()
			=> $"{Title.AsTitleWithColon()}{Joiner}{Value}";
	}
}
