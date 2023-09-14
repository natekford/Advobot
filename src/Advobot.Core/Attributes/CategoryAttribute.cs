namespace Advobot.Attributes;

/// <summary>
/// Indicates the category a command belongs to.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class CategoryAttribute(string category) : Attribute
{
	/// <summary>
	/// The command category commands belong to.
	/// </summary>
	public string Category { get; } = category;
}