using System;

namespace Advobot.Attributes.ParameterPreconditions.Strings
{
	/// <summary>
	/// Validates the guild name by making sure it is between 2 and 100 characters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class GuildNameAttribute : StringParameterPreconditionAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="GuildNameAttribute"/>.
		/// </summary>
		public GuildNameAttribute() : base(2, 100) { }

		/// <inheritdoc />
		public override string ToString()
			=> $"Valid guild name ({ValidLength} long)";
	}
}
