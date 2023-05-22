using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Forms.Application;

namespace LSGETIP
{
    internal class Program
    {

        static NotifyIcon notifyIcon;
        static MenuItem menuItemIsConectado;
        static AppStatus appStatus;
        static GetIp getIp;
        
        //static InternetChecker checker;

        static void GetIp_StatusChanged(object sender, bool status)
        {
            appStatus = (AppStatus)sender;
            Console.WriteLine(appStatus.ToString());

            if (status)
            {
                Console.WriteLine("Internet is on");
                menuItemIsConectado.Text = "Connected (" + appStatus.Address + ")";
                menuItemIsConectado.Enabled = true;
                AppConfig.SetString("addr", appStatus.Address);
            }
            else
            {
                Console.WriteLine("Internet is off");
                menuItemIsConectado.Text = "Disconnected";
                menuItemIsConectado.Enabled = false;
            }

        }

        static void Main(string[] args)
        {
            AppConfig.SetConfigFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.xml"));

            getIp = new GetIp();
            getIp.StatusChanged += GetIp_StatusChanged;
            //getIp.Refresh();
            getIp.Enabled = true;

            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lsgetip.ico");
            Icon icon = new Icon(iconPath);

            notifyIcon = new NotifyIcon()
            {
                //Icon = SystemIcons.Information,
                Icon = icon,
                Text = "Get IP by Lopes & Souto - Firewall Authentication",
                Visible = true
            };

            menuItemIsConectado = new MenuItem("Disconnected");
            menuItemIsConectado.Enabled = false;
            menuItemIsConectado.Click += MenuItemIsConectado_Click;

            var contextMenu = new ContextMenu();

            contextMenu.MenuItems.Add(menuItemIsConectado);
            contextMenu.MenuItems.Add("-");
            
            var checkBoxMenuItem = new MenuItem("Start With O.S.", OnHabilitarClick);
            checkBoxMenuItem.Checked = WindowsHelper.StartWithUser;
            contextMenu.MenuItems.Add(checkBoxMenuItem);
            contextMenu.MenuItems.Add("Configurations", configuraçõesToolStripMenuItem_Click);
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add("Exit", OnExitClick);

            notifyIcon.ContextMenu = contextMenu;

            //checker.Enabled = true;
            //checker.StartChecking();

            Application.Run();

            //ShowStatistics(NetworkInterfaceComponent.IPv4);
            //ShowStatistics(NetworkInterfaceComponent.IPv6);

            //TestInternetConnection("www.lopessouto.com.br");
            //if (IsConnectionAvailable("https://www.lopessouto.com.br"))
            //{
            //    Console.WriteLine("Internet is on");
            //}
            //else
            //{
            //    Console.WriteLine("Internet is off");
            //}

            //NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
            //NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;

            //Console.WriteLine(
            //    "Listening changes in network availability. Press any key to continue.");
            //Console.ReadLine();

            //NetworkChange.NetworkAvailabilityChanged -= OnNetworkAvailabilityChanged;
            //NetworkChange.NetworkAddressChanged -= OnNetworkAddressChanged;
        }

