using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Advobot.NetCoreUI.Classes.ViewModels
{
	public class SettingsViewModel : ReactiveObject
	{
		private readonly Dictionary<string, bool> _ValidProperties = new Dictionary<string, bool>();

		public SettingsViewModel(INotifyPropertyChanged settings)
		{
			settings.PropertyChanged += (sender, e) =>
			{
				_ValidProperties[e.PropertyName] = true;
				this.RaisePropertyChanged(e.PropertyName);
			};
		}

		protected bool IsValid([CallerMemberName] string propertyName = null)
		{
			if (!_ValidProperties.TryGetValue(propertyName, out var val))
			{
				//Default to true since any values loaded from file have to be valid
				_ValidProperties.Add(propertyName, val = true);
			}
			return val;
		}
		protected void RaiseAndSetIfChangedAndValid<T>(Action<T> setter, ref T backingField, T newValue, ValidationAttribute validation, [CallerMemberName] string propertyName = null)
		{
			if (_ValidProperties[propertyName] = validation.IsValid(newValue))
			{
				setter(newValue);
			}
			//If same value, don't bother setting it, just say it was changed
			if (EqualityComparer<T>.Default.Equals(backingField, newValue))
			{
				this.RaisePropertyChanged(propertyName);
			}
			else
			{
				this.RaiseAndSetIfChanged(ref backingField, newValue, propertyName);
			}
		}
	}
}