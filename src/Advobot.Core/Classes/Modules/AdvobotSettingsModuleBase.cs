using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Classes.Modules
{
	/// <summary>
	/// Handles methods for printing out, modifying, and resetting settings.
	/// </summary>
	/// <typeparam name="TSettings">The settings to modify.</typeparam>
	/// <remarks>
	/// This uses expression a lot so the property name can be gotten from the same argument.
	/// This essentially acts as reflection but with more type safety and resistance to refactoring issues.
	/// </remarks>
	public abstract class ReadOnlyAdvobotSettingsModuleBase<TSettings> : AdvobotModuleBase where TSettings : ISettingsBase
	{
		/// <summary>
		/// Returns the settings this module is targetting.
		/// </summary>
		/// <returns></returns>
		protected abstract TSettings Settings { get; }

		/// <summary>
		/// Prints out the names of settings.
		/// </summary>
		/// <returns></returns>
		protected Task ShowNamesAsync()
		{
			throw new NotImplementedException();
			/*
			return ReplyEmbedAsync(new EmbedWrapper
			{
				Title = Settings.GetType().Name.FormatTitle(),
				Description = $"`{Settings.GetSettingNames().Join("`, `")}`"
			});*/
		}
		/// <summary>
		/// Prints out all the settings.
		/// </summary>
		/// <returns></returns>
		protected Task ShowAllAsync()
		{
			throw new NotImplementedException();
			/*
			return ReplyFileAsync($"**{Settings.GetType().Name.FormatTitle()}:**", new TextFileInfo
			{
				Name = Settings.GetType().Name.FormatTitle().Replace(' ', '_'),
				Text = Settings.Format(Context.Client, Context.Guild),
			}); */
		}
		/// <summary>
		/// Prints out the specified setting.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		protected Task ShowAsync(string name)
		{
			throw new NotImplementedException();
			/*
			if (!Settings.IsSetting(name))
			{
				return ReplyErrorAsync($"`{name}` is not a valid setting.");
			}

			var description = Settings.FormatSetting(Context.Client, Context.Guild, name);
			if (description.Length <= EmbedBuilder.MaxDescriptionLength)
			{
				return ReplyEmbedAsync(new EmbedWrapper
				{
					Title = name,
					Description = description,
				});
			}
			return ReplyFileAsync($"**{name}:**", new TextFileInfo
			{
				Name = name,
				Text = description,
			}); */
		}
		/// <summary>
		/// Sends the settings file.
		/// </summary>
		/// <param name="accessor"></param>
		/// <returns></returns>
		protected Task GetFileAsync(IBotDirectoryAccessor accessor)
		{
			throw new NotImplementedException();
			/*
			var file = Settings.GetFile(accessor);
			if (!file.Exists)
			{
				return ReplyErrorAsync("The settings file does not exist.");
			}
			return Context.Channel.SendFileAsync(file.FullName, null); */
		}
	}

	public abstract class AdvobotSettingsModuleBase<TSettings> : ReadOnlyAdvobotSettingsModuleBase<TSettings>
		where TSettings : ISettingsBase
	{
		/// <summary>
		/// The name of the setting which has been changed.
		/// </summary>
		protected virtual string SettingName { get; }

		/// <summary>
		/// Returns a task which prints out the setting.
		/// </summary>
		/// <returns></returns>
		protected Task ShowResponseAsync()
			=> ShowAsync(SettingName);
		/// <summary>
		/// Resets the specified field and 
		/// </summary>
		/// <param name="reset"></param>
		/// <returns></returns>
		protected Task ResetResponseAsync(Action<TSettings> reset)
		{
			reset.Invoke(Settings);
			return ReplyAsync($"Successfully set {SettingName} back to its default value.");
		}
		protected Task ModifyResponseAsync(Action<TSettings> setter)
		{
			setter.Invoke(Settings);
			return ReplyAsync($"Successfully set {SettingName} to {null}");
		}

		/// <inheritdoc />
		protected override void AfterExecute(CommandInfo command)
		{
			if (!command.Attributes.Any(x => x is DontSaveAfterExecutionAttribute))
			{
				Settings.Save(BotSettings);
			}
			base.AfterExecute(command);
		}
	}

	public interface ISettingModule
	{
		Task Show();
		Task Reset();
	}

	public interface ISettingModule<TProperty> : ISettingModule
	{
		Task Modify(TProperty value);
	}

	public interface ICollectionSettingModule<TProperty> : ISettingModule
	{
		Task Modify(bool add, TProperty value);
	}
}