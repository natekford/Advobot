namespace Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation
{
	/// <summary>
	/// Validates the invite uses allowing specified valid values.
	/// </summary>
	public class ValidateInviteUsesAttribute : ValidateNumberAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateInviteUsesAttribute"/>.
		/// </summary>
		public ValidateInviteUsesAttribute() : base(new[] { 0, 1, 5, 10, 25, 50, 100 }) { }
	}
}