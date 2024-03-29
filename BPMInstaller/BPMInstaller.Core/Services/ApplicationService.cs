﻿using BPMInstaller.Core.Model;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BPMInstaller.Core.Constants;

namespace BPMInstaller.Core.Services
{
    public class ApplicationService
    {
        private static Action? ActiveApplicationCloseProcessAction;

        public void RunApplication(string applicationPath, Action applicationStarted)
        {
            if (ActiveApplicationCloseProcessAction != null)
            {
                ActiveApplicationCloseProcessAction.Invoke();
                ActiveApplicationCloseProcessAction = null;
            }

            //TODO: handle not installed core 3.1 exception
            Process process = new Process();
            process.StartInfo.WorkingDirectory = applicationPath;
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
            ActiveApplicationCloseProcessAction = () => process.CloseMainWindow();
        }

        public bool CloseActiveApplication(ushort applicationPort, string appPath)
        {
            if (applicationPort == default)
            {
                return false;
            }

            var connections = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
            var existingAppConnection =
                IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().FirstOrDefault(tcp =>
                    tcp.Address.Equals(0) && tcp?.Port == applicationPort);

            var process = Process.GetProcessesByName("dotnet");

            if (process.Length == 0)
            {
                return false;
            }

            bool hasModule = false;

            foreach (var possibleProcess in process)
            {
                foreach (ProcessModule module in possibleProcess.Modules)
                {
                    hasModule |= module.FileName == appPath;
                }
                possibleProcess.CloseMainWindow();
            }
            return hasModule;
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
            message.Content = new StringContent($"{{\"UserName\":\"{ApplicationAdministrator.UserName}\", \"UserPassword\":\"{ApplicationAdministrator.Password}\"}}", 
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
