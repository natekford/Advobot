using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Advobot.UILauncher.Classes.Controls
{
	internal class AdvobotComboBox : ComboBox, IFontResizeValue
	{
		private double _FRV;
		public double FontResizeValue
		{
			get => _FRV;
			set
			{
				UIModification.SetFontResizeProperty(this, value);
				_FRV = value;
			}
		}
		private Type _SET;
		public Type SourceEnumType
		{
			get => _SET;
			set
			{
				this.ItemsSource = CreateItemsSourceOutOfEnum(value);
				_SET = value;
			}
		}

		public AdvobotComboBox()
		{
			this.VerticalContentAlignment = VerticalAlignment.Center;
		}

		public static IEnumerable<TextBox> CreateItemsSourceOutOfEnum<T>() where T : struct, IConvertible, IComparable, IFormattable
		{
			if (!typeof(T).IsEnum)
			{
				throw new ArgumentException($"{typeof(T).Name} must be an enum.");
			}

			foreach (T e in Enum.GetValues(typeof(T)))
			{
				yield return new AdvobotTextBox
				{
					Text = Enum.GetName(typeof(T), e),
					Tag = e,
					IsReadOnly = true,
					IsHitTestVisible = false,
					BorderThickness = new Thickness(0),
					Background = Brushes.Transparent,
					Foreground = Brushes.Black,
				};
			}
		}
		public static IEnumerable<TextBox> CreateItemsSourceOutOfEnum(Type enumType)
		{
			if (!enumType.IsEnum)
			{
				throw new ArgumentException($"{nameof(enumType)} must be an enum.");
			}

			foreach (var e in Enum.GetValues(enumType))
			{
				yield return new AdvobotTextBox
				{
					Text = Enum.GetName(enumType, e),
					Tag = e,
					IsReadOnly = true,
					IsHitTestVisible = false,
					BorderThickness = new Thickness(0),
					Background = Brushes.Transparent,
					Foreground = Brushes.Black,
				};
			}
		}
		public static IEnumerable<TextBox> CreateComboBoxSourceOutOfStrings(params string[] strings)
		{
			foreach (var s in strings)
			{
				yield return new AdvobotTextBox
				{
					Text = s,
					Tag = s,
					IsReadOnly = true,
					IsHitTestVisible = false,
					BorderThickness = new Thickness(0),
					Background = Brushes.Transparent,
					Foreground = Brushes.Black,
				};
			}
		}
	}
}
