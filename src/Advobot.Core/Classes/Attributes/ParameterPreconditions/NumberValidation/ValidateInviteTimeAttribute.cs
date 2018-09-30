namespace Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation
{
	/// <summary>
	/// Validates the invite time in seconds allowing specified valid values.
	/// </summary>
	public class ValidateInviteTimeAttribute : ValidateNumberAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateInviteTimeAttribute"/>.
		/// </summary>
		public ValidateInviteTimeAttribute() : base(new[] { 0, 1800, 3600, 21600, 43200, 86400 }) { }
	}
}