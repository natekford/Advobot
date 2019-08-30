using System;

namespace Advobot.Attributes.ParameterPreconditions.Strings
{
	/// <summary>
	/// Validates the nickname by making sure it is between 1 and 32 characters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class NicknameAttribute : StringParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string StringType => "nickname";

		/// <summary>
		/// Creates an instance of <see cref="NicknameAttribute"/>.
		/// </summary>
		public NicknameAttribute() : base(1, 32) { }
	}
}