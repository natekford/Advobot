using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace Advobot.Localization
{
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
		public static Localized<T> Create<T>() where T : new()
			=> new Localized<T>(key => new T());
	}

	/// <summary>
	/// Holds different instances of <typeparamref name="T"/> based on <see cref="CultureInfo.CurrentUICulture"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class Localized<T>
	{
		private readonly ConcurrentDictionary<CultureInfo, T> _Source
			= new ConcurrentDictionary<CultureInfo, T>();

		private readonly Func<CultureInfo, T> _ValueFactory;

		/// <summary>
		/// Creates an instance of <see cref="Localized{T}"/>.
		/// </summary>
		/// <param name="valueFactory"></param>
		public Localized(Func<CultureInfo, T> valueFactory)
		{
			_ValueFactory = valueFactory;
		}

		/// <summary>
		/// Gets or adds a value for the current culture.
		/// </summary>
		/// <returns></returns>
		public T Get()
			=> _Source.GetOrAdd(CultureInfo.CurrentUICulture, _ValueFactory);
	}
}