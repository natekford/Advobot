using System;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Specifies the default value for whether a command is enabled or not.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class DefaultEnabledAttribute : Attribute
	{
		public readonly bool Enabled;

		public DefaultEnabledAttribute(bool enabled)
		{
			Enabled = enabled;
		}
	}

	/// <summary>
	/// Describes what arguments to invoke the command with.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class UsageAttribute : Attribute
	{
		public readonly string Usage;

		public UsageAttribute(string usage)
		{
			Usage = usage;
		}

		public string ToString(string name)
		{
			return name + " " + Usage;
		}
	}
}
