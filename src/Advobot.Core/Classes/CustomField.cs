namespace Advobot.Classes
{
	/// <summary>
	/// Specifies a field on an embed.
	/// </summary>
	public sealed class CustomField
	{
		/// <summary>
		/// The name of the field.
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// The text of the field.
		/// </summary>
		public string Text { get; private set; }
		/// <summary>
		/// Whether the field is inline.
		/// </summary>
		public bool Inline { get; private set; }

		/// <summary>
		/// Returns the name and text.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> $"**Name:** `{Name}`\n**Text:** `{Text}`";
	}
}
