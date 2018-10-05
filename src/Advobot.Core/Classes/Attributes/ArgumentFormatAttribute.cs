using System;
using System.Collections.Generic;
using System.Text;
using AdvorangesSettingParser.Implementation;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Describes how an argument needs to be supplied to parse correctly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
	public sealed class ArgumentFormatAttribute : Attribute
	{
		/// <summary>
		/// The format to parse correctly.
		/// </summary>
		public string Format { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="format"></param>
		public ArgumentFormatAttribute(string format)
		{
			Format = format;
		}
	}

	public class ArgumentFormatRegistry
	{
		/// <summary>
		/// The singleton instance of this.
		/// </summary>
		public static ArgumentFormatRegistry Instance = new ArgumentFormatRegistry();

		/// <summary>
		/// The types registered in this instance.
		/// </summary>
		public IEnumerable<Type> RegisteredTypes => _Formats.Keys;

		private Dictionary<Type, string> _Formats = new Dictionary<Type, string>();

		public void Register<T>(string format)
			=> _Formats[typeof(T)] = format;
		public void Remove<T>()
			=> _Formats.Remove(typeof(T));
		public string Retrieve<T>()
			=> TryRetrieve<T>(out var value) ? value : throw new KeyNotFoundException($"There is no format registered for {typeof(T).Name}.");
		public bool TryRetrieve<T>(out string format)
			=> _Formats.TryGetValue(typeof(T), out format);
	}
}