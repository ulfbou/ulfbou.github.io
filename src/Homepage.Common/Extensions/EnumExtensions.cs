namespace Homepage.Common.Extensions;

public static class EnumExtensions
{
    // Convert an enum to a string
    public static string ToFriendlyString(this Enum value) => value.ToString().Replace("_", " ");

    // Convert a string to an enum type
    public static T? ToEnum<T>(this string value) => Enum.TryParse(typeof(T), value, true, out var result) ? (T)result : default;
}