using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.ServiceProcess;
using System.Threading;

namespace TEPSClientInstallService_UpdateUtility.Classes
{
    internal class serviceClass
    {
        private loggingClass loggingClass = new loggingClass();

        #region Service Related Code

        //will stop the service by name
        public async void stopService(string name)
        {
            try
            {
                ServiceController sc = new ServiceController(name);
                if (sc.Status.Equals(ServiceControllerStatus.Running))
                {
                    sc.Stop();

                    string logEntry = name + " has been stopped.";

                    loggingClass.logEntryWriter(logEntry, "info");
                    //loggingClass.queEntrywriter(logEntry);
                }
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("InvalidOperationException"))
                {
                    string logEntry = name + " could not be stopped. It likely is not installed";

                    loggingClass.logEntryWriter(logEntry, "error");
                }
                else
                {
                    string logEntry1 = ex.ToString();

                    loggingClass.logEntryWriter(logEntry1, "error");

                    // await loggingClass.remoteErrorReporting("Client Admin Tool", Assembly.GetExecutingAssembly().GetName().Version.ToString(), ex.ToString(), "Automated Error Reported by " + Environment.UserName);
                }
            }
        }

        //will start the service by name
        public async void startService(string name)
        {
            try
            {
                ServiceController sc = new ServiceController(name);
                if (sc.Status.Equals(ServiceControllerStatus.Stopped))
                {
                    sc.Start();

                    string logEntry = name + " has been started.";

                    loggingClass.logEntryWriter(logEntry, "info");
                    //loggingClass.queEntrywriter(logEntry);
                }
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("InvalidOperationException"))
                {
                    string logEntry = name + " Could not be started. It likely is not installed";

                    loggingClass.logEntryWriter(logEntry, "error");
                }
                else
                {
                    string logEntry1 = ex.ToString();

                    loggingClass.logEntryWriter(logEntry1, "error");

                    //await loggingClass.remoteErrorReporting("Client Admin Tool", Assembly.GetExecutingAssembly().GetName().Version.ToString(), ex.ToString(), "Automated Error Reported by " + Environment.UserName);
                }
            }
        }

        //searches for a service with a specific name and returns it's status
        public string getServiceStatus(string serviceName)
        {
            try
            {
                ServiceController myservice = new ServiceController(serviceName);

                string svcStatus = myservice.Status.ToString();

                return svcStatus;
            }
            catch
            {
                return "error";
            }
        }

        #endregion Service Related Code

        //this will run command prompt scripts within the application.
        public bool cmdScriptRun(string Command)
        {
            bool value = false;

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = Command
            };
            process.StartInfo = startInfo;

            loggingClass.logEntryWriter("running script =>" + Command, "info");
            process.Start();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                string logEntry2 = Command + " has been installed successfully";

                loggingClass.logEntryWriter(logEntry2, "info");

                value = true;
            }
            else if (process.ExitCode == -1)
            {
            }
            else
            {
                string errorcode = process.ExitCode.ToString();
                string logEntry2 = Command + " failed to install. Error code: " + errorcode;

                loggingClass.logEntryWriter(logEntry2, "error");

                value = false;
            }

            return value;
        }

        //this will give the user role full control for folder permissions
        //this also will modify the all files and sub directories with full control to the user role
        public static bool setAcl(string destinationDirectory)
        {
            try
            {
                FileSystemRights Rights = (FileSystemRights)0;
                Rights = FileSystemRights.FullControl;

                // *** Add Access Rule to the actual directory itself
                FileSystemAccessRule AccessRule = new FileSystemAccessRule("Users", Rights,
                                            InheritanceFlags.None,
                                            PropagationFlags.NoPropagateInherit,
                                            AccessControlType.Allow);

                DirectoryInfo Info = new DirectoryInfo(destinationDirectory);
                DirectorySecurity Security = Info.GetAccessControl(AccessControlSections.Access);

                Security.ModifyAccessRule(AccessControlModification.Set, AccessRule, out bool Result);

                if (!Result)
                    return false;

                // *** Always allow objects to inherit on a directory
                InheritanceFlags iFlags = InheritanceFlags.ObjectInherit;
                iFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;

                // *** Add Access rule for the inheritance
                AccessRule = new FileSystemAccessRule("Users", Rights,
                                            iFlags,
                                            PropagationFlags.InheritOnly,
                                            AccessControlType.Allow);
                Result = false;
                Security.ModifyAccessRule(AccessControlModification.Add, AccessRule, out Result);

                if (!Result)
                    return false;

                Info.SetAccessControl(Security);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool clearOutDir(string path)
        {
            bool value = false;

            try
            {
                DirectoryInfo di = new DirectoryInfo(path);
                FileInfo[] files = di.GetFiles();

                foreach (FileInfo item in files)
                {
                    loggingClass.logEntryWriter($"{item.Name} will be deleted for upgrade", "info");
                    item.Delete();
                }

                value = true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Access to the path"))
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    FileInfo[] files = di.GetFiles();
                    Thread.Sleep(1000);

                    foreach (FileInfo item in files)
                    {
                        loggingClass.logEntryWriter($"{item.Name} will be deleted for upgrade", "info");
                        item.Delete();
                    }
                }

                loggingClass.logEntryWriter(ex.ToString(), "error");

                value = false;
            }

            return value;
        }
    }
}