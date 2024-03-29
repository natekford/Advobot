﻿using System.Collections.Concurrent;
using System.Globalization;

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
}

/// <summary>
/// Holds different instances of <typeparamref name="T"/> based on <see cref="CultureInfo.CurrentUICulture"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// Creates an instance of <see cref="Localized{T}"/>.
/// </remarks>
/// <param name="valueFactory"></param>
public sealed class Localized<T>(Func<CultureInfo, T> valueFactory)
{
	private readonly ConcurrentDictionary<CultureInfo, T> _Source = new();
	private readonly Func<CultureInfo, T> _ValueFactory = valueFactory;

	/// <summary>
	/// Gets or adds a value for the current culture.
	/// </summary>
	/// <returns></returns>
	public T Get()
		=> _Source.GetOrAdd(CultureInfo.CurrentUICulture, _ValueFactory);
}