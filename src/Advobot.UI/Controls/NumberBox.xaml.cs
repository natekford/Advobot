﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

using ReactiveUI;

using System.Windows.Input;

namespace Advobot.UI.Controls;

public class NumberBox : UserControl
{
	public static readonly DirectProperty<NumberBox, bool> HasErrorProperty =
		AvaloniaProperty.RegisterDirect<NumberBox, bool>(
			nameof(HasError),
			o => o.HasError,
			(o, v) => o.HasError = v,
			defaultBindingMode: BindingMode.OneWayToSource);

	public static readonly StyledProperty<int> MaxValueProperty =
		AvaloniaProperty.Register<NumberBox, int>(nameof(MaxValue), int.MaxValue);

	public static readonly StyledProperty<int> MinValueProperty =
		AvaloniaProperty.Register<NumberBox, int>(nameof(MinValue), int.MinValue);

	public static readonly DirectProperty<NumberBox, int> StoredValueProperty =
		AvaloniaProperty.RegisterDirect<NumberBox, int>(
			nameof(StoredValue),
			o => o.StoredValue,
			(o, v) => o.StoredValue = v,
			defaultBindingMode: BindingMode.TwoWay,
			enableDataValidation: true);

	public static readonly DirectProperty<NumberBox, string> TextProperty =
										AvaloniaProperty.RegisterDirect<NumberBox, string>(
			nameof(Text),
			o => o.Text,
			(o, v) => o.Text = v,
			defaultBindingMode: BindingMode.TwoWay,
			enableDataValidation: true);

	private bool _HasError;

	private int _StoredValue;

	private string _Text;

	public bool HasError
	{
		get => _HasError;
		private set => SetAndRaise(HasErrorProperty, ref _HasError, value);
	}

	public int MaxValue
	{
		get => GetValue(MaxValueProperty);
		set => SetValue(MaxValueProperty, value);
	}

	public int MinValue
	{
		get => GetValue(MinValueProperty);
		set => SetValue(MinValueProperty, value);
	}

	public ICommand ModifyValueCommand { get; }

	public int StoredValue
	{
		get => _StoredValue;
		set
		{
			if (value < MinValue)
			{
				throw new ArgumentException($"{nameof(StoredValue)} must be more than {MinValue}.");
			}
			if (value > MaxValue)
			{
				throw new ArgumentException($"{nameof(StoredValue)} must be less than {MaxValue}.");
			}
			if (Text != value.ToString())
			{
				Text = value.ToString();
			}
			SetAndRaise(StoredValueProperty, ref _StoredValue, value);
		}
	}

	public string Text
	{
		get => _Text;
		set
		{
			SetAndRaise(TextProperty, ref _Text, value);
			StoredValue = int.Parse(value);
		}
	}

	public NumberBox()
	{
		_Text = StoredValue.ToString();

		ModifyValueCommand = ReactiveCommand.Create<string>(x => StoredValue += int.Parse(x));
		InitializeComponent();

		var input = this.FindControl<TextBox>("Input");
		var errorBinding = DataValidationErrors.HasErrorsProperty.Bind().WithMode(BindingMode.OneWay);
		this[HasErrorProperty.Bind()] = input[errorBinding];
	}

	private void InitializeComponent()
		=> AvaloniaXamlLoader.Load(this);
}