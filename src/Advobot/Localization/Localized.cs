using System.Collections.Concurrent;
using System.Globalization;
using System.Resources;

namespace Advobot.Localization;

/// <summary>
/// Extension methods for <see cref="Localized{T}"/>.
/// </summary>
public static class Localized
{
	/// <summary>
	/// Creates an instance of <see cref="Localized{T}"/> with a class that needs no parameters.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static Localized<T> Create<T>() where T : new() => new(_ => new T());

	/// <summary>
	/// Calls <see cref="ResourceManager.GetString(string)"/> and throws an exception if it does not exist.
	/// </summary>
	/// <param name="resources"></param>
	/// <param name="name"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	public static string GetStringEnsured(this ResourceManager resources, string name)
	{
		var r = resources.GetString(name);
		if (r != null)
		{
			return r;
		}
		var culture = CultureInfo.CurrentUICulture;
		var message = $"{name} does not have an associated string in the {culture} culture.";
		throw new ArgumentException(message, name);
	}
}

/// <summary>
/// Holds different instances of <typeparamref name="T"/> based on <see cref="CultureInfo.CurrentUICulture"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="valueFactory"></param>
public sealed class Localized<T>(Func<CultureInfo, T> valueFactory)
{
	private readonly ConcurrentDictionary<CultureInfo, T> _Source = new();
	private readonly Func<CultureInfo, T> _ValueFactory = valueFactory;

	/// <summary>
	/// Gets or adds a value for the culture.
	/// </summary>
	/// <param name="culture"></param>
	/// <returns></returns>
	public T Get(CultureInfo? culture = null)
		=> _Source.GetOrAdd(culture ?? CultureInfo.CurrentUICulture, _ValueFactory);
}