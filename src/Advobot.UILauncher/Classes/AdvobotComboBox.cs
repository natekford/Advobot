using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Advobot.UILauncher.Classes
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

		public static AdvobotComboBox CreateEnumComboBox<T>(string tag) where T : struct, IConvertible, IComparable, IFormattable
		{
			return new AdvobotComboBox { ItemsSource = CreateItemsSourceOutOfEnum<T>(), Tag = tag, };
		}
		public static AdvobotComboBox CreateStringComboBox(string tag, params string[] strings)
		{
			return new AdvobotComboBox { ItemsSource = CreateComboBoxSourceOutOfStrings(strings), Tag = tag, };
		}

		public static IEnumerable<TextBox> CreateItemsSourceOutOfEnum<T>() where T : struct, IConvertible, IComparable, IFormattable
		{
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
				yield break;
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
