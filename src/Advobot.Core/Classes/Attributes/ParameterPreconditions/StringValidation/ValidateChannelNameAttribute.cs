namespace Advobot.Classes.Attributes.ParameterPreconditions.StringValidation
{
	/// <summary>
	/// Validates the channel name by making sure it is between 2 and 100 characters.
	/// </summary>
	public class ValidateChannelNameAttribute : ValidateStringAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateChannelNameAttribute"/>.
		/// </summary>
		public ValidateChannelNameAttribute() : base(2, 100) { }
	}
}
