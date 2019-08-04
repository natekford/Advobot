using System;

namespace Advobot.Attributes.ParameterPreconditions.Numbers
{
	/// <summary>
	/// Validates the guild afk timer in seconds allowing specified valid values.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class GuildAfkTimeAttribute : IntParameterPreconditionAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="GuildAfkTimeAttribute"/>.
		/// </summary>
		public GuildAfkTimeAttribute() : base(new[] { 60, 300, 900, 1800, 3600 }) { }

		/// <inheritdoc />
		public override string ToString()
			=> $"Valid afk time ({Numbers})";
	}
}