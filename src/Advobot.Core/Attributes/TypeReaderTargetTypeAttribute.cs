namespace Advobot.Attributes;

/// <summary>
/// Specifies what type this type reader targets.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class TypeReaderTargetTypeAttribute : Attribute
{
	/// <summary>
	/// The type this type reader targets.
	/// </summary>
	public IReadOnlyList<Type> TargetTypes { get; }

	/// <summary>
	/// Creates an instance of <see cref="TypeReaderTargetTypeAttribute"/>.
	/// </summary>
	/// <param name="types"></param>
	public TypeReaderTargetTypeAttribute(params Type[] types)
	{
		TargetTypes = types;
	}
}