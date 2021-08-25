using System.Reflection;

using Avalonia.Markup.Xaml;

namespace Advobot.UI.Extensions
{
	/// <summary>
	/// Acts as nameof(x) for XAML.
	/// </summary>
	public sealed class NameOfExtension : MarkupExtension
	{
		/// <summary>
		/// The name to make sure exists.
		/// </summary>
		[ConstructorArgument("member")]
		public string? Member { get; set; }

		/// <summary>
		/// The type to check the name from.
		/// </summary>
		public Type? Type { get; set; }

		public NameOfExtension()
		{
		}

		public NameOfExtension(string member)
		{
			Member = member;
		}

		/// <summary>
		/// Checks if the name exists on the type.
		/// </summary>
		/// <param name="serviceProvider"></param>
		/// <returns></returns>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
			{
				throw new ArgumentNullException(nameof(serviceProvider));
			}
			if (Type == null || string.IsNullOrEmpty(Member) || Member.Contains("."))
			{
				throw new ArgumentException("Syntax for x:NameOf is Type={x:Type [className]} Member=[propertyName]");
			}
			var pinfo = Type.GetRuntimeProperties().FirstOrDefault(pi => pi.Name == Member);
			var finfo = Type.GetRuntimeFields().FirstOrDefault(fi => fi.Name == Member);
			if (pinfo == null && finfo == null)
			{
				throw new ArgumentException($"No property or field found for {Member} in {Type}");
			}
			return Member;
		}
	}
}