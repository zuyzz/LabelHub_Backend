using System.Text.Json.Serialization;

namespace DataLabelProject.Business.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MediaType
    {
        image,
        audio,
        video
    }
}