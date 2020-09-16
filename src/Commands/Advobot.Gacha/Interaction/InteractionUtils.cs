using System.Reflection;

namespace Advobot.Gacha.Interaction
{
	public static class InteractionUtils
	{
		public static string GetRepresentation(this InteractionType value, bool useReactions)
		{
			if (!useReactions)
			{
				return value.ToString();
			}

			var field = typeof(InteractionType).GetField(value.ToString());
			var attr = field?.GetCustomAttribute<UnicodeRepresentationAttribute>();
			if (attr is null)
			{
				return value.ToString();
			}
			return attr.Name;
		}
	}
}