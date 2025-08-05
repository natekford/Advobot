using Discord;

namespace Advobot.Embeds;

/// <summary>
/// Utilties for <see cref="EmbedWrapper"/>.
/// </summary>
public static class EmbedWrapperUtils
{
	/// <summary>
	/// Attempts to modify the author using a user. Does nothing if fails.
	/// </summary>
	/// <param name="embed"></param>
	/// <param name="user"></param>
	/// <param name="errors"></param>
	/// <returns></returns>
	public static bool TryAddAuthor(
		this EmbedWrapper embed,
		IUser user,
		out IReadOnlyList<EmbedException> errors)
	{
		var url = user?.GetAvatarUrl();
		return embed.TryAddAuthor(user?.Username, url, url, out errors);
	}
}