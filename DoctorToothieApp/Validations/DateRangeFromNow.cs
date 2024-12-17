using System.ComponentModel.DataAnnotations;

namespace DoctorToothieApp.Validations;

public class DateRangeFromNow(bool equals) : ValidationAttribute
{

    public override bool IsValid(object? value)
    {
        if (value is not DateTime) return false;
        DateTime dt = (DateTime)value;

        if (equals) return dt >= DateTime.UtcNow;

        return dt > DateTime.UtcNow;
    }
}
