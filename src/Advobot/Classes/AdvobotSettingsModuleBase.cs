using System;
using System.Collections;
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
using Discord.Commands.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Classes
{
	/// <summary>
	/// Handles methods for printing out, modifying, and resetting settings.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class AdvobotSettingsModuleBase<T> : AdvobotModuleBase where T : ISettingsBase
	{
		/// <summary>
		/// Prints out the names of settings.
		/// </summary>
		/// <param name="service"></param>
		/// <returns></returns>
		protected async Task ShowNames(T service)
		{
			var embed = new EmbedWrapper
			{
				Title = service.GetType().Name.FormatTitle(),
				Description = $"`{String.Join("`, `", service.GetSettings().Keys)}`"
			};
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		/// <summary>
		/// Prints out all the settings.
		/// </summary>
		/// <param name="service"></param>
		/// <returns></returns>
		protected async Task ShowAll(T service)
		{
			var tf = new TextFileInfo
			{
				Name = service.GetType().Name.FormatTitle().Replace(' ', '_'),
				Text = service.ToString(Context.Client, Context.Guild),
			};
			await MessageUtils.SendMessageAsync(Context.Channel, $"**{service.GetType().Name.FormatTitle()}:**", textFile: tf).CAF();
		}
		/// <summary>
		/// Prints out the specified setting.
		/// </summary>
		/// <param name="service"></param>
		/// <param name="settingName"></param>
		/// <returns></returns>
		protected async Task ShowCommand(T service, string settingName)
		{
			if (!(await VerifyProperty(service, settingName).CAF() is PropertyInfo property))
			{
				return;
			}

			var desc = service.ToString(Context.Client, Context.Guild, property.Name);
			if (desc.Length <= EmbedBuilder.MaxDescriptionLength)
			{
				var embed = new EmbedWrapper
				{
					Title = settingName,
					Description = desc
				};
				await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
			}
			else
			{
				var tf = new TextFileInfo
				{
					Name = settingName,
					Text = desc,
				};
				await MessageUtils.SendMessageAsync(Context.Channel, $"**{property.Name.FormatTitle()}:**", textFile: tf).CAF();
			}
		}
		/// <summary>
		/// Resets the targeted setting.
		/// </summary>
		/// <param name="service"></param>
		/// <param name="settingName"></param>
		/// <returns></returns>
		protected async Task Reset(T service, string settingName)
		{
			if (!(await VerifyProperty(service, settingName).CAF() is PropertyInfo property))
			{
				return;
			}
			service.ResetSetting(property.Name);
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully reset `{property.Name}`.").CAF();
		}
		/// <summary>
		/// Adds or removes the specified object from the list.
		/// </summary>
		/// <param name="service"></param>
		/// <param name="obj"></param>
		/// <param name="add"></param>
		/// <param name="settingName"></param>
		/// <returns></returns>
		protected async Task ModifyListAsync(T service, object obj, bool add, [CallerMemberName] string settingName = "")
		{
			if (!(await VerifyProperty(service, settingName).CAF() is PropertyInfo property))
			{
				return;
			}
			if (add)
			{
				((IList)property.GetValue(service)).Add(obj);
			}
			else
			{
				((IList)property.GetValue(service)).Remove(obj);
			}
			var resp = $"Successfully {(add ? "added" : "removed")} `{obj}` {(add ? "to" : "from")} `{property.Name}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		/// <summary>
		/// Sets the property to the supplied value.
		/// </summary>
		/// <param name="service"></param>
		/// <param name="obj"></param>
		/// <param name="settingName"></param>
		/// <returns></returns>
		protected async Task ModifyAsync(T service, object obj, [CallerMemberName] string settingName = "")
		{
			if (!(await VerifyProperty(service, settingName).CAF() is PropertyInfo property))
			{
				return;
			}
			property.SetValue(service, Convert.ChangeType(obj, property.PropertyType));
			var resp = $"Successfully set `{property.Name}` to `{service.ToString(Context.Client, Context.Guild, property.Name)}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		private async Task<PropertyInfo> VerifyProperty(T service, string name)
		{
			if (!service.GetSettings().TryGetValue(name, out var property))
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
				Context.GuildSettings.SaveSettings(Context.Config);
			}
			if (preconditions.Any(x => x is SaveBotSettingsAttribute))
			{
				Context.BotSettings.SaveSettings(Context.Config);
			}
			if (preconditions.Any(x => x is SaveLowLevelConfigAttribute))
			{
				Context.Config.SaveSettings();
			}
			base.AfterExecute(command);
		}
		/// <inheritdoc />
		protected override void OnModuleBuilding(CommandService commandService, ModuleBuilder builder)
		{
			//TODO: make this work by adding the setting attribute onto the interface?
			if (builder.Attributes.Any(x => x is SettingModificationAttribute))
			{
				var settings = SettingsBase.GetSettings(typeof(T));
				foreach (var command in builder.Commands)
				{
					settings.Remove(command.Name);
				}
				if (settings.Any())
				{
					throw new NotImplementedException($"Setting not modifiable: {String.Join(", ", settings.Keys)}");
				}
			}
			base.OnModuleBuilding(commandService, builder);
		}
	}
}