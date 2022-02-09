namespace Advobot.ParameterPreconditions;

/// <summary>
/// An interface for something which can either exist or not exist and some states of that may be invalid.
/// </summary>
public interface IExistenceParameterPrecondition
{
	/// <summary>
	/// How something must exist before being valid.
	/// </summary>
	ExistenceStatus Status { get; }
}