using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Tests;

[TestClass]
public static class AssemblyCleanup
{
	[AssemblyCleanup]
	public static void Cleanup()
		=> Directory.Delete(Path.Combine(Environment.CurrentDirectory, "TestDatabases"), true);
}