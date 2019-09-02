using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using AdvorangesUtils;

namespace Advobot.Classes
{
	/// <summary>
	/// An error which occurs when attempting to modify an <see cref="EmbedWrapper"/>.
	/// </summary>
	public interface IEmbedError
	{
		/// <summary>
		/// The property which had an error.
		/// </summary>
		public string PropertyPath { get; }

		/// <summary>
		/// The reason for this error.
		/// </summary>
		public string? Reason { get; }

		/// <summary>
		/// The value that gave an error.
		/// </summary>
		public object? Value { get; }
	}

	internal interface IRemainingEmbedError : IEmbedError
	{
		bool IsNewLines { get; }
		int RemainingLength { get; }
	}

	internal static class EmbedUtils
	{
		//make into class
		//options: no param expression names or yes param expression names
		//options: use closure variable names or attempt to get value

		public static string GetPropertyPath(this LambdaExpression expr)
			=> expr.Body.GetFromAny();

		private static string GetFromAny(this Expression expr) => expr switch
		{
			LambdaExpression lambda => lambda.GetPropertyPath(),
			BinaryExpression binary => binary.GetFromBinary(),
			UnaryExpression unary => unary.GetFromUnary(),
			MemberExpression member => member.GetFromMember(),
			MethodCallExpression call => call.GetFromCall(),
			NewExpression @new => @new.GetFromNew(),
			ConstantExpression constant => constant.GetFromConstant(),
			ParameterExpression _ => "", //Change to 'param' and 'param.Name' to include them
			_ => throw new ArgumentException("This expression has not been implemented yet."),
		};

		private static string GetFromBinary(this BinaryExpression binary)
		{
			var left = binary.Left.GetFromAny();
			var right = binary.Right.GetFromAny();
			return $"{left} + {right}";
		}

		private static string GetFromCall(this MethodCallExpression call)
		{
			var name = call.Method.Name;
			var args = call.Arguments.Join(GetFromAny);

			//Changing indexer from obj.get_Item to obj[]
			if (call.Method.IsSpecialName)
			{
				var type = call.Method.DeclaringType;
				var def = type.GetCustomAttributes<DefaultMemberAttribute>().FirstOrDefault();
				if (def != null && name == $"get_{def.MemberName}")
				{
					return $"{call.Object.GetFromAny()}[{args}]";
				}
			}

			var nameAndArgs = $"{name}({args})";
			if (call.Method.IsStatic)
			{
				var type = call.Method.DeclaringType;
				if (!type.IsNested)
				{
					return $"{type.Name}.{nameAndArgs}";
				}

				var fn = call.Method.DeclaringType.FullName;
				var nestedClasses = fn.Substring(fn.LastIndexOf('.') + 1);
				var withPeriods = nestedClasses.Replace('+', '.');
				return $"{withPeriods}.{nameAndArgs}";
			}

			var obj = call.Object?.GetFromAny();
			if (!string.IsNullOrWhiteSpace(obj))
			{
				return $"{obj}.{nameAndArgs}";
			}
			return nameAndArgs;
		}

		private static string GetFromConstant(this ConstantExpression constant)
		{
			//Has to go first since strings are not value types
			//In quotes because strings are declared in quotes
			if (constant.Value is string s)
			{
				return $"\"{s}\"";
			}
			else if (constant.Type.IsEnum)
			{
				return constant.Value.ToString();
			}
			//Only occurs if 'this' is the constant value
			else if (!constant.Type.IsPrimitive)
			{
				return "";
			}
			return constant.Value.ToString();
		}

		private static string GetFromMember(this MemberExpression member)
		{
			var name = member.Member.Name;
			if (member.Expression == null)
			{
				return name;
			}
			//Can potentially get the direct value and replace the name with it,
			//but if the class doesn't implement a good .ToString then that's
			//worse than the var name
			//It's also annoying when dealing with closure because anonymous classes
			//have long annoying names
			if (member.Expression.NodeType == ExpressionType.Constant)
			{
				return name;
			}

			var exprRep = member.Expression.GetFromAny();
			if (string.IsNullOrWhiteSpace(exprRep))
			{
				return name;
			}

			return $"{exprRep}.{name}";
		}

		private static string GetFromNew(this NewExpression @new)
		{
			var name = @new.Type.Name;
			var args = @new.Arguments.Join(GetFromAny);
			return $"new {name}({args})";
		}

		private static string GetFromUnary(this UnaryExpression unary)
											=> unary.Operand.GetFromAny();
	}

	/// <summary>
	/// Provides information about why something failed to add to an embed.
	/// </summary>
	internal class EmbedError<TEmbedBuilder, TProperty> : IEmbedError
	{
		/// <inheritdoc />
		public string PropertyPath { get; }

		/// <inheritdoc />
		public string? Reason { get; private set; }

		/// <inheritdoc />
		public TProperty Value { get; }

		//IEmbedError
		object? IEmbedError.Value => Value;

		/// <summary>
		/// Creates an instance of <see cref="EmbedError{TEmbedBuilder, TProperty}"/>.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="value"></param>
		public EmbedError(Expression<Func<TEmbedBuilder, TProperty>> property, TProperty value)
		{
			PropertyPath = property.GetPropertyPath();
			Value = value;
		}

		/// <summary>
		/// Returns the errors saying the property path, value, and reason.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> $"{PropertyPath}: '{Value?.ToString() ?? "null"}' is invalid. Reason: {Reason}";

		public IEmbedError WithInvalidUrl()
			=> WithReason("Invalid url.");

		public IRemainingEmbedError WithMax(int m)
			=> new RemainingEmbedError(this, m, $"Max length is {m}.");

		public IEmbedError WithMustBePositive()
			=> WithReason("Cannot be less than zero.");

		public IEmbedError WithNone()
			=> WithReason("None to remove.");

		public IEmbedError WithNotEmpty()
			=> WithReason("Cannot be null or empty.");

		public IEmbedError WithOutOfBounds()
			=> WithReason("Out of bounds.");

		public IEmbedError WithReason(string reason)
		{
			Reason = reason;
			return this;
		}

		public IRemainingEmbedError WithRemaining(int r)
																					=> new RemainingEmbedError(this, r, $"Remaining length is {r}.");

		public IRemainingEmbedError WithRemainingNewLines(int r)
			=> new RemainingEmbedError(this, r, $"Remaining new lines is {r}.", newLines: true);
	}

	internal class RemainingEmbedError : IRemainingEmbedError
	{
		/// <inheritdoc />
		public bool IsNewLines { get; }

		/// <inheritdoc />
		public string PropertyPath { get; }

		/// <inheritdoc />
		public string? Reason { get; }

		/// <inheritdoc />
		public int RemainingLength { get; }

		/// <inheritdoc />
		public object? Value { get; }

		/// <summary>
		/// Creates an instance of <see cref="RemainingEmbedError"/>.
		/// </summary>
		/// <param name="error"></param>
		/// <param name="remainingLength"></param>
		/// <param name="reason"></param>
		/// <param name="newLines"></param>
		public RemainingEmbedError(IEmbedError error, int remainingLength, string reason, bool newLines = false)
		{
			RemainingLength = remainingLength;
			IsNewLines = newLines;
			Reason = reason;
			PropertyPath = error.PropertyPath;
			Value = error.Value;
		}
	}
}