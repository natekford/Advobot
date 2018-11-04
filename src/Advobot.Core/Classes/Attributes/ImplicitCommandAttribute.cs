using System;
using System.Runtime.CompilerServices;
using Discord.Commands;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Uses <see cref="CallerMemberNameAttribute"/> to implicitly pass in the name of the method this attribute is applied to.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class ImplicitCommandAttribute : CommandAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ImplicitCommandAttribute"/>.
		/// </summary>
		/// <param name="name"></param>
		public ImplicitCommandAttribute([CallerMemberName] string name = null) : base(name) { }
	}
}
