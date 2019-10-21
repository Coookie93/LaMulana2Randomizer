using System;
using System.Globalization;
using System.Windows.Controls;

namespace LM2Randomizer.UI
{
    public class IntRangeRule : ValidationRule
    {
        public double Min { get; set; }

        public double Max { get; set; }

        public override ValidationResult Validate(object value,
                                                   CultureInfo cultureInfo)
        {
            int parameter = 0;
            try
            {
                if (((string)value).Length > 0)
                {
                    parameter = int.Parse((string)value);
                }
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Illegal characters or "
                                             + e.Message);
            }

            if ((parameter < Min) || (parameter > Max))
            {
                return new ValidationResult(false,
                    "Please enter value in the range: "
                    + this.Min + " - " + this.Max + ".");
            }
            return new ValidationResult(true, null);
        }
    }
}
