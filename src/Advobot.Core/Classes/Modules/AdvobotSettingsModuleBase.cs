using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesSettingParser;
using AdvorangesSettingParser.Implementation;
using AdvorangesSettingParser.Implementation.Instance;
using AdvorangesSettingParser.Interfaces;
using AdvorangesSettingParser.Results;
using AdvorangesSettingParser.Utils;
using AdvorangesUtils;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
	public abstract class AdvobotSettingsModuleBase<TSettings> : AdvobotModuleBase where TSettings : ISettingsBase
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
			return ReplyEmbedAsync(new EmbedWrapper
			{
				Title = Settings.GetType().Name.FormatTitle(),
				Description = $"`{Settings.SettingParser.Join("`, `", x => x.MainName)}`"
			});
		}
		/// <summary>
		/// Prints out all the settings.
		/// </summary>
		/// <returns></returns>
		protected Task ShowAllAsync()
		{
			return ReplyFileAsync($"**{Settings.GetType().Name.FormatTitle()}:**", new TextFileInfo
			{
				Name = Settings.GetType().Name.FormatTitle().Replace(' ', '_'),
				Text = Settings.Format(Context.Client, Context.Guild),
			});
		}
		/// <summary>
		/// Prints out the specified setting.
		/// </summary>
		/// <param name="settingName"></param>
		/// <returns></returns>
		protected Task ShowAsync(string settingName)
		{
			if (!Settings.SettingParser.TryGetSetting(settingName, PrefixState.NotPrefixed, out var setting))
			{
				return ReplyErrorAsync($"`{settingName}` is not a valid setting.");
			}

			var description = FormatValue(setting.GetValue());
			if (description.Length <= EmbedBuilder.MaxDescriptionLength)
			{
				return ReplyEmbedAsync(new EmbedWrapper
				{
					Title = setting.MainName.FormatTitle(),
					Description = description,
				});
			}
			return ReplyFileAsync($"**{setting.MainName.FormatTitle()}:**", new TextFileInfo
			{
				Name = setting.MainName.FormatTitle(),
				Text = description,
			});
		}
		/// <summary>
		/// Prints out the values which target the specified user. If the setting does not support that, instead prints out an error.
		/// </summary>
		/// <param name="settingName"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		protected Task ShowUserAsync(string settingName, IUser user)
		{
			if (!Settings.SettingParser.TryGetSetting(settingName, PrefixState.NotPrefixed, out var setting))
			{
				return ReplyErrorAsync($"`{settingName}` is not a valid setting.");
			}

			var value = setting.GetValue();
			var title = setting.MainName.FormatTitle();

			IEnumerable<ITargetsUser> values;
			if (value is IEnumerable<ITargetsUser> temp1)
			{
				values = temp1;
			}
			//I don't know if this one should ever occur, but if it does, it's relatively easy to handle anyways
			else if (value is ITargetsUser temp2)
			{
				values = new[] { temp2 };
			}
			//If doesn't target users, then reply that
			else
			{
				return ReplyErrorAsync($"`{title}` does not target users directly.");
			}

			var userValues = values.Where(x => x.UserId == user.Id).ToArray();
			var description = userValues.Length == 0 ? "None" : userValues.FormatNumberedList(x => FormatValue(x));
			return ReplyEmbedAsync(new EmbedWrapper
			{
				Title = $"{title} | {user.Format()}",
				Description = description,
			});
		}
		/// <summary>
		/// Sends the settings file.
		/// </summary>
		/// <param name="accessor"></param>
		/// <returns></returns>
		protected Task GetFileAsync(IBotDirectoryAccessor accessor)
		{
			var file = Settings.GetFile(accessor);
			if (!file.Exists)
			{
				return ReplyErrorAsync("The settings file does not exist.");
			}
			return Context.Channel.SendFileAsync(file.FullName, null);
		}
		/// <summary>
		/// Resets the targeted setting.
		/// </summary>
		/// <param name="settingName"></param>
		/// <returns></returns>
		protected Task ResetAsync(string settingName)
		{
			if (!Settings.SettingParser.TryGetSetting(settingName, PrefixState.NotPrefixed, out var setting))
			{
				return ReplyErrorAsync($"`{settingName}` is not a valid setting.");
			}
			Settings.ResetSetting(setting.MainName);
			return ReplyTimedAsync($"Successfully reset `{setting.MainName}`.");
		}
		/// <summary>
		/// Adds or removes the specified objects from the list while also firing <see cref="ISettingsBase.RaisePropertyChanged(string)"/> and sending a response in Discord.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="selector"></param>
		/// <param name="add"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		protected Task ModifyCollectionAsync<TValue>(Expression<Func<TSettings, ICollection<TValue>>> selector, bool add, IEnumerable<TValue> values)
		{
			var (settings, setting, source, name) = GetCollection(selector);
			var context = new CollectionModificationContext { Action = add ? CollectionModificationAction.AddIfMissing : CollectionModificationAction.Remove };
			var results = values.Select(x => setting.ModifyCollection(source, x, context));
			settings.RaisePropertyChanged(name);

			var successes = results.Where(x => x.IsSuccess).OfType<SetValueResult>().Select(x => FormatValue(x.Value)).ToArray();
			var failures = results.Where(x => !x.IsSuccess).OfType<SetValueResult>().Select(x => FormatValue(x.Value)).ToArray();
			var success = successes.Any()
				? $"Successfully {(add ? "added" : "removed")} `{string.Join("`, `", successes)}` {(add ? "to" : "from")} `{name}`."
				: null;
			var failure = failures.Any()
				? $"`{string.Join("`, `", failures)}` {(failures.Length == 1 ? "is" : "are")} already {(add ? "added to" : "removed from")} `{name}`"
				: null;
			return ReplyTimedAsync(new[] { success, failure }.JoinNonNullStrings("\n"));
		}
		/// <summary>
		/// Modifies the matching values. This will only add if a value is not found and <paramref name="creationFactory"/> is not null.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="selector"></param>
		/// <param name="predicate"></param>
		/// <param name="creationFactory"></param>
		/// <param name="updateCallback"></param>
		/// <returns></returns>
		protected Task ModifyCollectionValuesAsync<TValue>(
			Expression<Func<TSettings, ICollection<TValue>>> selector,
			Func<TValue, bool> predicate,
			Func<TValue> creationFactory,
			Func<TValue, string> updateCallback)
		{
			var (settings, setting, list, name) = GetCollection(selector);

			var matchingValues = list.Where(predicate).ToList();
			if (matchingValues.Count == 0)
			{
				if (creationFactory == null)
				{
					return ReplyErrorAsync("Unable to create a new value to insert.");
				}
				var newValue = creationFactory();
				matchingValues.Add(newValue);
				list.Add(newValue);
			}

			var response = new StringBuilder();
			for (int i = 0; i < matchingValues.Count; ++i)
			{
				response.AppendLineFeed(updateCallback(matchingValues[i]));
			}
			return ReplyAsync(response.ToString());
		}
		/// <summary>
		/// Sets the property to the supplied value while also firing <see cref="ISettingsBase.RaisePropertyChanged(string)"/> and sending a response in Discord.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="selector"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		protected Task ModifyAsync<TValue>(Expression<Func<TSettings, TValue>> selector, TValue value)
		{
			var (settings, setting, currentValue, name) = GetValue(selector);
			//If the same value is passed in, return a message to the user that they're the same
			if (setting.EqualityComparer.Equals(currentValue, value))
			{
				return ReplyTimedAsync($"The passed in value for `{name}` matches the current value: {FormatValue(currentValue)}.");
			}
			var valid = setting.Validation(value);
			if (!valid.IsSuccess)
			{
				return ReplyTimedAsync($"The supplied value `{FormatValue(value)}` is invalid for `{name}`");
			}

			setting.SetValue(value);
			settings.RaisePropertyChanged(name);
			return ReplyTimedAsync($"Successfully set `{name}` to {FormatValue(value)}. Previous value was: {FormatValue(currentValue)}.");
		}
		/// <summary>
		/// Returns the value as a string.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private string FormatValue(object? value)
			=> Settings.FormatValue(Context.Client, Context.Guild, value);
		private SettingContext<TSettings, Setting<T>, T> GetValue<T>(Expression<Func<TSettings, T>> selector)
			=> new SettingContext<TSettings, Setting<T>, T>(Settings, selector);
		private SettingContext<TSettings, CollectionSetting<T>, ICollection<T>> GetCollection<T>(Expression<Func<TSettings, ICollection<T>>> selector)
			=> new SettingContext<TSettings, CollectionSetting<T>, ICollection<T>>(Settings, selector);

		private class SettingContext<TParentSettings, TSetting, TValue>
			where TParentSettings : IParsable
			where TSetting : ISetting
		{
			public TParentSettings Settings { get; }
			public TSetting Setting { get; }
			public TValue Value { get; }

			public SettingContext(TParentSettings parent, Expression<Func<TSettings, TValue>> selector)
			{
				Settings = parent;
				var name = selector.GetMemberExpression().Member.Name;
				Setting = (TSetting)Settings.SettingParser.GetSetting(name, PrefixState.NotPrefixed);
				Value = (TValue)Setting.GetValue();
			}

			public void Deconstruct(out TParentSettings settings, out TSetting setting, out TValue value, out string name)
			{
				settings = Settings;
				setting = Setting;
				value = Value;
				name = Setting.MainName;
			}
		}
	}
}