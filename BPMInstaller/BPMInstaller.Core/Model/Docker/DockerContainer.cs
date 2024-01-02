namespace BPMInstaller.Core.Model.Docker
{
    using System.Text.Json.Serialization;

    public class DockerContainer
    {
        [JsonPropertyName("ID")]
        public string Id { get; set; }

        [JsonPropertyName("Image")]
        public string ImageName { get; set; }
    }
}
