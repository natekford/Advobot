﻿using System.ComponentModel.DataAnnotations;

namespace Advobot.UI.ValidationAttributes
{
	/// <summary>
	/// Validation attribute for user ids.
	/// </summary>
	public sealed class UserIdValidationAttribute : ValidationAttribute
	{
		/// <summary>
		/// Determines whether the passed in object is a valid user id.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="validationContext"></param>
		/// <returns></returns>
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			return !(value is string str) || string.IsNullOrWhiteSpace(str) || ulong.TryParse(str, out var _)
				? ValidationResult.Success
				: new ValidationResult("Invalid user id.");
		}
	}
}