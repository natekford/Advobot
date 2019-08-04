namespace Advobot.Attributes.ParameterPreconditions
{
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

	/// <summary>
	/// How something must exist before being valid.
	/// </summary>
	public enum ExistenceStatus
	{
		/// <summary>
		/// The value can either exist or not exist.
		/// </summary>
		None,
		/// <summary>
		/// The value can only exist.
		/// </summary>
		MustExist,
		/// <summary>
		/// The value can only not exist.
		/// </summary>
		MustNotExist,
	}
}
