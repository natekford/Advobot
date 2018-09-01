using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Advobot.Interfaces;
using Advobot.NetCoreUI.Classes.ValidationAttributes;
using Avalonia.Threading;
using Discord;

namespace Advobot.NetCoreUI.Classes.ViewModels
{
	public sealed class BotSettingsViewModel : SettingsViewModel
	{
		public bool AlwaysDownloadUsers
		{
			get => _BotSettings.AlwaysDownloadUsers;
			set => _BotSettings.AlwaysDownloadUsers = value;
		}
		//When validation is used these weird methods and useless field have to be used.
		[PrefixValidation]
		public string Prefix
		{
			get => IsValid() ? _BotSettings.Prefix : _Prefix;
			set => RaiseAndSetIfChangedAndValid(v => _BotSettings.Prefix = v, ref _Prefix, value, new PrefixValidationAttribute());
		}
		private string _Prefix;
		public string Game
		{
			get => _BotSettings.Game;
			set => _BotSettings.Game = value;
		}
		[TwitchStreamValidation]
		public string Stream
		{
			get => IsValid() ? _BotSettings.Stream : _Stream;
			set => RaiseAndSetIfChangedAndValid(v => _BotSettings.Stream = v, ref _Stream, value, new TwitchStreamValidationAttribute());
		}
		private string _Stream;
		public int MessageCacheSize
		{
			get => _BotSettings.MessageCacheSize;
			set => _BotSettings.MessageCacheSize = value;
		}
		public int MaxUserGatherCount
		{
			get => _BotSettings.MaxUserGatherCount;
			set => _BotSettings.MaxUserGatherCount = value;
		}
		public int MaxMessageGatherSize
		{
			get => _BotSettings.MaxMessageGatherSize;
			set => _BotSettings.MaxMessageGatherSize = value;
		}
		public int MaxRuleCategories
		{
			get => _BotSettings.MaxRuleCategories;
			set => _BotSettings.MaxRuleCategories = value;
		}
		public int MaxRulesPerCategory
		{
			get => _BotSettings.MaxRulesPerCategory;
			set => _BotSettings.MaxRulesPerCategory = value;
		}
		public int MaxSelfAssignableRoleGroups
		{
			get => _BotSettings.MaxSelfAssignableRoleGroups;
			set => _BotSettings.MaxSelfAssignableRoleGroups = value;
		}
		public int MaxQuotes
		{
			get => _BotSettings.MaxQuotes;
			set => _BotSettings.MaxQuotes = value;
		}
		public int MaxBannedStrings
		{
			get => _BotSettings.MaxBannedStrings;
			set => _BotSettings.MaxBannedStrings = value;
		}
		public int MaxBannedRegex
		{
			get => _BotSettings.MaxBannedRegex;
			set => _BotSettings.MaxBannedRegex = value;
		}
		public int MaxBannedNames
		{
			get => _BotSettings.MaxBannedNames;
			set => _BotSettings.MaxBannedNames = value;
		}
		public int MaxBannedPunishments
		{
			get => _BotSettings.MaxBannedPunishments;
			set => _BotSettings.MaxBannedPunishments = value;
		}
		public LogSeverity LogLevel
		{
			get => _BotSettings.LogLevel;
			set => _BotSettings.LogLevel = value;
		}
		public IList<ulong> TrustedUsers { get; } = new ObservableCollection<ulong>();
		public IList<ulong> UsersUnableToDmOwner { get; } = new ObservableCollection<ulong>();
		public IList<ulong> UsersIgnoredFromCommands { get; } = new ObservableCollection<ulong>();

		private readonly IBotSettings _BotSettings;

		public BotSettingsViewModel(IBotSettings botSettings) : base(botSettings)
		{
			_BotSettings = botSettings;

			UseObservableCollectionFromCorrectThread(_BotSettings.TrustedUsers, TrustedUsers);
			UseObservableCollectionFromCorrectThread(_BotSettings.UsersUnableToDmOwner, UsersUnableToDmOwner);
			UseObservableCollectionFromCorrectThread(_BotSettings.UsersIgnoredFromCommands, UsersIgnoredFromCommands);
		}

		private static void UseObservableCollectionFromCorrectThread<T>(IList<T> source, IList<T> destination)
			=> UseObservableCollectionFromCorrectThread((ObservableCollection<T>)source, destination);
		private static void UseObservableCollectionFromCorrectThread<T, TSource>(TSource source, IList<T> destination)
			where TSource : IList<T>, INotifyCollectionChanged
		{
			//Start by making the lists have the same items
			foreach (var item in source)
			{
				destination.Add(item);
			};
			//Then do this so the ui list will update with the settings list
			source.CollectionChanged += (sender, e) =>
			{
				//Get invalid calling thread if this isn't used.
				Dispatcher.UIThread.InvokeAsync(() =>
				{
					switch (e.Action)
					{
						case NotifyCollectionChangedAction.Add:
							foreach (T item in e.NewItems)
							{
								destination.Add(item);
							}
							return;
						case NotifyCollectionChangedAction.Remove:
							foreach (T item in e.OldItems)
							{
								destination.Remove(item);
							}
							return;
						default:
							throw new NotImplementedException();
					}
				});
			};
		}
	}
}