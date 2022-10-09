using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TEPSClientInstallService_Master_UpdateUtility.Classes;

namespace TEPSClientInstallService_UpdateUtility.Classes
{
    internal class MasterServiceUpdateClass
    {
        private readonly string badAppName = "TEPS.Automated.Client.Install.Master.Service.zip";
        private readonly string goodAppName = "TEPS Automated Client Install Master Service.zip";
        private readonly string serviceName = "TEPSClientInstallMasterService.exe";
        private readonly string serviceInstallPath = @"C:\Services\Tyler-Client-Install-Master-Service";
        private readonly string serviceBackUpPath = @"C:\ProgramData\Tyler Technologies\Public Safety\Tyler-Client-Install-Master-Service\BackUps";
        private readonly string scriptsDir = @"C:\ProgramData\Tyler Technologies\Public Safety\Tyler-Client-Install-Master-Service\Resources\Scripts";
        private string serviceRelease = "";
        private readonly string getByIDNum = "44";
        private readonly string downloadByIDNum = "33";
        private readonly string externalURL1 = "https://github.com/davasorus/FileRepository/releases/download/1.5/NWPS.Client.Admin.Tool.exe";

        private int i = 0;

        private loggingClass loggingClass = new loggingClass();
        private serviceClass serviceClass = new serviceClass();
        private compressionClass compressionClass = new compressionClass();
        private serviceConfigInteractionClass serviceConfigInteractionClass = new serviceConfigInteractionClass();
        private sqlInteractionClass sqlInteractionClass = new sqlInteractionClass();

        private jsonObj JO1 = new jsonObj();

        //app start up API checker
        public async Task updateAPICheckAsync()
        {
            try
            {
                await getByID(getByIDNum);
            }
            catch (Exception ex)
            {
                string logEntry1 = ex.ToString();

                loggingClass.logEntryWriter(logEntry1, "error");

                //await loggingClass.remoteErrorReporting("Client Admin Tool", Assembly.GetExecutingAssembly().GetName().Version.ToString(), ex.ToString(), "Automated Error Reported by " + Environment.UserName);
            }
        }

        //will query the API - test
        public async Task getByID(string ID)
        {
            try
            {
                var httpClient = new HttpClient();
                var defaultRequestHeaders = httpClient.DefaultRequestHeaders;

                if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new
                      MediaTypeWithQualityHeaderValue("application/json"));
                }

                HttpResponseMessage response = await httpClient.GetAsync("https://davasoruswebapi.azurewebsites.net/api/webapi/" + ID);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();

