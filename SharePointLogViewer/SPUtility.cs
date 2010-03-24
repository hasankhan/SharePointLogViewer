using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Security;

namespace SharePointLogViewer
{
    class SPUtility
    {
        public static bool IsWSSInstalled
        {
            get
            {
                try
                {
                    RegistryKey key = GetWSSRegistryKey();
                    if (key != null)
                    {
                        object val = key.GetValue("SharePoint");
                        if (val != null && val.Equals("Installed"))
                            return true;
                    }
                }
                catch (SecurityException) { }
                return false;
            }
        }

        public static bool IsMOSSInstalled
        {
            get
            {
                try
                {
                    using (RegistryKey key = GetMOSSRegistryKey())
                        if (key != null)
                        {
                            string versionStr = key.GetValue("BuildVersion") as string;
                            if (versionStr != null)
                            {
                                Version buildVersion = new Version(versionStr);
                                if (buildVersion.Major == 12 || buildVersion.Major == 14)
                                    return true;
                            }
                        }
                }
                catch (SecurityException) {}
                return false;
            }
        }        

        public static string LogsLocations
        {
            get 
            {
                string logsPath = WSSInstallPath;
                if (logsPath != String.Empty)
                    logsPath = Path.Combine(logsPath, "logs");

                return logsPath;
            }
        }

        public static string LatestLogFile
        {
            get
            {
                string lastAccessedFile = null;
                if (IsWSSInstalled)
                    lastAccessedFile = GetLastAccessedFile(LogsLocations);

                return lastAccessedFile;
            }
        }
        public static string WSSInstallPath
        {
            get
            {
                string installPath = String.Empty;
                try
                {
                    using (RegistryKey key = GetWSSRegistryKey())
                        if (key != null)
                            installPath = key.GetValue("Location").ToString();
                }
                catch (SecurityException) { }
                return installPath;
            }
        }

        public static string GetLastAccessedFile(string folderPath)
        {
            var dirInfo = new DirectoryInfo(folderPath);
            var file = dirInfo.GetFiles().OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
            if (file != null)
                return file.FullName;
         
            return null;
        }

        static RegistryKey GetMOSSRegistryKey()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Office Server\12.0");
            if (key == null)
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Office Server\14.0");
            return key;
        }
       
        static RegistryKey GetWSSRegistryKey()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Shared Tools\Web Server Extensions\12.0");
            if (key == null)
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Shared Tools\Web Server Extensions\14.0");
            return key;
        }
    }
}
