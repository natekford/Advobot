using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using ReactiveUI;
using Advobot.SharedUI.Colors;
using Advobot.NetCoreUI.Classes.Colors;
using Avalonia.Controls;
using Avalonia.Data;

namespace Advobot.NetCoreUI.Classes.ViewModels
{
	public class ColorsViewModel<TBrush> : SettingsViewModel
	{
		private readonly IColorSettings<TBrush> _Colors;

		public ColorsViewModel(IColorSettings<TBrush> colors) : base(colors)
		{
			_Colors = colors;
		}
	}
}
