namespace BPMInstaller.Core.Model.Docker
{
    using System.Text.Json.Serialization;

    public class DockerContainer
    {
        public string Id { get; set; }

        public string ImageName { get; set; }
    }
}
