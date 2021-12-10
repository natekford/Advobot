namespace Advobot.Classes;

/// <summary>
/// Indicates whether when searching for a number to look at numbers exactly equal, below, or above.
/// </summary>
public enum CountTarget
{
	/// <summary>
	/// Valid results are results that are the same.
	/// </summary>
	Equal,
	/// <summary>
	/// Valid results are results that are below.
	/// </summary>
	Below,
	/// <summary>
	/// Valid results are results that are above.
	/// </summary>
	Above,
}

/// <summary>
/// Utilities for <see cref="Filterer{T}"/>.
/// </summary>
public static class FitlererUtils
{
	/// <summary>
	/// Returns objects where the function does not return null and is either equal to, less than, or greater than a specified number.
	/// </summary>
	/// <param name="objects"></param>
	/// <param name="method"></param>
	/// <param name="number"></param>
	/// <param name="f"></param>
	/// <returns></returns>
	public static IEnumerable<T> GetFromCount<T>(
		this IEnumerable<T> objects,
		CountTarget method,
		int? number,
		Func<T, int?> f) => method switch
		{
			CountTarget.Equal => objects.Where(x => f(x) == number),
			CountTarget.Below => objects.Where(x => f(x) < number),
			CountTarget.Above => objects.Where(x => f(x) > number),
			_ => throw new ArgumentOutOfRangeException(nameof(method)),
		};
}

/// <summary>
/// Finds matching items.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Filterer<T>
{
	/// <summary>
	/// Finds matching items from <paramref name="source"/>.
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	public abstract IReadOnlyList<T> Filter(IEnumerable<T> source);
}