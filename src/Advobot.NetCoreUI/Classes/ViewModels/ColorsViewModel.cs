using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using ReactiveUI;
using Advobot.SharedUI.Colors;
using Advobot.NetCoreUI.Classes.Colors;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Advobot.NetCoreUI.Classes.ValidationAttributes;

namespace Advobot.NetCoreUI.Classes.ViewModels
{
	public class ColorsViewModel : SettingsViewModel
	{
        public ColorTheme Theme
        {
            get => _Colors.Theme;
            set => _Colors.Theme = value;
        }
        [ColorValidation]
        public string BaseBackground
        {
            get => IsValid() ? _Factory.FormatBrush(_Colors.UserDefinedColors[nameof(BaseBackground)]) : _BaseBackground;
            set => RaiseAndSetIfChangedAndValid(v => _Colors.UserDefinedColors[nameof(BaseBackground)] = _Factory.CreateBrush(v), ref _BaseBackground, value, new ColorValidationAttribute());
        }
        private string _BaseBackground;

        //TODO: fix this and colors getting saved as names
        [ColorValidation]
        public string this[string target]
        {
            get => IsValid(target) ? _Factory.FormatBrush(_Colors.UserDefinedColors[target]) : _Throwaway;
            set => RaiseAndSetIfChangedAndValid(v => _Colors.UserDefinedColors[target] = _Factory.CreateBrush(v), ref _Throwaway, value, new ColorValidationAttribute());
        }
        private string _Throwaway;

		private readonly IColorSettings<SolidColorBrush> _Colors;
        private readonly NetCoreBrushFactory _Factory = new NetCoreBrushFactory();

        public ColorsViewModel(IColorSettings<SolidColorBrush> colors) : base(colors)
		{
			_Colors = colors;
		}
	}
}
