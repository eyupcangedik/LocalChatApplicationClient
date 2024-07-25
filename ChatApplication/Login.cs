using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatAppV4
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
            this.Icon = new Icon(@"C:\ChatApplication\user.ico");
            this.FormClosing += Login_FormClosing;

            this.MaximumSize = new Size(290, this.Height);
            this.MinimumSize = new Size(290, this.Height);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string password = textBoxPassword.Text;
                if (password == "Chat1234")
                {
                    Chat chat = new Chat();
                    chat.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Hatalı Şifre", "UYARI!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "UYARI!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBoxPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                button1.PerformClick();
            }
        }

        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
