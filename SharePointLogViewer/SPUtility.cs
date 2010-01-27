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
                    RegistryKey key = Registry.LocalMachine.OpenSubKey( @"SOFTWARE\Microsoft\Shared Tools\Web Server Extensions\12.0");
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
                    using (RegistryKey key = GetSharePointRegistryKey())
                        if (key != null)
                        {
                            string versionStr = key.GetValue("BuildVersion") as string;
                            if (versionStr != null)
                            {
                                Version buildVersion = new Version(versionStr);
                                if (buildVersion.Major == 12)
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
                string logsPath = SharePointInstallPath;
                if (logsPath != String.Empty)
                    logsPath = Path.Combine(logsPath, "logs");

                return logsPath;
            }
        }

        public static string SharePointInstallPath
        {
            get
            {
                string installPath = String.Empty;
                try
                {
                    using (RegistryKey key = GetSharePointRegistryKey())
                        if (key != null)
                            installPath = key.GetValue("InstallPath").ToString();
                }
                catch (SecurityException) { }
                return installPath;
            }
        }

        static RegistryKey GetSharePointRegistryKey()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Office Server\12.0");
            return key;
        }
    }
}
