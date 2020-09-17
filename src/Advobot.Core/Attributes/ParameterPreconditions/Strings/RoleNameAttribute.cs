using System;

namespace Advobot.Attributes.ParameterPreconditions.Strings
{
	/// <summary>
	/// Validates the role name by making sure it is between 1 and 100 characters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class RoleNameAttribute : StringRangeParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string StringType => "role name";

		/// <summary>
		/// Creates an instance of <see cref="RoleNameAttribute"/>.
		/// </summary>
		public RoleNameAttribute() : base(1, 100) { }
	}
}