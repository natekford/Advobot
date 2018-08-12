using System;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Specifies the default value for whether a command is enabled or not.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class DefaultEnabledAttribute : Attribute
	{
		/// <summary>
		/// Whether or not the command is enabled by default.
		/// </summary>
		public bool Enabled { get; }
		/// <summary>
		/// Whether or not the command can be toggled.
		/// </summary>
		public bool AbleToToggle { get; }

		/// <summary>
		/// Creates an instance of <see cref="DefaultEnabledAttribute"/>.
		/// </summary>
		/// <param name="enabled"></param>
		/// <param name="ableToToggle"></param>
		public DefaultEnabledAttribute(bool enabled, bool ableToToggle = true)
		{
			Enabled = enabled;
			AbleToToggle = ableToToggle;
		}
	}
}
