namespace Advobot.Attributes;

/// <summary>
/// Specifies the default value for whether a command is enabled or not.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class MetaAttribute(string guid) : Attribute
{
	/// <summary>
	/// Whether or not the command can be toggled.
	/// </summary>
	public bool CanToggle { get; set; } = true;
	/// <summary>
	/// The id of the command.
	/// </summary>
	public Guid Guid { get; } = Guid.Parse(guid);
	/// <summary>
	/// Whether or not the command is enabled by default.
	/// </summary>
	public bool IsEnabled { get; set; }
}