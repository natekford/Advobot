using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Advobot.Interfaces;
using ReactiveUI;

namespace Advobot.NetCoreUI.Classes.ViewModels
{
	public class SettingsViewModel : ReactiveObject
	{
		private readonly Dictionary<string, bool> _ValidProperties = new Dictionary<string, bool>();

		public SettingsViewModel(ISettingsBase settings)
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
			this.RaiseAndSetIfChanged(ref backingField, newValue, propertyName);
		}
	}
}