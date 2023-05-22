using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSGETIP
{
    internal static class WindowsHelper
    {

        public static bool StartWithUser { 
            get 
            {
                RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                return (rkApp.GetValue(ExeNameWithoutExtencion()) == null) ? false : true;
            } 
            set 
            {
                
                if (value)
                {
                    RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    string exePath = Process.GetCurrentProcess().MainModule.FileName;
                    rkApp.SetValue(ExeNameWithoutExtencion(), exePath);
                }
                else
                {
                    RegistryKey currentUserKey = Registry.CurrentUser;
                    RegistryKey runKey = currentUserKey.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                    runKey.DeleteValue(ExeNameWithoutExtencion(), false);
                    runKey.Close();
                    currentUserKey.Close();
                }
            } 
        }

        private static string ExeNameWithoutExtencion()
        {
            string exeNameWithoutExt = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
            return exeNameWithoutExt;
        }
    }
}
