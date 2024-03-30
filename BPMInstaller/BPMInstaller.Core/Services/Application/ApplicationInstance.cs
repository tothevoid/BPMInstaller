using BPMInstaller.Core.Model;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BPMInstaller.Core.Constants;

namespace BPMInstaller.Core.Services.Application
{
    public class ApplicationInstance
    {
        private static Process? applicationProcess;

        public ApplicationInstance(string applicationPath)
        {

        }


        public class LicenseResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("errorInfo")]
            public ErrorInfo ErrorInfo { get; set; }
        }

        public class ErrorInfo
        {
            [JsonPropertyName("message")]
            public string Message { get; set; }
        }
    }
}
