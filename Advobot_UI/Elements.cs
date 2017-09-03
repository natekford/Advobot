using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Advobot.Graphics.UserInterface
{
	internal class MyRichTextBox : RichTextBox
	{
		public MyRichTextBox()
		{
			this.Background = null;
			this.Foreground = null;
			this.BorderBrush = null;
		}
	}
	internal class MyTextBox : TextBox
	{
		public MyTextBox()
		{
			this.Background = null;
			this.Foreground = null;
			this.BorderBrush = null;
			this.TextWrapping = TextWrapping.Wrap;
		}
	}
	internal class MyButton : Button
	{
		public MyButton()
		{
			this.Background = null;
			this.Foreground = null;
			this.BorderBrush = null;
		}
	}
	internal class MyComboBox : ComboBox
	{
		public MyComboBox()
		{
			this.VerticalContentAlignment = VerticalAlignment.Center;
		}
	}
	internal class MyNumberBox : MyTextBox
	{
		public MyNumberBox()
		{
			this.PreviewTextInput += MakeSureKeyIsNumber;
			DataObject.AddPastingHandler(this, MakeSurePasteIsNumbers);
		}

		private void MakeSureKeyIsNumber(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !char.IsDigit(e.Text, e.Text.Length - 1);
		}
		private void MakeSurePasteIsNumbers(object sender, DataObjectPastingEventArgs e)
		{
			if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true))
			{
				return;
			}

			var textBeingPasted = e.SourceDataObject.GetData(DataFormats.UnicodeText).ToString();
			var onlyNums = System.Text.RegularExpressions.Regex.Replace(textBeingPasted, @"[^\d]", "", System.Text.RegularExpressions.RegexOptions.Compiled);
			this.Text = onlyNums.Substring(0, Math.Min(this.MaxLength, onlyNums.Length));
			e.CancelCommand();
		}
	}
}
