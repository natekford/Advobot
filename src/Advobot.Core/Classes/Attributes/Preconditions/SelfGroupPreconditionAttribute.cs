using System;
using Discord.Commands;

namespace Advobot.Classes.Attributes.Preconditions
{
	/// <summary>
	/// Functionally same as <see cref="PreconditionResult"/> except automatically sets the group as the type name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public abstract class SelfGroupPreconditionAttribute : PreconditionAttribute
	{
		/// <summary>
		/// Indicates this precondition should be visible in the help command.
		/// </summary>
		public abstract bool Visible { get; }

		/// <summary>
		/// Creates an instance of <see cref="SelfGroupPreconditionAttribute"/> with the Group as the type name.
		/// </summary>
		public SelfGroupPreconditionAttribute()
		{
			Group = GetType().Name;
		}
	}
}