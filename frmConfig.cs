using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LSGETIP
{
    public partial class frmConfig : Form
    {
        public frmConfig()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
        }

        private void txtMin_ValueChanged(object sender, EventArgs e)
        {
            txtMax.Minimum = txtMin.Value + 1;
        }

        private void bynCancel_Click(object sender, EventArgs e)
        {
            // Descartar as mudanças

            // Definir o resultado do diálogo como Cancel
            this.DialogResult = DialogResult.Cancel;

            // Fechar o formulário
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            
            if (!Uri.TryCreate(txtUrl.Text, UriKind.Absolute, out Uri uriResult) || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                errorProvider1.SetError(txtUrl, "Url inválida");
                return;
            }

            // Definir o resultado do diálogo como OK
            this.DialogResult = DialogResult.OK;

            // Fechar o formulário
            this.Close();
        }

        private void txtUrl_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void txtUrl_Validating(object sender, CancelEventArgs e)
        {
            errorProvider1.SetError(txtUrl, string.Empty);
        }
    }
}
