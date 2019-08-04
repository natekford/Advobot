using System;

namespace Advobot.Attributes.ParameterPreconditions.Numbers
{
	/// <summary>
	/// Validates the amount of time a reminder can last for in seconds allowing 1 to 526000 (1 year).
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class RemindTimeAttribute : IntParameterPreconditionAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="RemindTimeAttribute"/>.
		/// </summary>
		public RemindTimeAttribute() : base(1, 525600) { }

		/// <inheritdoc />
		public override string ToString()
			=> $"Valid remind time ({Numbers})";
	}
}