using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TEPSClientInstallService_Master_UpdateUtility.Classes;
using TEPSClientInstallService_UpdateUtility.Classes;

namespace TEPSClientInstallService_UpdateUtility
{
    internal class Program
    {
        private loggingClass loggingClass = new loggingClass();
        private MasterServiceUpdateClass agentUpdateClass = new MasterServiceUpdateClass();
        private selfUpdateClass selfUpdateClass = new selfUpdateClass();
        private serviceConfigInteractionClass serviceConfigInteractionClass = new serviceConfigInteractionClass();
        private sqlInteractionClass sqlInteractionClass = new sqlInteractionClass();
        private serviceClass serviceClass = new serviceClass();

        private static async Task Main(string[] args)
        {
            Directory.CreateDirectory(@"C:\Services\Tyler-Client-Install-Master-Service");
            Directory.CreateDirectory(@"C:\ProgramData\Tyler Technologies\Public Safety\Tyler-Client-Install-Master-Service");

            Program program = new Program();

            program.loggingClass.initializeNLogLogger();

            await program.configBackUp();
            await program.DBConfig();

            await program.utilityUpdater();

            Thread.Sleep(30000);

            await program.serviceUpdater();

            Thread.Sleep(30000);

            if (program.serviceClass.getServiceStatus("TEPS Automated Client Install Master Service") == "stopped")
            {
                program.serviceClass.startService($"TEPS Automated Client Install Master Service");
            }
        }

        private async Task serviceUpdater()
        {
            await agentUpdateClass.updateAPICheckAsync();
        }

        private async Task utilityUpdater()
        {
            await selfUpdateClass.updateAPICheckAsync();
        }

        private async Task configBackUp()
        {
            serviceConfigInteractionClass.configBackUp();

            if (string.IsNullOrEmpty(configData.DBname))
            {
                string dbValue;
                Console.WriteLine("No DB config detected. Please type in Prod DB Name - ");
                dbValue = Console.ReadLine();
                configData.DBname = dbValue;
                Console.WriteLine($"{configData.DBname} saved");
            }
        }

        private async Task DBConfig()
        {
            dbPresent.checkDB = sqlInteractionClass.checkDB();
        }
    }
}

internal class dbPresent
{
    public static bool checkDB { get; set; }
}