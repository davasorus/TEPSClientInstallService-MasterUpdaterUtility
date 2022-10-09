using System;
using System.IO;
using System.IO.Compression;

namespace TEPSClientInstallService_UpdateUtility.Classes
{
    internal class compressionClass
    {
        private loggingClass loggingClass = new loggingClass();

        private readonly string serviceBackUpPath = @"C:\ProgramData\Tyler Technologies\Public Safety\Tyler-Client-Install-Master-Service\BackUps";

        //does the actual compression of found folders
        //will get the full folder path and then create substrings to compare against the entered state ID
        //if the substring starts with the state ID the folder is compressed and labeled: ORI + Date time(yyyyMMdd_HHmmss) + machine name
        //dropped into the NWS Hold\Field Reporting\Back Ups Folder
        //   if that folder doesn't exist it is created and the process is started a new.
        public void compression(string startPath, string zipPath)
        {
            try
            {
                loggingClass.logEntryWriter($"backing up {startPath} to {zipPath}", "info");

                ZipFile.CreateFromDirectory(startPath, zipPath);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("_-1_.zip"))
                {
                    createDir(serviceBackUpPath);
                }
                else
                {
                    string logEntry = ex.ToString();

                    loggingClass.logEntryWriter(logEntry, "error");
                    //loggingClass.ezViewLogWriter(logEntry);
                    //loggingClass.remoteErrorReporting("ORI Copy Utility", Assembly.GetExecutingAssembly().GetName().Version.ToString(), ex.ToString(), "Automated Error Reported by " + Environment.UserName);
                }
            }
        }

        //this is used to create directory's
        public void createDir(string folder)
        {
            try
            {
                DirectoryInfo di = Directory.CreateDirectory(folder);

                string logEntry = folder + " was created.";
                loggingClass.logEntryWriter(logEntry, "info");
                //loggingClass.ezViewLogWriter(logEntry);
            }
            catch (Exception ex)
            {
                string logEntry = ex.ToString();

                loggingClass.logEntryWriter(logEntry, "error");
                //loggingClass.ezViewLogWriter(logEntry);
                //loggingClass.remoteErrorReporting("ORI Copy Utility", Assembly.GetExecutingAssembly().GetName().Version.ToString(), ex.ToString(), "Automated Error Reported by " + Environment.UserName);
            }
        }

        //this will decompress the file found in start path to zip path
        //once this is done the file is relabeled to the correct ORI
        public void decompress(string zipPath, string extractPath)
        {
            try
            {
                //ZipFile.ExtractToDirectory(startPath, zipPath);

                // Normalizes the path.
                extractPath = Path.GetFullPath(extractPath);

                // Ensures that the last character on the extraction path
                // is the directory separator char.
                // Without this, a malicious zip file could try to traverse outside of the expected
                // extraction path.
                if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                    extractPath += Path.DirectorySeparatorChar;

                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        // Gets the full path to ensure that relative segments are removed.
                        string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                        // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                        // are case-insensitive.
                        if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                            entry.ExtractToFile(destinationPath);
                    }
                }
            }
            catch (Exception ex)
            {
                string logEntry = ex.ToString();

                loggingClass.logEntryWriter(logEntry, "error");
                //loggingClass.ezViewLogWriter(logEntry);
                //loggingClass.remoteErrorReporting("ORI Copy Utility", Assembly.GetExecutingAssembly().GetName().Version.ToString(), ex.ToString(), "Automated Error Reported by " + Environment.UserName);
            }
        }

        public void decompress1(string zipPath, string extractPath)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string fullPath = Path.Combine(extractPath, entry.FullName);
                    if (String.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                    else
                    {
                        if (entry.FullName.Contains("Resources"))
                        {
                            entry.ExtractToFile(fullPath,true);
                        }
                    }
                }
            }
        }
    }
}