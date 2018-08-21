﻿using System.ComponentModel.DataAnnotations;

namespace Advobot.NetCoreUI.Classes.ValidationAttributes
{
	/// <summary>
	/// Validation attribute for bot prefixes.
	/// </summary>
	public sealed class PrefixValidationAttribute : ValidationAttribute
	{
		/// <summary>
		/// Determines whether the passed in object is a valid prefix.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="validationContext"></param>
		/// <returns></returns>
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			return !string.IsNullOrWhiteSpace((string)value)
				? ValidationResult.Success
				: new ValidationResult("Invalid prefix.");
		}
	}
}
