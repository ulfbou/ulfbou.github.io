using System.Text.Json;

namespace Homepage.Common.Extensions;

public static class JsonExtensions
{
    // Convert an object to a JSON string
    public static string ToJson(this object obj)
    {
        try
        {
            return JsonSerializer.Serialize(obj);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}
