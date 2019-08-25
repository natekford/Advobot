using System.Collections.Generic;
using AdvorangesUtils;

namespace Advobot.Formatting
{
	/// <summary>
	/// Holds a 1d collection of <see cref="Information"/>.
	/// </summary>
	public sealed class InformationCollection
	{
		/// <summary>
		/// A row of an <see cref="InformationMatrix"/>.
		/// </summary>
		public IReadOnlyList<Information> Information => _Information.AsReadOnly();

		private readonly List<Information> _Information = new List<Information>();

		/// <summary>
		/// Adds a new <see cref="Formatting.Information"/>.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="value"></param>
		/// <param name="joiner"></param>
		public void Add(string title, string value, string joiner = " ")
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				_Information.Add(new Information(title, value, joiner));
			}
		}
		/// <summary>
		/// Adds a new <see cref="Formatting.Information"/> with the number's string value.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="value"></param>
		public void Add(string title, int value)
			=> Add(title, value.ToString());
		/// <summary>
		/// Adds a new <see cref="Formatting.Information"/> with the bool's string value.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="value"></param>
		public void Add(string title, bool value)
			=> Add(title, value.ToString());

		/// <inheritdoc />
		public override string ToString()
			=> _Information.Join(x => x.ToString(), "\n");
	}
}
