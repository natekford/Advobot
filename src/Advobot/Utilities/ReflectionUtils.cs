using System;
using System.Reflection;

namespace Advobot.Utilities
{
	/// <summary>
	/// Actions for reflection.
	/// </summary>
	public static class ReflectionUtils
	{
		/// <summary>
		/// Gets the underlying type of a memberinfo. This only supports events, fields, methods, and properties.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public static Type GetUnderlyingType(this MemberInfo member)
		{
			switch (member.MemberType)
			{
				case MemberTypes.Event:
					return ((EventInfo)member).EventHandlerType;
				case MemberTypes.Field:
					return ((FieldInfo)member).FieldType;
				case MemberTypes.Method:
					return ((MethodInfo)member).ReturnType;
				case MemberTypes.Property:
					return ((PropertyInfo)member).PropertyType;
				default:
					throw new ArgumentException("Input MemberInfo must be EventInfo, FieldInfo, MethodInfo, or PropertyInfo.");
			}
		}
		/// <summary>
		/// Gets the value of a memberinfo. This only supports fields and properties.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static object GetValue(this MemberInfo member, object obj)
		{
			switch (member.MemberType)
			{
				case MemberTypes.Field:
					return ((FieldInfo)member).GetValue(obj);
				case MemberTypes.Property:
					return ((PropertyInfo)member).GetValue(obj);
				default:
					throw new ArgumentException("Input MemberInfo must be FieldInfo or PropertyInfo.");
			}
		}
		/// <summary>
		/// Sets the value of a memberinfo. This only supports fields and properties.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="obj"></param>
		/// <param name="value"></param>
		public static void SetValue(this MemberInfo member, object obj, object value)
		{
			switch (member.MemberType)
			{
				case MemberTypes.Field:
					((FieldInfo)member).SetValue(obj, value);
					return;
				case MemberTypes.Property:
					((PropertyInfo)member).SetValue(obj, value);
					return;
				default:
					throw new ArgumentException("Input MemberInfo must be FieldInfo or PropertyInfo.");
			}
		}
	}
}