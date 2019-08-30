namespace Advobot.Attributes.Preconditions.QuantityLimits
{
	/// <summary>
	/// Specifies what action the command is going to do.
	/// </summary>
	public enum QuantityLimitAction
	{
		/// <summary>
		/// Specifies this command is used to add something.
		/// </summary>
		Add,

		/// <summary>
		/// Specifies this command is used to remove something.
		/// </summary>
		Remove,
	}
}