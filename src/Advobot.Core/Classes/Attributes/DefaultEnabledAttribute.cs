using System;

namespace Advobot.Core.Classes.Attributes
{
	/// <summary>
	/// Specifies the default value for whether a command is enabled or not.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class DefaultEnabledAttribute : Attribute
	{
		public bool Enabled { get; }

		public DefaultEnabledAttribute(bool enabled)
		{
			Enabled = enabled;
		}
	}
}
