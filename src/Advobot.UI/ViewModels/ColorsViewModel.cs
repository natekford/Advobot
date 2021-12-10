using Advobot.UI.AbstractUI.Colors;
using Advobot.UI.Colors;
using Advobot.UI.ValidationAttributes;

using Avalonia.Media;

using System.Runtime.CompilerServices;

namespace Advobot.UI.ViewModels;

public sealed class ColorsViewModel : SettingsViewModel
{
	private readonly IColorSettings<ISolidColorBrush> _Colors;
	private readonly NetCoreBrushFactory _Factory = new();
	private readonly ColorValidationAttribute _Validator = new();
	private string _Throwaway = "";

	[ColorValidation]
	public string BaseBackground
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
	public string BaseForeground
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
	public string ButtonDisabledBorder
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
	public string ButtonForeground
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
	public string JsonParamName
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

	public ColorTheme Theme
	{
		get => _Colors.ActiveTheme;
		set => _Colors.ActiveTheme = value;
	}

	public ColorsViewModel(IColorSettings<ISolidColorBrush> colors) : base(colors)
	{
		_Colors = colors;
	}

	private string Get([CallerMemberName] string caller = "")
	{
		return IsValid(caller)
			? _Factory.FormatBrush(_Colors.UserDefinedColors[caller])
			: _Throwaway;
	}

	private void Set(string newValue, [CallerMemberName] string caller = "")
	{
		var setter = new Action<string>(v => _Colors.UserDefinedColors[caller] = _Factory.CreateBrush(v));
		RaiseAndSetIfChangedAndValid(setter, ref _Throwaway, newValue, _Validator, caller);
	}
}