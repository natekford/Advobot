using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using ReactiveUI;

namespace Advobot.NetCoreUI.Classes.ViewModels
{
	public abstract class SettingsViewModel : ReactiveObject
	{
		public bool CanSave => Errors.AllValid();

		public ValidationErrors Errors { get; } = new ValidationErrors();

		public SettingsViewModel(INotifyPropertyChanged settings)
		{
			settings.PropertyChanged += (sender, e) =>
			{
				Errors[e.PropertyName] = false;
				this.RaisePropertyChanged(e.PropertyName);
				this.RaisePropertyChanged(nameof(CanSave));
			};
			Errors.PropertyChanged += (sender, e) =>
			{
				this.RaisePropertyChanged(nameof(CanSave));
			};
		}

		protected bool IsValid([CallerMemberName] string propertyName = null)
			=> !Errors[propertyName];
		protected void RaiseAndSetIfChangedAndValid<T>(Action<T> setter, ref T backingField, T newValue, ValidationAttribute validation, [CallerMemberName] string propertyName = null)
		{
			var isValid = validation.IsValid(newValue);
			Errors[propertyName] = !isValid;
			if (isValid)
			{
				setter(newValue);
			}
			backingField = newValue;
			this.RaisePropertyChanged(propertyName);
			this.RaisePropertyChanged(nameof(CanSave));
		}

		public sealed class ValidationErrors : INotifyPropertyChanged
		{
			private readonly ConcurrentDictionary<string, bool> _ValidationErrors = new ConcurrentDictionary<string, bool>();

			public event PropertyChangedEventHandler PropertyChanged;

			public bool this[string target]
			{
				//Default to false since any values loaded from file have to be valid
				get => _ValidationErrors.GetOrAdd(target, false);
				set
				{
					_ValidationErrors.AddOrUpdate(target, value, (k, v) => value);
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(target));
				}
			}

			public bool AllValid()
				=> _ValidationErrors.Values.All(x => !x);
		}
	}
}