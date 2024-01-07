using BPMInstaller.Core.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BPMInstaller.Core.Services
{
    public class ApplicationService
    {
        public void RunApplication(ApplicationConfig applicationConfig, Action applicationStarted)
        {
            //TODO: handle not installed core 3.1 exception
            Process process = new Process();
            process.StartInfo.WorkingDirectory = applicationConfig.ApplicationPath;
            process.StartInfo.FileName = $"dotnet";
            process.StartInfo.Arguments = "BPMSoft.WebHost.dll";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                if (e?.Data != null && e.Data.Contains("started"))
                {
                    applicationStarted?.Invoke();
                }
            };
            process.Start();
            process.BeginOutputReadLine();
        }


        public void RebuildApplication(ApplicationConfig applicationConfig)
        {
            var authResultHeaders = Auth(applicationConfig);

            using var client = new HttpClient();
            foreach (var header in authResultHeaders)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            var message = new HttpRequestMessage(HttpMethod.Post, $"{applicationConfig.ApplicationUrl}/ServiceModel/WorkspaceExplorerService.svc/Rebuild");
            var result = client.Send(message);
        }

        private Dictionary<string, IEnumerable<string>> Auth(ApplicationConfig applicationConfig)
        {
            using var client = new HttpClient();

            var message = new HttpRequestMessage(HttpMethod.Post, $"{applicationConfig.ApplicationUrl}/ServiceModel/AuthService.svc/Login");
            message.Content = new StringContent($"{{\"UserName\":\"{applicationConfig.AdminUserName}\", \"UserPassword\":\"{applicationConfig.AdminUserPassword}\"}}", 
                Encoding.UTF8, "application/json");

            var authResult = client.Send(message);

            if (!authResult.IsSuccessStatusCode)
            {
                throw new ArgumentException(nameof(applicationConfig));
            }

            var authorizationHeaders = new Dictionary<string, IEnumerable<string>>();
            foreach (var (key, value) in authResult.Headers)
            {
                authorizationHeaders.TryAdd(key, value);
            }
          
            if (authResult.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                var enumerable = cookies.ToList();
                var allCookies = enumerable.Select(cookie => cookie.Split(";").First()).ToArray();
                authorizationHeaders.Add("Cookie", new List<string> { string.Join(";", allCookies) });
                foreach (var cookie in allCookies.Select(x => x.Split("=")))
                {
                    var values = new List<string>();
                    if (cookie.Any())
                    {
                        values.Add(cookie[1]);
                    }
                    authorizationHeaders.TryAdd(cookie[0], values);
                }
            }

            return authorizationHeaders;
        }

        public void UploadLicenses(ApplicationConfig appConfig, LicenseConfig licenseConfig)
        {
            if (licenseConfig == null || string.IsNullOrEmpty(licenseConfig.Path) || licenseConfig.CId == default)
            {
                return;
            }

            var authResultHeaders = Auth(appConfig);
            using var client = new HttpClient();
            foreach (var header in authResultHeaders)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
            var message = new HttpRequestMessage(HttpMethod.Post, 
                $"{appConfig.ApplicationUrl}/ServiceModel/LicenseManagerProxyService.svc/UploadLicenses");

            var licenseContent = File.ReadAllText(licenseConfig.Path).Replace("\"", "\\\"");
            var licenseJson = $"{{\"licData\":\"{licenseContent}\"}}";
            message.Content = new StringContent(licenseJson, Encoding.UTF8, "application/json");
            var result = client.Send(message);
            var content = result.Content.ReadAsStringAsync().Result;
            var licenseResponse = JsonSerializer.Deserialize<LicenseReponse>(content);

            if (!licenseResponse.Success)
            {
                throw new Exception("Incorrect license");
            }
        }

        public class LicenseReponse
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
