using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;

namespace Advobot.Graphics
{
	internal class UITextBoxStreamWriter : TextWriter
	{
		private TextBoxBase _Output;
		private bool _IgnoreNewLines;
		private string _CurrentLineText;

		public UITextBoxStreamWriter(TextBoxBase output)
		{
			_Output = output;
			_IgnoreNewLines = output is RichTextBox;
		}

		public override void Write(char value)
		{
			if (value.Equals('\n'))
			{
				Write(_CurrentLineText);
				_CurrentLineText = null;
			}
			//Done because crashes program without exception. Could not for the life of me figure out why; something in outside .dlls.
			else if (value.Equals('﷽'))
			{
				return;
			}
			else
			{
				_CurrentLineText += value;
			}
		}
		public override void Write(string value)
		{
			if (value == null || (_IgnoreNewLines && value.Equals('\n')))
				return;

			_Output.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
			{
				_Output.AppendText(value);
			}));
		}
		public override Encoding Encoding
		{
			get { return Encoding.UTF8; }
		}
	}

	internal class UIFontResizer : IValueConverter
	{
		private double _ConvertFactor;

		public UIFontResizer(double convertFactor)
		{
			_ConvertFactor = convertFactor;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var converted = (int)(System.Convert.ToInt16(value) * _ConvertFactor);
			return Math.Max(converted, -1);
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
