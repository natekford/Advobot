using System;

namespace Advobot.Attributes.ParameterPreconditions.Strings
{
	/// <summary>
	/// Validates the emote name by making sure it is between 2 and 32 characters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class EmoteNameAttribute : StringParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string StringType => "emote name";

		/// <summary>
		/// Creates an instance of <see cref="EmoteNameAttribute"/>.
		/// </summary>
		public EmoteNameAttribute() : base(2, 32) { }
	}
}