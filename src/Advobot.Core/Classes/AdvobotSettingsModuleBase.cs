using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
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
		private static readonly Dictionary<Type, object> _Comparers = new Dictionary<Type, object>();

		/// <summary>
		/// Registers the equality comparer so it can be used when modifying values/lists.
		/// </summary>
		/// <param name="comparer"></param>
		public static void RegisterEqualityComparer<T>(IEqualityComparer<T> comparer)
			=> _Comparers.Add(typeof(T), comparer);
		/// <summary>
		/// Gets the equality comparer registered for the specified type.
		/// If no equality comparer is found, will return the default one.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IEqualityComparer<T> GetEqualityComparer<T>()
			=> _Comparers.TryGetValue(typeof(T), out var comparer) ? (IEqualityComparer<T>)comparer : EqualityComparer<T>.Default;
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
			if (!(await VerifyAsync(settings, settingName).CAF() is ISetting property))
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
			if (!(await VerifyAsync(settings, settingName).CAF() is ISetting setting))
			{
				return;
			}
			settings.ResetSetting(setting.Name);
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully reset `{setting.Name}`.").CAF();
		}
		/// <summary>
		/// Adds or removes the specified objects from the list while also firing <see cref="ISettingsBase.RaisePropertyChanged(string)"/> and sending a response in Discord.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="propertySelector"></param>
		/// <param name="add"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		protected async Task ModifyCollectionAsync<TValue>(
			Expression<Func<TSettings, ICollection<TValue>>> propertySelector,
			bool add,
			IEnumerable<TValue> values)
		{
			var settings = GetSettings();
			var prop = GetProperty(propertySelector);
			var list = propertySelector.Compile()(settings);

			var failures = new List<TValue>();
			var successes = new List<TValue>();
			var comparer = GetEqualityComparer<TValue>();
			foreach (var value in values)
			{
				//Not in and trying to remove = failure
				//Is in and trying to add = failure
				//This should function the same as .Contains() then .Single() afterwards, except this only iterates once
				var obj = list.SingleOrDefault(x => comparer.Equals(x, value));
				if (comparer.Equals(obj, value) == add)
				{
					failures.Add(value);
					continue;
				}

				if (add)
				{
					list.Add(value);
					successes.Add(value);
				}
				else
				{
					list.Remove(obj);
					successes.Add(obj);
				}
			}
			settings.RaisePropertyChanged(prop.Name);

			var success = successes.Any()
				? $"Successfully {(add ? "added" : "removed")} `{FormatValues(settings, successes)}` {(add ? "to" : "from")} `{prop.Name}`."
				: null;
			var failure = failures.Any()
				? $"`{FormatValues(settings, failures)}` {(failures.Count == 1 ? "is" : "are")} already {(add ? "added to" : "removed from")} `{prop.Name}`"
				: null;
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, new[] { success, failure }.JoinNonNullStrings("\n")).CAF();
		}
		/// <summary>
		/// Invokes <see cref="ModifyCollectionAsync{TValue}(Expression{Func{TSettings, ICollection{TValue}}}, bool, IEnumerable{TValue})"/>.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="propertySelector"></param>
		/// <param name="add"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		protected async Task ModifyCollectionAsync<TValue>(
			Expression<Func<TSettings, ICollection<TValue>>> propertySelector,
			bool add,
			params TValue[] values)
			=> await ModifyCollectionAsync(propertySelector, add, (IEnumerable<TValue>)values);
		/// <summary>
		/// Acts as <see cref="ModifyCollectionAsync{TValue}(Expression{Func{TSettings, ICollection{TValue}}}, bool, IEnumerable{TValue})"/> but with a passed in converter/validation callback.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <typeparam name="TOriginal"></typeparam>
		/// <param name="propertySelector"></param>
		/// <param name="validation"></param>
		/// <param name="add"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		protected async Task ModifyCollectionAsync<TValue, TOriginal>(
			Expression<Func<TSettings, ICollection<TValue>>> propertySelector,
			Func<TOriginal, (bool Valid, TValue Converted)> validation,
			bool add,
			IEnumerable<TOriginal> values)
		{
			var validValues = new List<TValue>();
			foreach (var value in values)
			{
				var (Valid, Converted) = validation(value);
				if (Valid)
				{
					validValues.Add(Converted);
					continue;
				}
				await SendValidationError(value, GetProperty(propertySelector)).CAF();
				return;
			}
			await ModifyCollectionAsync(propertySelector, add, validValues.ToArray()).CAF();
		}
		/// <summary>
		/// Sets the property to the supplied value while also firing <see cref="ISettingsBase.RaisePropertyChanged(string)"/> and sending a response in Discord.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="propertySelector"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		protected async Task ModifyAsync<TValue>(
			Expression<Func<TSettings, TValue>> propertySelector,
			TValue value)
		{
			//This uses Expression<Func<TSettings, T>> instead of Action<TSettings, T> so the property name can be gotten from the same arg
			var settings = GetSettings();
			var prop = GetProperty(propertySelector);
			var currentValue = propertySelector.Compile()(settings);
			//If the same value is passed in, return a message to the user that they're the same
			if (GetEqualityComparer<TValue>().Equals(currentValue, value))
			{
				var same = $"The passed in value for `{prop.Name}` is the current value: {FormatValue(settings, currentValue)}.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, same).CAF();
				return;
			}

			prop.SetValue(settings, value);
			settings.RaisePropertyChanged(prop.Name);

			var resp = $"Successfully set `{prop.Name}` to {FormatValue(settings, value)}. " +
				$"Previous value was: {FormatValue(settings, currentValue)}.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		/// <summary>
		/// Acts as <see cref="ModifyAsync{TValue}(Expression{Func{TSettings, TValue}}, TValue)"/> but with a passed in converter/validation callback.
		/// Any errors during conversion/validation will lead to sending an error message.
		/// </summary>
		/// <typeparam name="TValue">The type to convert to.</typeparam>
		/// <typeparam name="TOriginal">The passed in type.</typeparam>
		/// <param name="propertySelector"></param>
		/// <param name="validation"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		protected async Task ModifyAsync<TValue, TOriginal>(
			Expression<Func<TSettings, TValue>> propertySelector,
			Func<TOriginal, (bool Valid, TValue Converted)> validation,
			TOriginal value)
		{
			var (Valid, Converted) = validation(value);
			if (!Valid)
			{
				await SendValidationError(value, GetProperty(propertySelector)).CAF();
				return;
			}
			await ModifyAsync(propertySelector, Converted).CAF();
		}
		/// <summary>
		/// Returns the settings this module is targetting.
		/// </summary>
		/// <returns></returns>
		protected abstract TSettings GetSettings();
		/// <summary>
		/// Gets the targetted property from the expression.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="propertySelector"></param>
		/// <returns></returns>
		private PropertyInfo GetProperty<TValue>(Expression<Func<TSettings, TValue>> propertySelector)
		{
			var expr = (MemberExpression)propertySelector.Body;
			return (PropertyInfo)expr.Member;
		}
		/// <summary>
		/// Returns the values as a string.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="settings"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		private string FormatValues<TValue>(TSettings settings, IEnumerable<TValue> values)
			=> string.Join("`, `", values.Select(x => FormatValue(settings, x)));
		/// <summary>
		/// Returns the value as a string.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private string FormatValue(TSettings settings, object value)
			=> settings.FormatValue(Context.Client, Context.Guild, value);
		/// <summary>
		/// Sends an error to the discord channel saying the passed in value is invalid.
		/// </summary>
		/// <typeparam name="TOriginal"></typeparam>
		/// <param name="value"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		private async Task SendValidationError<TOriginal>(TOriginal value, PropertyInfo property)
		{
			var error = new Error($"The supplied value `{FormatValue(GetSettings(), value)}` is invalid for `{property.Name}`");
			await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
		}
		/// <summary>
		/// Makes sure the settings exist, otherwise prints out to the channel that they don't.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		private async Task<ISetting> VerifyAsync(TSettings settings, string name)
		{
			if (!settings.GetSettings().TryGetValue(name, out var property))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"`{name}` is not a valid setting.")).CAF();
				return null;
			}
			return property;
		}
	}

	/// <summary>
	/// Handles saving settings in addition to other things.
	/// </summary>
	/// <typeparam name="TSettings"></typeparam>
	public abstract class AdvobotSettingsSavingModuleBase<TSettings> : AdvobotSettingsModuleBase<TSettings> where TSettings : ISettingsBase
	{
		/// <summary>
		/// Saves the settings then calls the base <see cref="ModuleBase{T}.AfterExecute(CommandInfo)"/>.
		/// </summary>
		/// <param name="command"></param>
		protected override void AfterExecute(CommandInfo command)
		{
			GetSettings().SaveSettings(Context.BotSettings);
			base.AfterExecute(command);
		}
	}
}