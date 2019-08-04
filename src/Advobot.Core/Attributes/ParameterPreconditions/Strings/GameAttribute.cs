using System;

namespace Advobot.Attributes.ParameterPreconditions.Strings
{
	/// <summary>
	/// Validates the game by making sure it is between 0 and 128 characters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class GameAttribute : StringParameterPreconditionAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="GameAttribute"/>.
		/// </summary>
		public GameAttribute() : base(0, 128) { }

		/// <inheritdoc />
		public override string ToString()
			=> $"Valid game ({ValidLength} long)";
	}
}
