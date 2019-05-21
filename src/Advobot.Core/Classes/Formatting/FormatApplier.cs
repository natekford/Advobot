using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Classes.Formatting
{
	/// <summary>
	/// Determines whether to add some formatting to a string.
	/// </summary>
	public class FormatApplier
	{
		/// <summary>
		/// Whether this is enabled.
		/// </summary>
		public bool Enabled { get; set; } = true;
		/// <summary>
		/// The format this formatter applies to.
		/// </summary>
		public string FormatName { get; }

		private readonly Func<string, string> _Modifier;

		/// <summary>
		/// Creates an instance of <see cref="FormatApplier"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="modifier"></param>
		public FormatApplier(string name, Func<string, string> modifier)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(nameof(name));
			}
			if (modifier == null)
			{
				throw new ArgumentException(nameof(name));
			}

			FormatName = name;
			_Modifier = modifier;
		}

		/// <summary>
		/// Returns a modified string if <paramref name="formats"/> contains the format this formatter is specified for or if this is enaabled and no formats are passed in.
		/// </summary>
		/// <param name="formats"></param>
		/// <param name="arg"></param>
		/// <returns></returns>
		public string ModifyString(IReadOnlyCollection<string> formats, string arg)
			=> formats.Contains(FormatName) || (formats.Count == 0 && Enabled) ? _Modifier(arg) : arg;
	}
}