                    compare(json);
                }
                else
                {
                    string logEntry1 = $" Failed to call the Web Api: {response.StatusCode}";

                    loggingClass.logEntryWriter(logEntry1, "error");

                    string content = await response.Content.ReadAsStringAsync();
                    string logEntry2 = $" Content: {content}";

                    loggingClass.logEntryWriter(logEntry2, "error");
                }
            }
            catch (TaskCanceledException)
            {
                //await loggingClass.remoteErrorReporting("Client Admin Tool", Assembly.GetExecutingAssembly().GetName().Version.ToString(), "Task Cancellation error, must be awaited all the way down", "Automated Error Reported by " + Environment.UserName);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Unable to connect to the remote server"))
                {
                    i = 100;

                    loggingClass.logEntryWriter("Unable to connect to end point, unable to check for updates", "error");
                }
                else if (ex.Message.Contains("The underlying connection was closed: A connection that was expected to be kept alive was closed by the server"))
                {
                    i = 100;

                    loggingClass.logEntryWriter("Unable to connect to end point, unable to check for updates", "error");
                }
                else if (ex.ToString().Contains("System.IO.DirectoryNotFoundException"))
                {
                    i = 100;
                    loggingClass.logEntryWriter("Unable to find config file directory, unable to check for updates", "error");
                    //loggingClass.queEntrywriter("Please verify that client and config file are in the same folder");
                }
                else if (ex.Message.Contains("The underlying connection was closed: A connection that was expected to be kept alive was closed by the server"))
                {
                }
                else
                {
                    i = 100;

                    string logEntry1 = ex.ToString();

                    loggingClass.logEntryWriter(logEntry1, "error");

                    //await loggingClass.remoteErrorReporting("Client Admin Tool", Assembly.GetExecutingAssembly().GetName().Version.ToString(), ex.ToString(), "Automated Error Reported by " + Environment.UserName);
                }
            }
        }

        //will query the file controller API
        public async Task downloadByID(string ID)
        {
            try
            {
                var httpClient = new HttpClient();
                var defaultRequestHeaders = httpClient.DefaultRequestHeaders;

                if (defaultRequestHeaders.Accept == null ||
                   !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new
                      MediaTypeWithQualityHeaderValue("application/json"));
                }

                HttpResponseMessage response = await httpClient.GetAsync("https://davasoruswebapi.azurewebsites.net/api/webapi/filecontroller/" + ID);

                string downloadsPath = @"C:\ProgramData\Tyler Technologies\Public Safety\Tyler-Client-Install-Master-Service\Downloads";

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    var fileInfo = new FileInfo(downloadsPath + "\\" + badAppName);
                    using (var fileStream = fileInfo.OpenWrite())
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }

                if (response.IsSuccessStatusCode)
                {
                    string logEntry = "File downloaded";
                    loggingClass.logEntryWriter(logEntry, "info");
                }
                else
                {
                    string logEntry1 = $"Failed to call the Web Api: {response.StatusCode}";

                    loggingClass.logEntryWriter(logEntry1, "error");

                    string content = await response.Content.ReadAsStringAsync();
                    string logEntry2 = $"Content: {content}";

                    loggingClass.logEntryWriter(logEntry2, "info");
                }
            }
            catch (TaskCanceledException)
            {
                //await loggingClass.remoteErrorReporting("Client Admin Tool", Assembly.GetExecutingAssembly().GetName().Version.ToString(), "Task Cancellation error, must be awaited all the way down", "Automated Error Reported by " + Environment.UserName);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Unable to connect to the remote server"))
                {
                    loggingClass.logEntryWriter("Unable to connect to end point, unable to check for update history", "error");
                }
                else
                {
                    string logEntry1 = ex.ToString();

                    loggingClass.logEntryWriter(logEntry1, "error");

                    //await loggingClass.remoteErrorReporting("Client Admin Tool", Assembly.GetExecutingAssembly().GetName().Version.ToString(), ex.ToString(), "Automated Error Reported by " + Environment.UserName);
                }
            }
        }

        //compares application version number to API version number
        private void compare(string json)
        {
            deserializeJSON(json);

            string A = returnAssemblyInformation(Path.Combine(serviceInstallPath, serviceName));

            //this removes the separators in the version number
            string B = A.Replace(".", "");
            string subB = JO1.appVersion.Replace(".", "");

            //this converts the string numbers into ints
            int a = int.Parse(B);
            int b = int.Parse(subB);

            //this compares the incoming number from the app version number
            int x = a - b;

            if (x > 0)
            {
                updateResult.updateMessage = "Dev Special :)";

                return;
            }
            else if (x == 0)
            {
                updateResult.updateMessage = "Up-to-Date";

                return;
            }
            else if (x < 0)
            {
                string logEntry = "TEPS Automated Client Install Agent current version is on older release";
                loggingClass.logEntryWriter(logEntry, "info");

                updateResult.updateMessage = "Newer Version Found";

                downloadTask(badAppName, externalURL1, serviceInstallPath);

                return;
            }
        }

        //deserializes JSON package and dynamically creates objects
        private async void deserializeJSON(string json)
        {
            try
            {
                var appInfo = JsonConvert.DeserializeObject<dynamic>(json);

                if (appInfo.id != null)
                {
                    JO1.id = appInfo.id;
                    JO1.appName = appInfo.appName;
                    JO1.appVersion = appInfo.appVersion;
                    JO1.releaseNotes = appInfo.releaseNotes;
                    JO1.tansactionDateTime = appInfo.tansactionDateTime;

                    string logEntry1 = ("    ID " + appInfo.id);
                    string logEntry2 = ("    App Name " + appInfo.appName);
                    string logEntry3 = ("    App Version " + appInfo.appVersion);
                    string logEntry4 = ("    Release Notes " + appInfo.releaseNotes);
                    string logEntry5 = ("    Date of Update " + appInfo.tansactionDateTime);

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("JSON Package Parsed below ");
                    sb.AppendLine(logEntry1);
                    sb.AppendLine(logEntry2);
                    sb.AppendLine(logEntry3);
                    sb.AppendLine(logEntry4);
                    sb.Append(logEntry5);
                    loggingClass.nLogLogger(sb.ToString(), "info");
                }
                else
                {
                    throw new ArgumentNullException(json + " was not correctly parsed into " + appInfo + " ID was null.");
                }
            }
            catch (Exception ex)
            {
                string logEntry = "error parsing information from JSON package: " + ex.Message;
                loggingClass.logEntryWriter(logEntry, "error");

                //await loggingClass.remoteErrorReporting("Client Admin Tool", Assembly.GetExecutingAssembly().GetName().Version.ToString(), ex.ToString(), "Automated Error Reported by " + Environment.UserName);
            }
        }

        //prompts the user with a message to update or not
        public async void downloadTask(string programName, string URL, string location)
        {
            await downloadExternal(programName, URL, location);
        }

        //does the actual downloading
        public async Task downloadExternal(string programName, string URL, string location)
        {
            try
            {
                string logEntry = "Attempting to Download " + programName;

                loggingClass.logEntryWriter(logEntry, "info");
                //loggingClass.queEntrywriter(logEntry);

                //this will grab the local user downloads folder
                string downloadsPath = @"C:\ProgramData\Tyler Technologies\Public Safety\Tyler-Client-Install-Master-Service\Downloads";

                Directory.CreateDirectory(downloadsPath);

                //will check the download folder to make sure the download folder is clear of an older version of the app
                downloadPreChecker(downloadsPath);

                //this will download the application from the passed URL
                await downloadByID(downloadByIDNum);

                string logEntry1 = programName + " downloaded";
                loggingClass.logEntryWriter(logEntry1, "info");
                //loggingClass.queEntrywriter(logEntry1);

                //this will attempt to copy the program from the downloads folder to a folder
                //this then will run that application in the new location
                try
                {
                    //stop current service
                    serviceClass.stopService($"TEPS Automated Client Install Master Service");

                    var num = returnAssemblyInformation(Path.Combine(serviceInstallPath, serviceName));

                    //back up current service folder
                    compressionClass.compression(serviceInstallPath, Path.Combine(serviceBackUpPath, $"TEPSClientInstallMasterService _{num}_") + ".zip");

                    //delete current service
                    string command = "/C C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\installutil.exe -u C:\\Services\\Tyler-Client-Install-Master-Service\\TEPSClientInstallMasterService.exe";
                    serviceClass.cmdScriptRun(command);

                    //clear out service directory
                    serviceClass.clearOutDir(serviceInstallPath);

                    //decompress from download folder to service folder
                    compressionClass.decompress(Path.Combine(downloadsPath, badAppName), serviceInstallPath);

                    compressionClass.decompress1(Path.Combine(downloadsPath, badAppName), @"C:\ProgramData\Tyler Technologies\Public Safety\Tyler-Client-Install-Master-Service\");

                    //install new service
                    string command1 = "/C C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\installutil.exe C:\\Services\\Tyler-Client-Install-Master-Service\\TEPSClientInstallMasterService.exe";
                    serviceClass.cmdScriptRun(command1);

                    if (dbPresent.checkDB.Equals(false))
                    {
                        foreach (string s in Directory.GetFiles(scriptsDir))
                        {
                            await sqlInteractionClass.buildOutDB(s);
                        }

                        loggingClass.logEntryWriter("DB Build out Complete", "info");
                    }
                    else
                    {
                        loggingClass.logEntryWriter("DB present, build out skipped", "info");
                    }

                    Thread.Sleep(500);

                    serviceConfigInteractionClass.configUpdate(configData.DBname);

                    //start new service
                    serviceClass.startService($"TEPS Automated Client Install Master Service");
                }
                catch (IOException ex)
                {
                    loggingClass.logEntryWriter(ex.ToString(), "error");
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Access to the path"))
                    {
                        loggingClass.logEntryWriter(ex.ToString(), "error");

                        Thread.Sleep(5000);

                        Process.Start(goodAppName);
                    }
                    else
                    {
                        string logEntry9 = ex.ToString();

                        loggingClass.logEntryWriter(logEntry9, "error");

                        //await loggingClass.remoteErrorReporting("Client Admin Tool", Assembly.GetExecutingAssembly().GetName().Version.ToString(), ex.ToString(), "Automated Error Reported by " + Environment.UserName);
                    }
                }
            }
            catch (IOException ex)
            {
                loggingClass.logEntryWriter(ex.ToString(), "error");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Access to the path"))
                {
                    downloadExternal(programName, URL, location);
                }
                else
                {
                    string logEntry = ex.ToString();

                    loggingClass.logEntryWriter(logEntry, "error");

                    //await loggingClass.remoteErrorReporting("Client Admin Tool", Assembly.GetExecutingAssembly().GetName().Version.ToString(), logEntry, "Automated Error Reported by " + Environment.UserName);
                }
            }
        }

        //this will check to make sure the application that is being downloaded is actually present in the download folder.
        //this will wait 6 - 10 Second increments after the initial 10 second window.
        private void externalDownloadChecker(string location)
        {
            for (int i = 1; i <= 10; i++)
            {
                if (!File.Exists(Path.Combine(location, Path.GetFileName(badAppName))))
                {
                    string logEntry = "waiting to find downloaded application for " + i.ToString() + "0 seconds";
                    loggingClass.logEntryWriter(logEntry, "info");

                    Thread.Sleep(5000);
                }
                else
                {
                    return;
                }
            }
        }

        //this will look for the old client (currently running) and relabel it to appname_oldversion
        //if an older version exists it is deleted.
        private async void relableandMove(string location, string app, string sourceFile, string destinationFile)
        {
            string appFolder = Path.GetDirectoryName(location);
            string appName = Path.GetFileNameWithoutExtension(app);
            string appExtension = Path.GetExtension(location);
            string archivePath = Path.Combine(appFolder, appName + "_OldVersion" + appExtension);
            try
            {
                if (File.Exists(archivePath))
                {
                    string logEntry3 = "Old backup found. Will attempt to delete";
                    loggingClass.logEntryWriter(logEntry3, "info");

                    File.Delete(archivePath);

                    string logEntry2 = archivePath + " deleted";
                    loggingClass.logEntryWriter(logEntry2, "info");
                }

                File.Move(app, archivePath);

                string logEntry = app + " renamed to " + appName + "_OldVersion" + appExtension;
                loggingClass.logEntryWriter(logEntry, "info");

                File.Move(sourceFile, destinationFile);

                string logEntry1 = sourceFile + " renamed to " + destinationFile;
                loggingClass.logEntryWriter(logEntry1, "info");
            }
            catch (Exception ex)
            {
                string logEntry = ex.ToString();

                loggingClass.logEntryWriter(logEntry, "error");

                //await loggingClass.remoteErrorReporting("Client Admin Tool", Assembly.GetExecutingAssembly().GetName().Version.ToString(), ex.ToString(), "Automated Error Reported by " + Environment.UserName);
            }
        }

        //will check the download folder to make sure the download folder is clear of an older version of the app
        private void downloadPreChecker(string downloadsPath)
        {
            string logEntry = "Checking download folder to make sure " + badAppName + " is not already downloaded.";

            loggingClass.logEntryWriter(logEntry, "info");

            string[] programName = { badAppName, goodAppName };

            string[] files = Directory.GetFiles(downloadsPath, "*.*");

            //for each file in the downloads folder the name is compared against either the of the known versions of the name
            //if it is found the file is deleted to prepare for the download from the API or GitHub
            foreach (var f in files)
            {
                foreach (var name in programName)
                {
                    if (f.Contains(name))
                    {
                        string DeletedName = " " + name;

                        File.Delete(Path.Combine(downloadsPath, name));

                        string logEntry1 = DeletedName + " was found in and deleted from " + downloadsPath;

                        loggingClass.logEntryWriter(logEntry1, "info");
                    }
                }
            }
        }

        private string returnAssemblyInformation(string path)
        {
            try
            {
                FileVersionInfo info = FileVersionInfo.GetVersionInfo(path);

                return info.FileVersion;
            }
            catch (Exception ex)
            {
                return "-1";
            }
        }
    }
}

internal class apiObj
{
    public string id { get; set; }
    public string appName { get; set; }
    public string appVersion { get; set; }
    public string releaseNotes { get; set; }
    public string tansactionDateTime { get; set; }
}

internal class jsonObj
{
    public string id { get; set; }
    public string appName { get; set; }
    public string appVersion { get; set; }
    public string releaseNotes { get; set; }
    public string tansactionDateTime { get; set; }
}

internal class updateResult
{
    public static string updateMessage { get; set; }
}