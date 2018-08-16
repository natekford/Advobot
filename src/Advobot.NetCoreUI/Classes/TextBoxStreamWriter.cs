using System;
using System.IO;
using System.Text;
using System.Windows.Input;

namespace Advobot.NetCoreUI.Classes
{
	internal class TextBoxStreamWriter : TextWriter
	{
		private readonly ICommand _Command;
		private readonly StringBuilder _CurrentLineText = new StringBuilder();

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
		public override void Write(string value)
		{
			if (String.IsNullOrWhiteSpace(value))
			{
				return;
			}

			_Command.Execute(Environment.NewLine + value);
		}
	}
}