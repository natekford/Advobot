using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
	public abstract class AdvobotSettingsModuleBase<TSettings> : AdvobotModuleBase where TSettings : ISettingsBase
	{
		/// <summary>
		/// Prints out the names of settings.
		/// </summary>
		/// <returns></returns>
		protected async Task ShowNamesAsync()
		{
			var settings = GetSettings();
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
		/// <returns></returns>
		protected async Task ShowAllAsync()
		{
			var settings = GetSettings();
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
		/// <param name="settingName"></param>
		/// <returns></returns>
		protected async Task ShowAsync(string settingName)
		{
			var settings = GetSettings();
			if (!(await VerifyAsync(settings, settingName).CAF() is PropertyInfo property))
			{
				return;
			}

			var desc = settings.FormatSetting(Context.Client, Context.Guild, property.Name);
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
		/// <param name="accessor"></param>
		/// <returns></returns>
		protected async Task GetFileAsync(IBotDirectoryAccessor accessor)
		{
			var file = GetSettings().GetFile(accessor);
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
		/// <param name="settingName"></param>
		/// <returns></returns>
		protected async Task ResetAsync(string settingName)
		{
			var settings = GetSettings();
			if (!(await VerifyAsync(settings, settingName).CAF() is PropertyInfo property))
			{
				return;
			}
			settings.ResetSetting(property.Name);
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully reset `{property.Name}`.").CAF();
		}
		/// <summary>
		/// Adds or removes the specified object from the list while also firing <see cref="ISettingsBase.RaisePropertyChanged(string)"/> and sending a response in Discord.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="value"></param>
		/// <param name="add"></param>
		/// <returns></returns>
		protected async Task ModifyListAsync<T>(Expression<Func<TSettings, IList<T>>> property, T value, bool add)
		{
			var settings = GetSettings();
			var expr = (MemberExpression)property.Body;
			var prop = (PropertyInfo)expr.Member;
			var list = property.Compile()(settings);

			if (!add)
			{
				list.Remove(value);
			}
			else if (!list.Contains(value))
			{
				list.Add(value);
			}

			settings.RaisePropertyChanged(prop.Name);
			var resp = $"Successfully {(add ? "added" : "removed")} `{FormatValue(settings, value)}` {(add ? "to" : "from")} `{prop.Name}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		/// <summary>
		/// Sets the property to the supplied value while also firing <see cref="ISettingsBase.RaisePropertyChanged(string)"/> and sending a response in Discord.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		protected async Task ModifyAsync<T>(Expression<Func<TSettings, T>> property, T value)
		{
			//This uses Expression<Func<TSettings, T>> instead of Action<TSettings, T> so the property name can be gotten from the same arg
			var settings = GetSettings();
			var expr = (MemberExpression)property.Body;
			var prop = (PropertyInfo)expr.Member;
			prop.SetValue(settings, value);

			settings.RaisePropertyChanged(prop.Name);
			var resp = $"Successfully set `{prop.Name}` to `{FormatValue(settings, value)}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		/// <summary>
		/// Returns the settings this module is targetting.
		/// </summary>
		/// <returns></returns>
		protected abstract TSettings GetSettings();
		/// <summary>
		/// Saves the settings this module is targetting.
		/// </summary>
		protected abstract void SaveSettings();
		/// <summary>
		/// Returns the value as a string.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private string FormatValue(TSettings settings, object value)
			=> settings.FormatValue(Context.Client, Context.Guild, value);
		/// <summary>
		/// Makes sure the settings exist, otherwise prints out to the channel that they don't.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		private async Task<PropertyInfo> VerifyAsync(TSettings settings, string name)
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
			SaveSettings();
			base.AfterExecute(command);
		}
	}
}