using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace Advobot.NetCoreUI.Classes.Controls
{
	public class NumberBox : UserControl
	{
		public static readonly DirectProperty<NumberBox, int> StoredValueProperty =
			AvaloniaProperty.RegisterDirect<NumberBox, int>(
				nameof(StoredValue),
				o => o.StoredValue,
				(o, v) => o.StoredValue = v,
				defaultBindingMode: BindingMode.TwoWay,
				enableDataValidation: true);
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
				SetAndRaise(StoredValueProperty, ref _StoredValue, value);
			}
		}
		private int _StoredValue;

		public static readonly StyledProperty<int> MaxValueProperty =
			AvaloniaProperty.Register<NumberBox, int>(nameof(MaxValue), int.MaxValue);
		public int MaxValue
		{
			get => GetValue(MaxValueProperty);
			set => SetValue(MaxValueProperty, value);
		}

		public static readonly StyledProperty<int> MinValueProperty =
			AvaloniaProperty.Register<NumberBox, int>(nameof(MinValue), int.MinValue);
		public int MinValue
		{
			get => GetValue(MinValueProperty);
			set => SetValue(MinValueProperty, value);
		}

		private ICommand ModifyValueCommand { get; }

		public NumberBox()
		{
			ModifyValueCommand = ReactiveCommand.Create<string>(x => StoredValue += int.Parse(x));
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
