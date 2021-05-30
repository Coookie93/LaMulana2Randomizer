using System;
using System.Globalization;
using System.Windows.Controls;

namespace LaMulana2Randomizer.UI
{
    public class IntRangeRule : ValidationRule
    {
        public int Min { get; set; }

        public int Max { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
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
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            if ((parameter < Min) || (parameter > Max))
            {
                return new ValidationResult(false, "Please enter value in the range: " + Min + " - " + Max + ".");
            }
            return new ValidationResult(true, null);
        }
    }
}
