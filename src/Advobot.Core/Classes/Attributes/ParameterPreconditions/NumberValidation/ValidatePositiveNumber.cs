namespace Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation
{
	/// <summary>
	/// Validates the passed in number allowing 1 to <see cref="int.MaxValue"/>.
	/// </summary>
	public class ValidatePositiveNumber : ValidateNumberAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidatePositiveNumber"/>.
		/// </summary>
		public ValidatePositiveNumber() : base(1, int.MaxValue) { }
	}
}