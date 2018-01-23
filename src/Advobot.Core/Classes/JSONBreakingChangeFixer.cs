using System;
using System.Linq;
using System.Reflection;
using Advobot.Core.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// A converter to help manually fix a class' JSON if they get broken by a new change.
	/// </summary>
	internal class JSONBreakingChangeFixer : JsonConverter
	{
		private const BindingFlags FLAGS = 0
			| BindingFlags.Instance
			| BindingFlags.Static
			| BindingFlags.Public
			| BindingFlags.NonPublic
			| BindingFlags.FlattenHierarchy;

		//Values to replace when building
		//Has to be manually set, but that shouldn't be a problem since the break would have been manually created anyways
		private Fix[] _Fixes = {
			#region January 20, 2018: Text Fix
			new Fix
			{
				Type = typeof(AdvobotGuildSettings),
				Path = "WelcomeMessage.Title",
				ErrorValues = new[] { "[]" },
				NewValue = null
			}
			#endregion
		};

		public override bool CanRead => true;
		public override bool CanWrite => false;
		public override bool CanConvert(Type objectType)
		{
			return true;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			//Fixing the JSON
			var jObj = JObject.Load(reader);
			//Only use fixes specified for the class
			foreach (var fix in _Fixes.Where(x => x.Type.IsAssignableFrom(objectType)))
			{
				if (!(jObj.SelectToken(fix.Path)?.Parent is JProperty jProp))
				{
				}
				else if (fix.ErrorValues.Any(x => x.CaseInsEquals(jProp.Value.ToString())))
				{
					jProp.Value = fix.NewValue;
				}
			}

			//Actually creating the object with the JSON
			var value = Activator.CreateInstance(objectType);
			foreach (var member in Config.GuildSettingsType.GetMembers(FLAGS))
			{
				if (!(member.GetCustomAttributes(typeof(JsonPropertyAttribute), false).SingleOrDefault() is JsonPropertyAttribute attr))
				{
					continue;
				}

				var name = String.IsNullOrWhiteSpace(attr.PropertyName) ? member.Name : attr.PropertyName;
				if (String.IsNullOrWhiteSpace(name))
				{
					continue;
				}

				//Setting a value to null will set it to default if value type.
				switch (member)
				{
					case FieldInfo field:
					{
						field.SetValue(value, jObj[name]?.ToObject(field.FieldType, serializer));
						break;
					}
					case PropertyInfo prop:
					{
						prop.SetValue(value, jObj[name]?.ToObject(prop.PropertyType, serializer));
						break;
					}
				}
			}
			return value;
		}
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		private struct Fix
		{
			public Type Type;
			public string Path;
			public string[] ErrorValues;
			public string NewValue;
		}
	}
}
