using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Advobot.UILauncher.Classes
{
	internal class TextBoxStreamWriter : TextWriter
	{
		private TextBoxBase _Output;
		private bool _IgnoreNewLines;
		private string _CurrentLineText;

		public TextBoxStreamWriter(TextBoxBase output)
		{
			_Output = output;
			//RTB will have extra new lines if they are printed out
			_IgnoreNewLines = output is RichTextBox;
		}

		public override void Write(char value)
		{
			//Done because crashes program without exception. Could not for the life of me figure out why; something in outside .dlls.
			if (value.Equals('﷽'))
			{
				return;
			}
			else if (value.Equals('\n'))
			{
				Write(_CurrentLineText);
				_CurrentLineText = null;
				return;
			}

			_CurrentLineText += value;
		}
		public override void Write(string value)
		{
			if (value == null || (_IgnoreNewLines && value.Equals('\n')))
			{
				return;
			}

			_Output.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
			{
				_Output.AppendText(value);
			}));
		}
		public override Encoding Encoding => Encoding.UTF8;
	}
}
