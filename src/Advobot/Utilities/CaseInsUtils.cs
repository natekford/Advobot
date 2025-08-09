namespace Advobot.Utilities;

/// <summary>
/// Case insensitive string utilities.
/// </summary>
public static class CaseInsUtils
{
	/// <summary>
	/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to check if a string contains a search string.
	/// </summary>
	/// <param name="source"></param>
	/// <param name="search"></param>
	/// <returns></returns>
	public static bool CaseInsContains(this string source, string search)
		=> source != null && search != null && source.Contains(search, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Utilizes <see cref="StringComparer.OrdinalIgnoreCase"/> to see if the search string is in the enumerable.
	/// </summary>
	/// <param name="enumerable"></param>
	/// <param name="search"></param>
	/// <returns></returns>
	public static bool CaseInsContains(this IEnumerable<string> enumerable, string search)
		=> enumerable.Contains(search, StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to check if two strings are the same.
	/// </summary>
	/// <param name="str1"></param>
	/// <param name="str2"></param>
	/// <returns></returns>
	public static bool CaseInsEquals(this string? str1, string? str2)
		=> string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Returns the string with the oldValue replaced with the newValue case insensitively.
	/// </summary>
	/// <param name="source"></param>
	/// <param name="oldValue"></param>
	/// <param name="newValue"></param>
	/// <returns></returns>
	public static string CaseInsReplace(this string source, string oldValue, string newValue)
		=> source.Replace(oldValue, newValue, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to check if a string ends with a search string.
	/// </summary>
	/// <param name="source"></param>
	/// <param name="search"></param>
	/// <returns></returns>
	public static bool CaseInsStartsWith(this string source, string search)
		=> source != null && search != null && source.StartsWith(search, StringComparison.OrdinalIgnoreCase);
}