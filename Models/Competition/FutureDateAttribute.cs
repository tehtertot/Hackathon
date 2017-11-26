using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Hackathon.Models
{
    public class FutureDateAttribute : ValidationAttribute, IClientModelValidator
    {
        private DateTime _now;

        public void AddValidation(ClientModelValidationContext context)
        {
            // throw new NotImplementedException();
        }

        public FutureDateAttribute()
        {
            _now = DateTime.Now;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Competition comp = (Competition)validationContext.ObjectInstance;
            DateTime submittedTime = comp.Start;
            if (submittedTime < _now)
            {
                return new ValidationResult("Competition must take place in the future.");
            }

            return ValidationResult.Success;
        }
    }
}