        private static void MenuItemIsConectado_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Copy to CLipboard?", "Copy", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                Clipboard.SetText(appStatus.Address);
            }
        }

        static void configuraçõesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Criar uma instância do formulário de configurações
            frmConfig configuracoesForm = new frmConfig();

            configuracoesForm.txtUrl.Text = AppConfig.GetString("url", "https://www.lopessouto.com.br/getip.asp");
            configuracoesForm.txtLogin.Text = AppConfig.GetString("login", "");
            configuracoesForm.txtSenha.Text = AppConfig.GetString("pass", "");
            configuracoesForm.txtName.Text = AppConfig.GetString("name", "");
            configuracoesForm.txtPort.Text = AppConfig.GetString("port", "8080");
            configuracoesForm.txtMin.Value = AppConfig.GetInt("MinIntervalSeconds", 30);
            configuracoesForm.txtMax.Value = AppConfig.GetInt("MaxIntervalSeconds", 180);


            // Exibir o formulário de configurações como um diálogo modal
            DialogResult result = configuracoesForm.ShowDialog();


            // Verificar se o usuário clicou no botão "Salvar"
            if (result == DialogResult.OK)
            {
                // Salvar as configurações
                AppConfig.SetString("url", configuracoesForm.txtUrl.Text);
                AppConfig.SetString("login", configuracoesForm.txtLogin.Text);
                AppConfig.SetString("pass", configuracoesForm.txtSenha.Text);
                AppConfig.SetString("name", configuracoesForm.txtName.Text);
                AppConfig.SetString("port", configuracoesForm.txtPort.Text);
                AppConfig.SetString("port", configuracoesForm.txtPort.Text);
                AppConfig.SetString("MinIntervalSeconds", configuracoesForm.txtMin.Value.ToString());
                AppConfig.SetString("MaxIntervalSeconds", configuracoesForm.txtMax.Value.ToString());
            }
        }

        static void OnHabilitarClick(object sender, EventArgs e)
        {
            var mnuHabilitar = (MenuItem)sender;

            if (mnuHabilitar.Checked)
            {
                mnuHabilitar.Checked = false;
                WindowsHelper.StartWithUser = false;
            }
            else
            {
                mnuHabilitar.Checked = true;
                WindowsHelper.StartWithUser = true;
            }
        }

        static void OnExitClick(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //static void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs networkAvailability)
        //{
        //    Console.WriteLine($"Network is available: {networkAvailability.IsAvailable}");
        //    if (IsConnectionAvailable("https://www.lopessouto.com.br"))
        //    {
        //        Console.WriteLine("Internet is on");
        //    }
        //    else
        //    {
        //        Console.WriteLine("Internet is off");
        //    }
        //}

        //static void OnNetworkAddressChanged(object sender, EventArgs args)
        //{
        //    NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        //    foreach (NetworkInterface n in adapters)
        //    {
        //        Console.WriteLine("NetworkMonitor {0}", String.Format("{0} is {1}", n.Name, n.OperationalStatus));
        //    }
        //    if (IsConnectionAvailable("https://www.lopessouto.com.br"))
        //    {
        //        Console.WriteLine("Internet is on");
        //    }
        //    else
        //    {
        //        Console.WriteLine("Internet is off");
        //    }
        //}

        //static void ShowStatistics(NetworkInterfaceComponent version)
        //{

        //    IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

        //    TcpStatistics stats;

        //    if(version == NetworkInterfaceComponent.IPv4)
        //    {
        //        stats = properties.GetTcpIPv4Statistics();
        //    }
        //    else
        //    {
        //        stats = properties.GetTcpIPv6Statistics();
        //    }

        //    Console.WriteLine($"TCP/{version} Statistics");
        //    Console.WriteLine($"  Minimum Transmission Timeout : {stats.MinimumTransmissionTimeout:#,#}");
        //    Console.WriteLine($"  Maximum Transmission Timeout : {stats.MaximumTransmissionTimeout:#,#}");
        //    Console.WriteLine("  Connection Data");
        //    Console.WriteLine($"      Current :                  {stats.CurrentConnections:#,#}");
        //    Console.WriteLine($"      Cumulative :               {stats.CumulativeConnections:#,#}");
        //    Console.WriteLine($"      Initiated  :               {stats.ConnectionsInitiated:#,#}");
        //    Console.WriteLine($"      Accepted :                 {stats.ConnectionsAccepted:#,#}");
        //    Console.WriteLine($"      Failed Attempts :          {stats.FailedConnectionAttempts:#,#}");
        //    Console.WriteLine($"      Reset :                    {stats.ResetConnections:#,#}");
        //    Console.WriteLine("  Segment Data");
        //    Console.WriteLine($"      Received :                 {stats.SegmentsReceived:#,#}");
        //    Console.WriteLine($"      Sent :                     {stats.SegmentsSent:#,#}");
        //    Console.WriteLine($"      Retransmitted :            {stats.SegmentsResent:#,#}");
        //    Console.WriteLine();
        //}

        //static void TestInternetConnection(string hostNameOrAddress)
        //{
        //    using (Ping pinger = new Ping())
        //    {
        //        try
        //        {
        //            PingReply reply = pinger.Send(hostNameOrAddress);

        //            if (reply.Status == IPStatus.Success)
        //            {
        //                Console.WriteLine($"Ping to {hostNameOrAddress} successful. Response time: {reply.RoundtripTime}ms");
        //            }
        //            else
        //            {
        //                Console.WriteLine($"Ping to {hostNameOrAddress} failed. Status: {reply.Status}");
        //            }
        //        }
        //        catch (PingException ex)
        //        {
        //            Console.WriteLine($"An error occurred: {ex.Message}");
        //        }
        //    }
        //}

        //public static bool IsConnectionAvailable(string hostNameOrAddress)
        //{
        //    try
        //    {
        //        using (var client = new HttpClient())
        //        {
        //            var response = client.GetAsync(hostNameOrAddress).Result;
        //            return response.IsSuccessStatusCode;
        //        }
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
    }

}
