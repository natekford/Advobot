using System;
using System.Linq.Expressions;
using System.Reflection;
using Advobot.Interfaces;

namespace Advobot.Classes
{
	/// <summary>
	/// Instructs how to get and set a property.
	/// </summary>
	/// <typeparam name="TSource"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public class Setting<TSource, TValue> : ISetting<TValue>, ISetting where TSource : ISettingsBase
	{
		/// <inheritdoc />
		public string Name { get; }

		private readonly TSource _Source;
		private readonly Func<TSource, TValue> _Getter;
		private readonly Action<TSource, TValue> _Setter;
		private readonly Func<TSource, TValue, TValue> _DefaultValueFactory;

		/// <summary>
		/// Creates an instance of <see cref="Setting{TSource, TValue}"/> which uses the passed in factory to either generate a value of modify the current value.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="propertySelector"></param>
		/// <param name="defaultValueFactory">This will be invoked even if there is no setter.</param>
		public Setting(TSource source, Expression<Func<TSource, TValue>> propertySelector, Func<TSource, TValue, TValue> defaultValueFactory)
			: this(source, propertySelector, GenerateSetter(propertySelector))
		{
			_DefaultValueFactory = defaultValueFactory;
		}
		/// <summary>
		/// Creates an instance of <see cref="Setting{TSource, TValue}"/> which resets a reference value.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="propertySelector"></param>
		/// <param name="resetter"></param>
		public Setting(TSource source, Expression<Func<TSource, TValue>> propertySelector, Action<TSource, TValue> resetter)
		{
			var expr = (MemberExpression)propertySelector.Body;
			var prop = (PropertyInfo)expr.Member;
			Name = prop.Name;

			_Source = source;
			_Getter = propertySelector.Compile();
			_Setter = resetter;
		}

		private static Action<TSource, TValue> GenerateSetter(Expression<Func<TSource, TValue>> propertySelector)
		{
			var expr = (MemberExpression)propertySelector.Body;
			var prop = (PropertyInfo)expr.Member;
			return prop.GetSetMethod() is MethodInfo setter
				? (Action<TSource, TValue>)Delegate.CreateDelegate(typeof(Action<TSource, TValue>), setter)
				: null;
		}
		/// <inheritdoc />
		public TValue GetValue() => _Getter(_Source);
		/// <inheritdoc />
		public void Reset()
		{
			var defaultValue = _DefaultValueFactory(_Source, GetValue());
			_Setter?.Invoke(_Source, defaultValue);
		}
		/// <inheritdoc />
		public void SetValue(TValue newValue) => _Setter(_Source, newValue);

		//ISetting
		object ISetting.GetValue() => GetValue();
		void ISetting.SetValue(object newValue) => SetValue((TValue)newValue);
	}
}
