using YACCS.Commands.Attributes;

namespace Advobot.Attributes;

/// <summary>
/// Specifies the default value for whether a command is enabled or not.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class MetaAttribute(string id) : IdAttribute(id)
{
	/// <summary>
	/// Whether or not the command can be toggled.
	/// </summary>
	public bool CanToggle { get; set; } = true;
	/// <summary>
	/// Whether or not the command is enabled by default.
	/// </summary>
	public bool IsEnabled { get; set; }
}