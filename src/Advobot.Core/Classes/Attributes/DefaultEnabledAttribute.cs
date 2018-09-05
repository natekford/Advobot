using System;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Specifies the default value for whether a command is enabled or not.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class DefaultEnabledAttribute : Attribute
	{
		/// <summary>
		/// Whether or not the command is enabled by default.
		/// </summary>
		public bool Enabled { get; }
		/// <summary>
		/// Whether or not the command can be toggled.
		/// </summary>
		public bool AbleToToggle { get; set; } = true;

		/// <summary>
		/// Creates an instance of <see cref="DefaultEnabledAttribute"/>.
		/// </summary>
		/// <param name="enabled"></param>
		public DefaultEnabledAttribute(bool enabled)
		{
			Enabled = enabled;
		}
	}
}
