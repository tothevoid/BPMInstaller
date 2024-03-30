using BPMInstaller.Core.Constants;
using BPMInstaller.Core.Model;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using BPMInstaller.Core.Interfaces;
using static BPMInstaller.Core.Services.Application.ApplicationInstance;

namespace BPMInstaller.Core.Services.Application
{
    public static class ApplicationRepository
    {
        private static Dictionary<string, IRunningApplication> Instances { get; } = new();

        public static IRunningApplication GetInstance(string physicalPath, ApplicationConfig applicationConfig)
        {
            if (Instances.ContainsKey(physicalPath))
            {
                return Instances[physicalPath];
            }
            else
            {
                var instance = new RunningApplication(physicalPath, applicationConfig);
                Instances[physicalPath] = instance;
                return instance;
            }
        }

        private class RunningApplication: IRunningApplication, IDisposable
        {
            private Process ApplicationProcess { get; }

            private string ApplicationPath { get; }

            private ApplicationConfig ApplicationConfig { get; }

            public RunningApplication(string applicationPath, ApplicationConfig applicationConfig)
            {
                if (string.IsNullOrEmpty(applicationPath))
                {
                    throw new ArgumentNullException(nameof(applicationPath));
                }
                ApplicationPath = applicationPath;

                ApplicationConfig = applicationConfig ?? throw new ArgumentNullException(nameof(applicationConfig));

                var applicationStartResult = StartApplication();
                if (applicationStartResult.IsStarted)
                {
                    throw new ArgumentException(applicationPath);
                }

                ApplicationProcess = applicationStartResult.ApplicationProcess;
            }

            //TODO: Rework loop
            private (bool IsStarted, Process ApplicationProcess) StartApplication()
            {
                bool applicationStarted = false;

                CloseActiveApplication();

                //TODO: handle not installed core 3.1 exception
                var appProcess = new Process();
                appProcess.StartInfo.WorkingDirectory = ApplicationPath;
                appProcess.StartInfo.FileName = $"dotnet";
                appProcess.StartInfo.Arguments = "BPMSoft.WebHost.dll";
                appProcess.StartInfo.RedirectStandardOutput = true;
                appProcess.StartInfo.UseShellExecute = false;
                appProcess.OutputDataReceived += (_, outputData) =>
                {
                    if (outputData.Data?.Contains("started") == true)
                    {
                        applicationStarted = true;
                    }
                };
                appProcess.Start();
                appProcess.BeginOutputReadLine();
                while (!applicationStarted)
                {
                    Thread.Sleep(100);
                }

                return (applicationStarted, appProcess);
            }

            private bool CloseActiveApplication()
            {
                var connections = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
                var existingAppConnection =
                    IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().FirstOrDefault(tcp =>
                        tcp.Address.Equals(0) && tcp?.Port == ApplicationConfig.ApplicationPort);

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
                        hasModule |= module.FileName == ApplicationPath;
                    }
                    possibleProcess.CloseMainWindow();
                }
                return hasModule;
            }

            public void Compile()
            {
                var authResultHeaders = Auth();

                using var client = new HttpClient();
                foreach (var header in authResultHeaders)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }

                var message = new HttpRequestMessage(HttpMethod.Post, ApplicationApiUtilities.GetAPIUrl(ApplicationConfig.ApplicationPort, "WorkspaceExplorerService", "Rebuild"));
                var result = client.Send(message);
            }

            private Dictionary<string, IEnumerable<string>> Auth()
            {
                using var client = new HttpClient();

                var message = new HttpRequestMessage(HttpMethod.Post, ApplicationApiUtilities.GetAPIUrl(ApplicationConfig.ApplicationPort, "AuthService", "Login"));
                message.Content = new StringContent($"{{\"UserName\":\"{ApplicationAdministrator.UserName}\", \"UserPassword\":\"{ApplicationAdministrator.Password}\"}}",
                    Encoding.UTF8, "application/json");

                var authResult = client.Send(message);

                if (!authResult.IsSuccessStatusCode)
                {
                    throw new Exception("Ошибка авторизации");
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

            public void AddLicenses(LicenseConfig licenseConfig)
            {
                if (string.IsNullOrEmpty(licenseConfig.Path) || licenseConfig.CId == default)
                {
                    return;
                }

                var authResultHeaders = Auth();
                using var client = new HttpClient();
                foreach (var header in authResultHeaders)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }

                var message = new HttpRequestMessage(HttpMethod.Post,
                    ApplicationApiUtilities.GetAPIUrl(ApplicationConfig.ApplicationPort, "LicenseManagerProxyService", "UploadLicenses"));

                var licenseContent = File.ReadAllText(licenseConfig.Path).Replace("\"", "\\\"");
                var licenseJson = $"{{\"licData\":\"{licenseContent}\"}}";
                message.Content = new StringContent(licenseJson, Encoding.UTF8, "application/json");
                var result = client.Send(message);
                var content = result.Content.ReadAsStringAsync().Result;
                var licenseResponse = JsonSerializer.Deserialize<LicenseResponse>(content);

                if (!licenseResponse.Success)
                {
                    throw new Exception("Incorrect license");
                }
            }

            public void Dispose()
            {
                ApplicationProcess.CloseMainWindow();
            }
        }
    }
}
