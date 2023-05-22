using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using System.Xml.Linq;
using System.Timers;

namespace LSGETIP
{
    internal class GetIp
    {
        public event EventHandler<bool> StatusChanged;
        
        private SortedList<string,string> kvp_Apps = new SortedList<string,string>();

        private object lockObj = new object();
        private Timer timer;
        private Random random;

        private string _addr;
        private bool _status;
        private bool _enabled;

        public string Address
        {
            get { return _addr; }
        }

        public bool Status
        {
            get { return _status; }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set 
            { 
                _enabled = value; 
                timer.Enabled = _enabled;
            }
        }

        public GetIp()
        {
            _addr = string.Empty;
            _status = false;

            NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
            NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;

            random = new Random();

            timer = new Timer();
            timer.Elapsed += OnTimerElapsed;

        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            timer.Enabled = false;
            Refresh();
        }

        private void SetTimer()
        {
            // string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.xml");
            // ExeConfigurationFileMap configMap = new ExeConfigurationFileMap { ExeConfigFilename = configFilePath };
            // Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

            int minInterval = AppConfig.GetInt("MinIntervalSeconds", 5);
            int maxInterval = AppConfig.GetInt("MaxIntervalSeconds", 5); ;
            
            int interval = random.Next(minInterval, maxInterval);
            
            Console.WriteLine("Next update in {0} seconds", interval.ToString());

            timer.Interval = interval * 1000;
            timer.Enabled = _enabled;
        }

        private void UpdateStatus(string url, string addr, string login,  bool status, string name)
        {

            string fullKey = url + "_" + name + "_" + login;
            if (kvp_Apps.ContainsKey(fullKey))
            {
                if (addr != kvp_Apps[fullKey])
                {
                    kvp_Apps[fullKey] = addr;
                    _addr = addr;
                    _status = status;
                    StatusChanged?.Invoke(new AppStatus(name, login, addr, status), status);
                }
            }
            else
            {
                kvp_Apps[fullKey] = addr;
                _addr = addr;
                _status = status;
                StatusChanged?.Invoke(new AppStatus(name, login, addr, status), status);
            }
            
            SetTimer(); 
        }

        private void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs networkAvailability)
        {
            timer.Enabled = false;
            Refresh();
        }

        private void OnNetworkAddressChanged(object sender, EventArgs args)
        {
            timer.Enabled = false;
            Refresh();
        }

        public void Refresh()
        {
            string url = AppConfig.GetString("url", "");
            string login = AppConfig.GetString("login", "");
            string pass = AppConfig.GetString("pass", "");
            string name = AppConfig.GetString("name", "");
            string port = AppConfig.GetString("port", "");

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                Console.WriteLine($"A entrada '{url}' não é uma URL válida. Ignorando...");
                return;
            }

            if (login.Trim() == "" || pass.Trim() == "")
            {
                Console.WriteLine($"A entrada '{url}' não contém dados válidos. Ignorando...");
                return;
            }

            Refresh(url, login, pass, name, port);

            //string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.xml");
            //ExeConfigurationFileMap configMap = new ExeConfigurationFileMap { ExeConfigFilename = configFilePath };
            //Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

            //var appsList = new List<string>();
            //var section = config.GetSection("apps");
            //if (section != null)
            //{
            //    List<string> myApps = new List<string>();
            //    var rawXml = section.SectionInformation.GetRawXml();
            //    var doc = XDocument.Parse(rawXml);
            //    foreach (var website in doc.Descendants("app"))
            //    {
            //        string url = website.Attribute("url").Value;
            //        string login = website.Attribute("login").Value;
            //        string pass = website.Attribute("pass").Value;
            //        string name = website.Attribute("name") == null ? "" : website.Attribute("name").Value;
            //        string port = website.Attribute("port") == null ? "" : website.Attribute("port").Value;

            //        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            //        {
            //            Console.WriteLine($"A entrada '{url}' não é uma URL válida. Ignorando...");
            //            continue;
            //        }

            //        if (login.Trim() == "" || pass.Trim() == "")
            //        {
            //            Console.WriteLine($"A entrada '{url}' não contém dados válidos. Ignorando...");
            //            continue;
            //        }

            //        Refresh(url, login, pass, name, port);

            //    }
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="nome"></param>
        /// <param name="auth"></param>
        /// <param name="app"></param>
        private void Refresh(string url, string nome, string auth, string app, string port)
        {
            if (IsInternetAvailable())
            {
                System.Threading.Tasks.Task doIt = new System.Threading.Tasks.Task(async () =>
                {
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("nome", nome),
                        new KeyValuePair<string, string>("auth", auth),
                        new KeyValuePair<string, string>("app", app),
                        new KeyValuePair<string, string>("port", port)
                    });

                    using (var client = new HttpClient())
                    {
                        try
                        {
                            HttpResponseMessage response = await client.PostAsync(url, content);
                            response.EnsureSuccessStatusCode();
                            string responseString = await response.Content.ReadAsStringAsync();
                            UpdateStatus(url, responseString.Trim(), nome, true, app);
                        }
                        catch (HttpRequestException ex)
                        {
                            // Lidar com erros de rede aqui
                            Console.WriteLine("The HTTP response was unsuccessful: " + ex.Message);
                            UpdateStatus(url, string.Empty, nome, false, app);
                        }
                        catch (Exception ex)
                        {
                            // Lidar com outros erros aqui
                            Console.WriteLine("Generic Error: " + ex.Message);
                            UpdateStatus(url, string.Empty, nome, false, app);
                        }
                    }
                });
                doIt.Start();
            }
            else
            {
                UpdateStatus(url, string.Empty, nome, false, app);
            }
        }

        private bool IsInternetAvailable()
        {
            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (var iface in interfaces)
                {
                    if (iface.OperationalStatus == OperationalStatus.Up)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
