using System;
using System.IO;
using System.Text;
using System.Windows.Input;

namespace Advobot.UI.Controls
{
	public sealed class TextBoxStreamWriter : TextWriter
	{
		private readonly ICommand _Command;
		private readonly StringBuilder _CurrentLineText = new();

		public override Encoding Encoding => Encoding.UTF32;

		public TextBoxStreamWriter(ICommand command)
		{
			_Command = command;
		}

		public override void Write(char value)
		{
			if (value.Equals('\n'))
			{
				Write(_CurrentLineText.ToString());
				_CurrentLineText.Clear();
			}
			_CurrentLineText.Append(value);
		}

		public override void Write(string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return;
			}

			_Command.Execute(value + Environment.NewLine);
		}
	}
}