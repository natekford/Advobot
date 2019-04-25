using Discord.Commands;

namespace Advobot.Classes
{
	/// <summary>
	/// Specifies a field on an embed.
	/// </summary>
	[NamedArgumentType]
	public sealed class CustomField
	{
		/// <summary>
		/// The name of the field.
		/// </summary>
		public string Name { get; set; } = Constants.ZERO_LENGTH_CHAR;
		/// <summary>
		/// The text of the field.
		/// </summary>
		public string Text { get; set; } = Constants.ZERO_LENGTH_CHAR;
		/// <summary>
		/// Whether the field is inline.
		/// </summary>
		public bool Inline { get; set; }

		/// <summary>
		/// Returns the name and text.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> $"**Name:** `{Name}`\n**Text:** `{Text}`";
	}
}
