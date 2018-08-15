using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Advobot.Windows.Classes.Controls
{
	/// <summary>
	/// Interaction logic for AdvobotNumberBox.xaml
	/// </summary>
	public partial class AdvobotNumberBox : AdvobotTextBox
	{
		private static Regex _NumberRegex = new Regex(@"[^\d-]", RegexOptions.Compiled);

		/// <summary>
		/// Indicates the default value for this instance.
		/// </summary>
		public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(int), typeof(AdvobotNumberBox), new PropertyMetadata(0));
		/// <summary>
		/// The default value for this instance.
		/// </summary>
		public int DefaultValue
		{
			get => (int)GetValue(DefaultValueProperty);
			set => SetValue(DefaultValueProperty, value);
		}
		/// <summary>
		/// Indicates the biggest number allowed.
		/// </summary>
		public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(int), typeof(AdvobotNumberBox), new PropertyMetadata(int.MaxValue, UpdateMaxLength));
		/// <summary>
		/// The biggest number allowed.
		/// </summary>
		public int MaxValue
		{
			get => (int)GetValue(MaxValueProperty);
			set => SetValue(MaxValueProperty, value);
		}
		/// <summary>
		/// Indicates the smallest number allowed.
		/// </summary>
		public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("MinValue", typeof(int), typeof(AdvobotNumberBox), new PropertyMetadata(int.MinValue, UpdateMaxLength));
		/// <summary>
		/// The smallest number allowed.
		/// </summary>
		public int MinValue
		{
			get => (int)GetValue(MinValueProperty);
			set => SetValue(MinValueProperty, value);
		}
		private static readonly DependencyPropertyKey StoredValuePropertyKey = DependencyProperty.RegisterReadOnly("StoredValue", typeof(int), typeof(AdvobotNumberBox), new PropertyMetadata(0, UpdateText));
		/// <summary>
		/// Indicates the current value.
		/// </summary>
		public static readonly DependencyProperty StoredValueProperty = StoredValuePropertyKey.DependencyProperty;
		/// <summary>
		/// The current value.
		/// </summary>
		public int StoredValue
		{
			get => (int)GetValue(StoredValueProperty);
			set => SetValue(StoredValuePropertyKey, value);
		}

		/// <summary>
		/// Creates an instance of <see cref="AdvobotNumberBox"/>.
		/// </summary>
		public AdvobotNumberBox()
		{
			InitializeComponent();
			DataObject.AddPastingHandler(this, OnPaste);
		}

		/// <inheritdoc />
		public override void EndInit()
		{
			base.EndInit();
			if (String.IsNullOrWhiteSpace(Text))
			{
				Text = DefaultValue.ToString();
			}
		}
		private void OnTextChanged(object sender, TextChangedEventArgs e)
		{
			//Update the stored value
			UpdateStoredValue(int.TryParse(((TextBox)sender).Text, out var result) ? result : DefaultValue);
		}
		private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !String.IsNullOrWhiteSpace(e.Text) && _NumberRegex.IsMatch(e.Text);
		}
		private void OnPaste(object sender, DataObjectPastingEventArgs e)
		{
			if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true))
			{
				return;
			}

			var input = e.SourceDataObject.GetData(DataFormats.UnicodeText).ToString();
			var nums = _NumberRegex.Replace(input, "");

			//Append the text in the correct part of the string
			var sb = new StringBuilder();
			var tb = (TextBox)e.Source;
			for (var i = 0; i < tb.MaxLength; ++i)
			{
				if (i < tb.CaretIndex)
				{
					sb.Append(tb.Text[i]);
				}
				else if (i < tb.CaretIndex + nums.Length)
				{
					sb.Append(nums[i - tb.CaretIndex]);
				}
				else if (i < tb.Text.Length + nums.Length)
				{
					sb.Append(tb.Text[i - nums.Length]);
				}
			}
			tb.Text = sb.ToString();
			tb.CaretIndex = tb.Text.Length;

			e.CancelCommand();
		}
		private void OnUpButtonClick(object sender, RoutedEventArgs e)
		{
			if (StoredValue < MaxValue)
			{
				UpdateStoredValue(StoredValue + 1);
			}
		}
		private void OnDownButtonClick(object sender, RoutedEventArgs e)
		{
			if (StoredValue > MinValue)
			{
				UpdateStoredValue(StoredValue - 1);
			}
		}
		private void UpdateStoredValue(int value)
		{
			value = Math.Max(MinValue, Math.Min(MaxValue, value));
			if (StoredValue != value)
			{
				StoredValue = value;
			}
		}
		private static void UpdateMaxLength(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var nb = (AdvobotNumberBox)d;
			nb.MaxLength = Math.Max(nb.MinValue.ToString().Length, nb.MaxValue.ToString().Length);
		}
		private static void UpdateText(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((AdvobotNumberBox)d).Text = e.NewValue.ToString();
		}
	}
}