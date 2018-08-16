using System;
using System.Reflection;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace Advobot.NetFrameworkUI
{
	internal static class SyntaxHighlightingUtils
	{
		public static string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

		public static void LoadJsonHighlighting()
		{
			LoadSyntaxHighlighting($"{AssemblyName}.Resources.JsonSyntaxHighlighting.xshd", "Json", new[] { ".json" });
		}
		public static void LoadSyntaxHighlighting(string loc, string name, string[] extensions)
		{
			using (var r = new XmlTextReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(loc))
				?? throw new InvalidOperationException($"{loc} is missing."))
			{
				var highlighting = HighlightingLoader.Load(r, HighlightingManager.Instance);
				HighlightingManager.Inst‌​ance.RegisterHighlighting(name, extensions, highlighting);
			}
		}
	}
}
