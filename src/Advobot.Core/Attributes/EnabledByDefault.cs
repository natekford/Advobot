using System;

namespace Advobot.Attributes
{
	/// <summary>
	/// Specifies the default value for whether a command is enabled or not.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class EnabledByDefaultAttribute : Attribute
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
		/// Creates an instance of <see cref="EnabledByDefaultAttribute"/>.
		/// </summary>
		/// <param name="enabled"></param>
		public EnabledByDefaultAttribute(bool enabled)
		{
			Enabled = enabled;
		}
	}
}
