namespace Advobot.Classes.Formatting
{
	/// <summary>
	/// Formats something which potentially involves a guild.
	/// </summary>
	public interface IGuildFormattable
	{
		/// <summary>
		/// Returns the object in a human readable format.
		/// </summary>
		/// <returns></returns>
		IDiscordFormattableString GetFormattableString();
	}
}
