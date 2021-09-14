﻿namespace Advobot.Attributes.ParameterPreconditions.Strings
{
	/// <summary>
	/// Validates the bot prefix by making sure it is between 1 and 10 characters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class PrefixAttribute : StringRangeParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string StringType => "prefix";

		/// <summary>
		/// Creates an instance of <see cref="PrefixAttribute"/>.
		/// </summary>
		public PrefixAttribute() : base(1, 10) { }
	}
}