using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Security;
using System.Reflection;
using System.Collections;

namespace SharePointLogViewer
{
    class SPUtility
    {
        public static SPVersion SPVersion
        {
            get
            {
                try
                {
                    var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Shared Tools\Web Server Extensions\12.0");
                    if (key != null)
                        return SPVersion.SP2007;
                    key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Shared Tools\Web Server Extensions\14.0");
                    if (key != null)
                        return SPVersion.SP2010;
                }
                catch (SecurityException){}                 
                return SPVersion.Unknown;
            }
        }

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

        public static string LogsLocation
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
                    lastAccessedFile = GetLastAccessedFile(LogsLocation);

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

        public static IEnumerable<string> GetServerNames()
        {
            Type farmType = null;

            if(SPUtility.SPVersion == SPVersion.SP2007)
                farmType = Type.GetType("Microsoft.SharePoint.Administration.SPFarm, Microsoft.SharePoint, Version=12.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c");
            else if(SPUtility.SPVersion == SPVersion.SP2010)
                farmType = Type.GetType("Microsoft.SharePoint.Administration.SPFarm, Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c");

            if (farmType != null)
            {
                PropertyInfo propLocalFarm = farmType.GetProperty("Local", BindingFlags.Public | BindingFlags.Static);
                object localFarm = propLocalFarm.GetValue(null, null);
                PropertyInfo propServers = localFarm.GetType().GetProperty("Servers", BindingFlags.Public | BindingFlags.Instance);
                IEnumerable servers = (IEnumerable)propServers.GetValue(localFarm, null);
                foreach (object server in servers)
                {
                    PropertyInfo propServerName = server.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
                    string serverName = (string)propServerName.GetValue(server, null);
                    yield return serverName;
                }
            }
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
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Shared Tools\Web Server Extensions\14.0");
            return key;
        }        
    }
}
