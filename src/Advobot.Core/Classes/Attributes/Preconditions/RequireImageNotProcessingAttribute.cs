using System;
using System.Threading.Tasks;
using Advobot.Classes.Modules;
using Advobot.Services.ImageResizing;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Classes.Attributes.Preconditions
{
	/// <summary>
	/// Disallows the command from running if an image is currently being resized.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireImageNotProcessingAttribute : AdvobotPreconditionAttribute
	{
		/// <inheritdoc />
		public override bool Visible => true;

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, CommandInfo command, IServiceProvider services)
		{
			var resizer = services.GetRequiredService<IImageResizer>();
			return resizer.IsGuildAlreadyProcessing(context.Guild.Id)
				? Task.FromResult(PreconditionResult.FromError("Guild already has an image processing."))
				: Task.FromResult(PreconditionResult.FromSuccess());
		}
		/// <summary>
		/// Returns a string describing what this attribute requires.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> "Not currently processing another image";
	}
}