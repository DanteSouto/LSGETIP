using System;
using System.Configuration;

namespace LSGETIP
{
    internal static class AppConfig
    {
        private static string configFile = "app.config"; // Nome padrão do arquivo de configuração

        public static string GetString(string key, string defaultValue = null)
        {
            Configuration config = GetConfig();
            string value = config.AppSettings.Settings[key].Value;
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            Configuration config = GetConfig();
            string value = config.AppSettings.Settings[key].Value;
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            Configuration config = GetConfig();
            string value = config.AppSettings.Settings[key].Value;
            return bool.TryParse(value, out bool result) ? result : defaultValue;
        }

        public static void SetString(string key, string value)
        {
            Configuration config = GetConfig();
            config.AppSettings.Settings[key].Value = value;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public static void SetInt(string key, int value)
        {
            SetString(key, value.ToString());
        }

        public static void SetBool(string key, bool value)
        {
            SetString(key, value.ToString());
        }

        private static Configuration GetConfig()
        {
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = configFile;
            return ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        }

        public static void SetConfigFile(string fileName)
        {
            configFile = fileName;
        }
    }
}
