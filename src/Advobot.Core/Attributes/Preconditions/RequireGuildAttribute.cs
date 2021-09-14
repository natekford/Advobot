
using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using Discord.Commands;

namespace Advobot.Attributes.Preconditions
{
	/// <summary>
	/// Specifies that the command will only work in the passed in guild.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireGuildAttribute : PreconditionAttribute, IPrecondition
	{
		/// <summary>
		/// The id of the guild.
		/// </summary>
		public ulong Id { get; }
		/// <inheritdoc />
		public string Summary => $"Will only work in the guild with the id {Id}";

		/// <summary>
		/// Creates an instance of <see cref="RequireGuildAttribute"/>.
		/// </summary>
		/// <param name="id"></param>
		public RequireGuildAttribute(ulong id)
		{
			Id = id;
		}

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			if (context.Guild.Id == Id)
			{
				return this.FromSuccess().AsTask();
			}
			return PreconditionResult.FromError($"This guild does not have the id {Id}.").AsTask();
		}
	}
}