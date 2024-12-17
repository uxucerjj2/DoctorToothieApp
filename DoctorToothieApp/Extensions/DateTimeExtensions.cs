namespace DoctorToothieApp.Extensions;

public static class DateTimeExtensions
{
    public static string ToStr(this DateTime? dateTime)
    {
        if (dateTime == null) return "null";

        return ((DateTime)dateTime!).ToString("yyyy-MM-dd HH:mm:ss");

    }
    public static string ToStr(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
