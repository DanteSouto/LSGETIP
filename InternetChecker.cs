using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Timers;
using System.Configuration;
using System.IO;
using System.Collections.Specialized;
using System.Xml.Linq;
using System.Linq;
using System.Security.Policy;

namespace LSGETIP
{
    internal class InternetChecker
    {
        
        public event EventHandler<bool> StatusChanged;

        private object lockObj = new object();
        private Timer timer;
        private Random random;
        private List<string> sites = new List<string>();
        private int minInterval = 6;
        private int maxInterval = 59;
        private bool internetAvailable;
        private bool isLocked = false;

        public InternetChecker()
        {
            internetAvailable = false;
            minInterval = 4;
            maxInterval = 59;

            var configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.xml");
            var configMap = new ExeConfigurationFileMap { ExeConfigFilename = configFilePath };
            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

            // Inicializa a lista de sites para verificar
            bool haslValidUrls = false;
            var websites = new List<string>();
            var section = config.GetSection("websites");
            if (section != null)
            {
                var rawXml = section.SectionInformation.GetRawXml();
                var doc = XDocument.Parse(rawXml);
                websites = doc.Descendants("website").Select(x => x.Attribute("url").Value).ToList();
                
                foreach (var website in websites)
                {
                    if (!Uri.TryCreate(website, UriKind.Absolute, out Uri uriResult) ||
                        (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                    {
                        Console.WriteLine($"A entrada '{website}' não é uma URL válida. Ignorando...");
                    }
                    else
                    {
                        sites.Add(website);
                        haslValidUrls = true;
                    }
                }
            }

            if(!haslValidUrls)
            {
                sites.Add("https://www.google.com/");
                sites.Add("https://www.facebook.com/");
                sites.Add("https://www.microsoft.com/");
                sites.Add("https://www.amazon.com/");
                sites.Add("https://www.twitter.com/");
            }

            var intervalConfig = config.AppSettings.Settings;
            if (intervalConfig["MinIntervalSeconds"] != null)
            {
                int.TryParse(intervalConfig["MinIntervalSeconds"].Value, out minInterval);
            }
            if (intervalConfig["MaxIntervalSeconds"] != null)
            {
                int.TryParse(intervalConfig["MaxIntervalSeconds"].Value, out maxInterval);
            }

            minInterval = minInterval * 1000;
            maxInterval = maxInterval * 1000;

            // Inicializa o Timer e o gerador de números aleatórios
            timer = new Timer();
            timer.Elapsed += OnTimerElapsed;
            random = new Random();
            timer.Enabled = false;

            NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
            NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;

        }

        public bool Status
        {
            get { return internetAvailable; }
        }

        public bool Enabled 
        {
            get
            {
                return timer.Enabled;
            }
            set
            {
                if(value)
                {
                    timer.Interval = random.Next(minInterval, maxInterval);
                    timer.AutoReset = true;
                    timer.Enabled = true;
                }
                else
                {
                    timer.Enabled = false;
                    UpdateStatus(false);
                }
                
            }
        }

        private void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs networkAvailability)
        {
            Console.WriteLine($"Network is available: {networkAvailability.IsAvailable}");
            if (timer.Enabled && !isLocked)
            {
                StartChecking();
            }
        }

        private void OnNetworkAddressChanged(object sender, EventArgs args)
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface n in adapters)
            {
                Console.WriteLine("NetworkMonitor {0}", String.Format("{0} is {1}", n.Name, n.OperationalStatus));
            }
            if (timer.Enabled && !isLocked)
            {
                StartChecking();
            }
        }

        public void StartChecking()
        {
            lock (lockObj)
            {
                isLocked = true;
                bool siteAvailable = false;

                if (IsInternetAvailable())
                {
                    var websites = new List<string>();
                    websites.AddRange(sites);

                    Random rand = new Random();
                    while (websites.Count > 0)
                    {
                        int index = rand.Next(websites.Count);
                        string site = websites[index];

                        siteAvailable = IsSiteAvailable(site);

                        if (siteAvailable)
                        {
                            Console.WriteLine("O site {0} está disponível.", site);
                            break;
                        }
                        else
                        {
                            Console.WriteLine("O site {0} não está disponível.", site);
                        }

                        websites.RemoveAt(index);

                    }
                }
                else
                {
                    Console.WriteLine("Internet indisponível.");
                }

                UpdateStatus(siteAvailable);
                isLocked = false;
            }
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            StartChecking();
            
            // Configura o Timer para verificar novamente em um intervalo randomizado entre 5 e 60 segundos
            timer.Interval = random.Next(minInterval, maxInterval);
        }

        private void UpdateStatus(bool newStatus)
        {
            if (internetAvailable != newStatus)
            {
                internetAvailable = newStatus;
                StatusChanged?.Invoke(this, newStatus);
            }
        }

        private bool IsSiteAvailable(string url)
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead(url))
                {
                    return true;
                }
            }
            catch
            {
                return false;
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
