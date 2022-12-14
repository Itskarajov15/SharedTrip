using SharedTrip.Core.Contracts;
using System;
using System.ComponentModel.DataAnnotations;

namespace SharedTrip.Core.CustomAttributes
{
    public class CompareDestinationsAttribute : ValidationAttribute
    {
        public string GetErrorMessage() =>
        $"The start destination should be different from the end destination";

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var trip = (ITripModel)validationContext.ObjectInstance;

            if (trip.StartDestinationId == trip.EndDestinationId)
            {
                return new ValidationResult(GetErrorMessage());
            }

            return ValidationResult.Success;
        }
    }
}