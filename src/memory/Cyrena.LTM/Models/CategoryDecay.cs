namespace Cyrena.LTM.Models
{
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public enum CategoryDecay
    {
        Fast,
        Normal,
        Slow,
        None
    }
}
