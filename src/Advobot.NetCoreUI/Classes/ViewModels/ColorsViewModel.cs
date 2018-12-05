using System;
using System.Runtime.CompilerServices;
using Advobot.NetCoreUI.Classes.Colors;
using Advobot.NetCoreUI.Classes.ValidationAttributes;
using Advobot.NetCoreUI.Classes.AbstractUI.Colors;
using Avalonia.Media;

namespace Advobot.NetCoreUI.Classes.ViewModels
{
	public sealed class ColorsViewModel : SettingsViewModel
	{
		public ColorTheme Theme
		{
			get => _Colors.Theme;
			set => _Colors.Theme = value;
		}
		[ColorValidation]
		public string BaseBackground
		{
			get => Get();
			set => Set(value);
		}
		[ColorValidation]
		public string BaseForeground
		{
			get => Get();
			set => Set(value);
		}
		[ColorValidation]
		public string BaseBorder
		{
			get => Get();
			set => Set(value);
		}
		[ColorValidation]
		public string ButtonBackground
		{
			get => Get();
			set => Set(value);
		}
		[ColorValidation]
		public string ButtonForeground
		{
			get => Get();
			set => Set(value);
		}
		[ColorValidation]
		public string ButtonBorder
		{
			get => Get();
			set => Set(value);
		}
		[ColorValidation]
		public string ButtonDisabledBackground
		{
			get => Get();
			set => Set(value);
		}
		[ColorValidation]
		public string ButtonDisabledForeground
		{
			get => Get();
			set => Set(value);
		}
		[ColorValidation]
		public string ButtonDisabledBorder
		{
			get => Get();
			set => Set(value);
		}
		[ColorValidation]
		public string ButtonMouseOverBackground
		{
			get => Get();
			set => Set(value);
		}
		[ColorValidation]
		public string JsonDigits
		{
			get => Get();
			set => Set(value);
		}
		[ColorValidation]
		public string JsonValue
		{
			get => Get();
			set => Set(value);
		}
		[ColorValidation]
		public string JsonParamName
		{
			get => Get();
			set => Set(value);
		}
		private string _Throwaway = "";

		private readonly IColorSettings<ISolidColorBrush> _Colors;
		private readonly NetCoreBrushFactory _Factory = new NetCoreBrushFactory();
		private readonly ColorValidationAttribute _Validator = new ColorValidationAttribute();

		public ColorsViewModel(IColorSettings<ISolidColorBrush> colors) : base(colors)
		{
			_Colors = colors;
		}

		private string Get([CallerMemberName] string propertyName = "")
		{
			return IsValid(propertyName)
				? _Factory.FormatBrush(_Colors.UserDefinedColors[propertyName])
				: _Throwaway;
		}
		private void Set(string newValue, [CallerMemberName] string propertyName = "")
		{
			var setter = new Action<string>(v => _Colors.UserDefinedColors[propertyName] = _Factory.CreateBrush(v));
			RaiseAndSetIfChangedAndValid(setter, ref _Throwaway, newValue, _Validator, propertyName);
		}
	}
}
