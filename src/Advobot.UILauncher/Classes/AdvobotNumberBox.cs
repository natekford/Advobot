using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Advobot.UILauncher.Classes
{
	internal class AdvobotNumberBox : AdvobotTextBox
	{
		private static Regex _NumberRegex = new Regex(@"[^\d]", RegexOptions.Compiled);

		public AdvobotNumberBox()
		{
			this.PreviewTextInput += Validate;
			DataObject.AddPastingHandler(this, Validate);
		}

		private void Validate(object sender, TextCompositionEventArgs e)
		{
			e.Handled = true
				&& !String.IsNullOrWhiteSpace(e.Text) 
				&& !char.IsDigit(e.Text, Math.Min(0, e.Text.Length - 1));
		}
		private void Validate(object sender, DataObjectPastingEventArgs e)
		{
			if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true))
			{
				return;
			}

			var source = e.Source as TextBox;
			var input = e.SourceDataObject.GetData(DataFormats.UnicodeText).ToString();
			var nums = _NumberRegex.Replace(input, "");

			//Append the text in the correct part of the string
			var sb = new StringBuilder();
			for (int i = 0; i < source.MaxLength; ++i)
			{
				if (i < source.CaretIndex)
				{
					sb.Append(source.Text[i]);
				}
				else if (i < source.CaretIndex + nums.Length)
				{
					sb.Append(nums[i - source.CaretIndex]);
				}
				else if (i < source.Text.Length + nums.Length)
				{
					sb.Append(source.Text[i - nums.Length]);
				}
			}
			source.Text = sb.ToString();
			source.CaretIndex = source.Text.Length;

			e.CancelCommand();
		}
	}
}
