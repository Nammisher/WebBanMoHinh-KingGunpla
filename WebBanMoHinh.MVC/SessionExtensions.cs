using System.Text.Json;

public static class SessionExtensions
{
    public static void SetAsJson(this ISession session, string key, object value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }
}