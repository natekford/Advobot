using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Classes
{
	/// <summary>
	/// Handles methods for printing out, modifying, and resetting settings.
	/// </summary>
	/// <typeparam name="TSettings">The settings to </typeparam>
	/// <typeparam name="TSettingsProvider">Can be a factory of the settings themselves.</typeparam>
	public abstract class AdvobotSettingsModuleBase<TSettings, TSettingsProvider> : AdvobotModuleBase
		where TSettings : ISettingsBase
		where TSettingsProvider : ISettingsProvider<TSettings>
	{
		private static bool _AlreadyChecked;

		/// <summary>
		/// Creates an instance of <see cref="AdvobotSettingsModuleBase{TSettings, TSettingsProvider}"/> which checks that every setting can be modified.
		/// </summary>
		/// <param name="provider"></param>
		public AdvobotSettingsModuleBase(TSettingsProvider provider)
		{
			//Make sure in the modify command that every setting has a method to invoke
			if (!_AlreadyChecked)
			{
				var implemented = GetType().GetMethods().Select(x => x.Name);
				var unimplemented = provider.GetSettings().Keys.Where(x => !implemented.CaseInsContains(x));
				if (unimplemented.Any())
				{
					throw new NotImplementedException($"Settings not modifiable: {string.Join(", ", unimplemented)}");
				}
				_AlreadyChecked = true;
			}
		}
		/// <summary>
		/// Creates an instance of <see cref="AdvobotSettingsModuleBase{TSettings, TSettingsProvider}"/> which does not check that every setting can be modified.
		/// </summary>
		public AdvobotSettingsModuleBase() { }

		/// <summary>
		/// Prints out the names of settings.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected async Task ShowNames(TSettings settings)
		{
			var embed = new EmbedWrapper
			{
				Title = settings.GetType().Name.FormatTitle(),
				Description = $"`{string.Join("`, `", settings.GetSettings().Keys)}`"
			};
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		/// <summary>
		/// Prints out all the settings.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected async Task ShowAll(TSettings settings)
		{
			var tf = new TextFileInfo
			{
				Name = settings.GetType().Name.FormatTitle().Replace(' ', '_'),
				Text = settings.ToString(Context.Client, Context.Guild),
			};
			await MessageUtils.SendMessageAsync(Context.Channel, $"**{settings.GetType().Name.FormatTitle()}:**", textFile: tf).CAF();
		}
		/// <summary>
		/// Prints out the specified setting.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="settingName"></param>
		/// <returns></returns>
		protected async Task ShowCommand(TSettings settings, string settingName)
		{
			if (!(await VerifyProperty(settings, settingName).CAF() is PropertyInfo property))
			{
				return;
			}

			var desc = settings.ToString(Context.Client, Context.Guild, property.Name);
			if (desc.Length <= EmbedBuilder.MaxDescriptionLength)
			{
				var embed = new EmbedWrapper
				{
					Title = property.Name.FormatTitle(),
					Description = desc
				};
				await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
			}
			else
			{
				var tf = new TextFileInfo
				{
					Name = property.Name.FormatTitle(),
					Text = desc,
				};
				await MessageUtils.SendMessageAsync(Context.Channel, $"**{property.Name.FormatTitle()}:**", textFile: tf).CAF();
			}
		}
		/// <summary>
		/// Sends the settings file.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="accessor"></param>
		/// <returns></returns>
		protected async Task GetFile(TSettings settings, IBotDirectoryAccessor accessor)
		{
			var file = settings.GetFile(accessor);
			if (!file.Exists)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("The settings file does not exist.")).CAF();
				return;
			}
			await Context.Channel.SendFileAsync(file.FullName).CAF();
		}
		/// <summary>
		/// Resets the targeted setting.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="settingName"></param>
		/// <returns></returns>
		protected async Task Reset(TSettings settings, string settingName)
		{
			if (!(await VerifyProperty(settings, settingName).CAF() is PropertyInfo property))
			{
				return;
			}
			settings.ResetSetting(property.Name);
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully reset `{property.Name}`.").CAF();
		}
		/// <summary>
		/// Adds or removes the specified object from the list.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="obj"></param>
		/// <param name="add"></param>
		/// <param name="settingName"></param>
		/// <returns></returns>
		protected async Task ModifyListAsync(TSettings settings, object obj, bool add, [CallerMemberName] string settingName = "")
		{
			if (!(await VerifyProperty(settings, settingName).CAF() is PropertyInfo property))
			{
				return;
			}
			settings.ModifyList(property.Name, obj, add);
			var resp = $"Successfully {(add ? "added" : "removed")} `{obj}` {(add ? "to" : "from")} `{property.Name}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		/// <summary>
		/// Sets the property to the supplied value.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="obj"></param>
		/// <param name="settingName"></param>
		/// <returns></returns>
		protected async Task ModifyAsync(TSettings settings, object obj, [CallerMemberName] string settingName = "")
		{
			if (!(await VerifyProperty(settings, settingName).CAF() is PropertyInfo property))
			{
				return;
			}
			settings.SetSetting(property.Name, obj);
			var resp = $"Successfully set `{property.Name}` to `{settings.ToString(Context.Client, Context.Guild, property.Name)}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		/// <summary>
		/// Makes sure the settings exist, otherwise prints out to the channel that they don't.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		private async Task<PropertyInfo> VerifyProperty(TSettings settings, string name)
		{
			if (!settings.GetSettings().TryGetValue(name, out var property))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"`{name}` is not a valid setting.")).CAF();
				return null;
			}
			return property;
		}
		/// <inheritdoc />
		protected override void AfterExecute(CommandInfo command)
		{
			var preconditions = command.Preconditions.Concat(command.Module.Preconditions);
			if (preconditions.Any(x => x is SaveGuildSettingsAttribute))
			{
				Context.GuildSettings.SaveSettings(Context.BotSettings);
			}
			if (preconditions.Any(x => x is SaveBotSettingsAttribute))
			{
				Context.BotSettings.SaveSettings(Context.BotSettings);
			}
			base.AfterExecute(command);
		}
	}
}