namespace Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation
{
	/// <summary>
	/// Validates the amount of days to prune with allowing specified valid values.
	/// </summary>
	public class ValidatePruneDaysAttribute : ValidateNumberAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidatePruneDaysAttribute"/>.
		/// </summary>
		public ValidatePruneDaysAttribute() : base(new[] { 1, 7, 30 }) { }
	}
}