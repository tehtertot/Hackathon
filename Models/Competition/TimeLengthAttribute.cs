using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Hackathon.Models
{
    public class TimeLengthAttribute : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            // throw new NotImplementedException();
        }

        public TimeLengthAttribute()
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Competition comp = (Competition)validationContext.ObjectInstance;
            DateTime start = comp.Start;
            DateTime end = comp.End;

            if (end < start)
            {
                return new ValidationResult("Competition must end after the start date and time.");
            }

            return ValidationResult.Success;
        }
    }
}