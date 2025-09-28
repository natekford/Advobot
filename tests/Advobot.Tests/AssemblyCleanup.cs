namespace Advobot.Tests;

[TestClass]
public static class AssemblyCleanup
{
	[AssemblyCleanup]
	public static void Cleanup()
	{
		try
		{
			var directory = Path.Combine(Environment.CurrentDirectory, "TestDatabases");
			if (Directory.Exists(directory))
			{
				Directory.Delete(path: directory, recursive: true);
			}
		}
		catch
		{
		}
	}
}