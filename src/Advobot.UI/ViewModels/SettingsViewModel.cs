using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

using ReactiveUI;

namespace Advobot.UI.ViewModels
{
	public abstract class SettingsViewModel : ReactiveObject
	{
		public bool CanSave => Errors.AllValid();

		public ValidationErrors Errors { get; } = new ValidationErrors();

		protected SettingsViewModel(INotifyPropertyChanged? settings)
		{
			if (settings == null)
			{
				throw new ArgumentException($"Must implement {nameof(INotifyPropertyChanged)}", nameof(settings));
			}
			settings.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == null)
				{
					return;
				}

				Errors[e.PropertyName] = false;
				this.RaisePropertyChanged(e.PropertyName);
				this.RaisePropertyChanged(nameof(CanSave));
			};
			Errors.PropertyChanged += (sender, e) => this.RaisePropertyChanged(nameof(CanSave));
		}

		protected bool IsValid([CallerMemberName] string caller = "")
			=> !Errors[caller];

		protected void RaiseAndSetIfChangedAndValid<T>(Action<T> setter, ref T backingField, T newValue, ValidationAttribute validation, [CallerMemberName] string caller = "")
		{
			var isValid = validation.IsValid(newValue);
			Errors[caller] = !isValid;
			if (isValid)
			{
				setter(newValue);
			}
			backingField = newValue;
			this.RaisePropertyChanged(caller);
			this.RaisePropertyChanged(nameof(CanSave));
		}

		public sealed class ValidationErrors : INotifyPropertyChanged
		{
			private readonly ConcurrentDictionary<string, bool> _ValidationErrors = new();

			public event PropertyChangedEventHandler? PropertyChanged;

			public bool this[string target]
			{
				//Default to false since any values loaded from file have to be valid
				get => _ValidationErrors.GetOrAdd(target, false);
				set
				{
					_ValidationErrors.AddOrUpdate(target, value, (_, __) => value);
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(target));
				}
			}

			public bool AllValid()
				=> _ValidationErrors.Values.All(x => !x);
		}
	}
}