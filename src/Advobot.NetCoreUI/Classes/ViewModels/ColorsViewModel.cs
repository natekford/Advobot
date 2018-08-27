using System;
using System.Runtime.CompilerServices;
using Advobot.NetCoreUI.Classes.Colors;
using Advobot.NetCoreUI.Classes.ValidationAttributes;
using Advobot.SharedUI.Colors;
using Avalonia.Media;

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
			get => IsValid() ? Get() : _BackBackground;
			set => Set(ref _BackBackground, value);
		}
		private string _BackBackground;
		[ColorValidation]
		public string BaseForeground
		{
			get => IsValid() ? Get() : _BaseForeground;
			set => Set(ref _BaseForeground, value);
		}
		private string _BaseForeground;
		[ColorValidation]
		public string BaseBorder
		{
			get => IsValid() ? Get() : _BaseBorder;
			set => Set(ref _BaseBorder, value);
		}
		private string _BaseBorder;
		[ColorValidation]
		public string ButtonBackground
		{
			get => IsValid() ? Get() : _ButtonBackground;
			set => Set(ref _ButtonBackground, value);
		}
		private string _ButtonBackground;
		[ColorValidation]
		public string ButtonForeground
		{
			get => IsValid() ? Get() : _ButtonForeground;
			set => Set(ref _ButtonForeground, value);
		}
		private string _ButtonForeground;
		[ColorValidation]
		public string ButtonBorder
		{
			get => IsValid() ? Get() : _ButtonBorder;
			set => Set(ref _ButtonBorder, value);
		}
		private string _ButtonBorder;
		[ColorValidation]
		public string ButtonDisabledBackground
		{
			get => IsValid() ? Get() : _ButtonDisabledBackground;
			set => Set(ref _ButtonDisabledBackground, value);
		}
		private string _ButtonDisabledBackground;
		[ColorValidation]
		public string ButtonDisabledForeground
		{
			get => IsValid() ? Get() : _ButtonDisabledForeground;
			set => Set(ref _ButtonDisabledForeground, value);
		}
		private string _ButtonDisabledForeground;
		[ColorValidation]
		public string ButtonDisabledBorder
		{
			get => IsValid() ? Get() : _ButtonDisabledBorder;
			set => Set(ref _ButtonDisabledBorder, value);
		}
		private string _ButtonDisabledBorder;
		[ColorValidation]
		public string ButtonMouseOverBackground
		{
			get => IsValid() ? Get() : _ButtonMouseOverBackground;
			set => Set(ref _ButtonMouseOverBackground, value);
		}
		private string _ButtonMouseOverBackground;
		[ColorValidation]
		public string JsonDigits
		{
			get => IsValid() ? Get() : _JsonDigits;
			set => Set(ref _JsonDigits, value);
		}
		private string _JsonDigits;
		[ColorValidation]
		public string JsonValue
		{
			get => IsValid() ? Get() : _JsonValue;
			set => Set(ref _JsonValue, value);
		}
		private string _JsonValue;
		[ColorValidation]
		public string JsonParamName
		{
			get => IsValid() ? Get() : _JsonParamName;
			set => Set(ref _JsonParamName, value);
		}
		private string _JsonParamName;

		private readonly IColorSettings<SolidColorBrush> _Colors;
		private readonly NetCoreBrushFactory _Factory = new NetCoreBrushFactory();
		private readonly ColorValidationAttribute _Validator = new ColorValidationAttribute();

		public ColorsViewModel(IColorSettings<SolidColorBrush> colors) : base(colors)
		{
			_Colors = colors;
		}
		private string Get([CallerMemberName] string propertyName = "")
		{
			return _Factory.FormatBrush(_Colors.UserDefinedColors[propertyName]);
		}
		private void Set(ref string backingField, string newValue, [CallerMemberName] string propertyName = "")
		{
			var setter = new Action<string>(v => _Colors.UserDefinedColors[propertyName] = _Factory.CreateBrush(v));
			RaiseAndSetIfChangedAndValid(setter, ref backingField, newValue, _Validator, propertyName);
		}
	}
}
