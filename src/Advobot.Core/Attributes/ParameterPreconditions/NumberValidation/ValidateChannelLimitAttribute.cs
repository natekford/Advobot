namespace Advobot.Attributes.ParameterPreconditions.NumberValidation
{
	/// <summary>
	/// Validates the channel limit allowing 0 to 99.
	/// </summary>
	public class ValidateChannelLimitAttribute : ValidateNumberAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateChannelLimitAttribute"/>.
		/// </summary>
		public ValidateChannelLimitAttribute() : base(0, 99) { }
	}
}