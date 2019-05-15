namespace Advobot.Classes.Attributes.ParameterPreconditions.StringValidation
{
	/// <summary>
	/// Validates the channel topic by making sure it is between 0 and 1024 characters.
	/// </summary>
	public sealed class ValidateChannelTopicAttribute : ValidateStringAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateChannelTopicAttribute"/>.
		/// </summary>
		public ValidateChannelTopicAttribute() : base(0, 1024) { }
	}
}
