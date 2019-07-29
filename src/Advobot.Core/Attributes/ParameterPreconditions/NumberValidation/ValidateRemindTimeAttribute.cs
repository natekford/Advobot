namespace Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation
{
	/// <summary>
	/// Validates the amount of time a reminder can last for in seconds allowing 1 to 526000 (1 year).
	/// </summary>
	public class ValidateRemindTimeAttribute : ValidateNumberAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateRemindTimeAttribute"/>.
		/// </summary>
		public ValidateRemindTimeAttribute() : base(1, 525600) { }
	}
}