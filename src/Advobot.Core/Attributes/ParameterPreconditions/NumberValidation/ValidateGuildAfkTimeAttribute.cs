namespace Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation
{
	/// <summary>
	/// Validates the guild afk timer in seconds allowing specified valid values.
	/// </summary>
	public class ValidateGuildAfkTimeAttribute : ValidateNumberAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateGuildAfkTimeAttribute"/>.
		/// </summary>
		public ValidateGuildAfkTimeAttribute() : base(new[] { 60, 300, 900, 1800, 3600 }) { }
	}
}