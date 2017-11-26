using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Hackathon.Models
{
    public class PositiveRangeAttribute : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            // throw new NotImplementedException();
        }

        public PositiveRangeAttribute()
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Competition comp = (Competition)validationContext.ObjectInstance;
            int size = comp.MaxSize;
            if (size < 1)
            {
                return new ValidationResult("Teams must be of positive size");
            }

            return ValidationResult.Success;
        }
    }
}