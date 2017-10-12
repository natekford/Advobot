using Advobot.Actions;
using Advobot.Classes.Attributes;
using System;
using System.Linq;
using System.Reflection;

namespace Advobot
{
	public static class CommandAssembly
	{
		private static Assembly _CMD_ASSEMBLY;
		public static Assembly COMMAND_ASSEMBLY { get; } = _CMD_ASSEMBLY ?? (_CMD_ASSEMBLY = GetCommandAssembly());

		private static Assembly GetCommandAssembly()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetCustomAttribute<CommandAssemblyAttribute>() != null);
			if (!assemblies.Any())
			{
				ConsoleActions.WriteLine($"Unable to find any command assemblies. Press any key to close the program.");
				Console.ReadKey();
				throw new DllNotFoundException("Unable to find any command assemblies.");
			}
			else if (assemblies.Count() > 1)
			{
				ConsoleActions.WriteLine("Too many command assemblies found. Press any key to close the program.");
				Console.ReadKey();
				throw new InvalidOperationException("Too many command assemblies found.");
			}

			return assemblies.Single();
		}
	}
}
