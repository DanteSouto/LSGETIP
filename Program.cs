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

            Application.Run();

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

            configuracoesForm.txtUrl.Text = AppConfig.GetString("url", "");
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

                getIp.Refresh();
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

    }

}
