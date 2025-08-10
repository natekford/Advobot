namespace Advobot.TypeReaders;

/// <summary>
/// Specifies what type this type reader targets.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class TypeReaderTargetTypeAttribute(params Type[] types) : Attribute
{
	/// <summary>
	/// The type this type reader targets.
	/// </summary>
	public IReadOnlyList<Type> TargetTypes { get; } = types;
}