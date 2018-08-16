using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Advobot.NetFrameworkUI.Classes
{
	internal class TextBoxStreamWriter : TextWriter
	{
		private readonly TextBoxBase _Output;
		private readonly bool _IgnoreNewLines;
		private readonly StringBuilder _CurrentLineText = new StringBuilder();

		public override Encoding Encoding => Encoding.UTF32;

		public TextBoxStreamWriter(TextBoxBase output)
		{
			_Output = output;
			_IgnoreNewLines = output is RichTextBox; //RTB will have extra new lines if they are printed out
		}

		public override void Write(char value)
		{
			//Done because crashes program without exception.
			//Could not for the life of me figure out why.
			if (value.Equals('﷽'))
			{
				return;
			}
			if (value.Equals('\n'))
			{
				Write(_CurrentLineText.ToString());
				_CurrentLineText.Clear();
			}
			_CurrentLineText.Append(value);
		}
		public override void Write(string value)
		{
			if (value == null || (_IgnoreNewLines && value.Equals('\n')))
			{
				return;
			}

			_Output.Dispatcher.InvokeAsync(() => _Output.AppendText(value), DispatcherPriority.ContextIdle);
		}
	}
}
