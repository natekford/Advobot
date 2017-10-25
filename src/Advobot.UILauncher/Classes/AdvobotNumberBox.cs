using System;
using System.Text.RegularExpressions;
using System.Windows;
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
			e.Handled = !char.IsDigit(e.Text, e.Text.Length - 1);
		}
		private void Validate(object sender, DataObjectPastingEventArgs e)
		{
			if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true))
			{
				return;
			}

			var textBeingPasted = e.SourceDataObject.GetData(DataFormats.UnicodeText).ToString();
			var onlyNums = _NumberRegex.Replace(textBeingPasted, "");
			this.Text = onlyNums.Substring(0, Math.Min(this.MaxLength, onlyNums.Length));
			e.CancelCommand();
		}
	}
}
