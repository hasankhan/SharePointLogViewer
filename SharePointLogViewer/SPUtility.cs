using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Administration;
using System.IO;

namespace SharePointLogViewer
{
    class SPUtility
    {
        public static string GetLogsLocations()
        {
            try
            {
                SPDiagnosticsService service = new SPDiagnosticsService("log", SPWebService.ContentService.Farm);
                return service.LogLocation;
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }
    }
}
