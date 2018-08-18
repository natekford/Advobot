using System;
using System.Windows.Input;

namespace Advobot.NetFrameworkUI.Classes
{
	/// <summary>
	/// A command for the UI.
	/// </summary>
	public class Command : ICommand
	{
		private readonly Action<object> _Execute;
		private readonly Func<object, bool> _CanExecute;

		/// <inheritdoc />
		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		/// <summary>
		/// Creates an instance of <see cref="Command"/>.
		/// </summary>
		/// <param name="execute"></param>
		/// <param name="canExecute"></param>
		public Command(Action<object> execute, Func<object, bool> canExecute = null)
		{
			_Execute = execute;
			_CanExecute = canExecute;
		}

		/// <inheritdoc />
		public bool CanExecute(object parameter)
		{
			return _CanExecute?.Invoke(parameter) ?? true;
		}
		/// <inheritdoc />
		public void Execute(object parameter)
		{
			_Execute?.Invoke(parameter);
		}
	}

	/// <summary>
	/// A generic command for the UI.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Command<T> : ICommand
	{
		private readonly Action<T> _Execute;
		private readonly Func<T, bool> _CanExecute;

		/// <inheritdoc />
		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		/// <summary>
		/// Creates an instance of <see cref="Command"/>.
		/// </summary>
		/// <param name="execute"></param>
		/// <param name="canExecute"></param>
		public Command(Action<T> execute, Func<T, bool> canExecute = null)
		{
			_Execute = execute;
			_CanExecute = canExecute;
		}

		/// <inheritdoc />
		public bool CanExecute(object parameter)
		{
			return _CanExecute?.Invoke((T)parameter) ?? true;
		}
		/// <inheritdoc />
		public void Execute(object parameter)
		{
			_Execute?.Invoke((T)parameter);
		}
	}
}
