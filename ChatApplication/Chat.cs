using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatAppV4
{
    public partial class Chat : Form
    {
        private TcpClient client;
        private NetworkStream clientStream;
        private Thread clientThread;

        private NotifyIcon notifyIcon;
        public Chat()
        {
            InitializeComponent();
            this.MinimumSize = new Size(600, this.Height);

            this.FormClosing += Chat_FormClosing;

            labelChat.Location = new Point(
                listBoxChat.Left + listBoxChat.Width / 2 - labelChat.Width / 2,
                listBoxChat.Top - labelChat.Height - 10
            );

            labelMessage.Location = new Point(
                richTextBoxMessage.Left + richTextBoxMessage.Width / 2 - labelMessage.Width / 2,
                richTextBoxMessage.Top - labelMessage.Height - 10
            );

            ConnectToServer();
           
            InitializeNotifyIcon();

        }

        private void Chat_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void buttonSend_Click(object sender, System.EventArgs e)
        {
            string message = GetLocalIPAddress().ToString() + " : " + richTextBoxMessage.Text;
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();

            UpdateMessagesList(message);
            richTextBoxMessage.Clear();
           
        }
        private void ConnectToServer()
        {
            try
            {
                client = new TcpClient("172.16.5.72", 5000); // Server Ip , Server Port
                clientStream = client.GetStream();
                UpdateStatus("Sunucu Bağlantısı Aktif...");

                buttonSend.Enabled = true;
                richTextBoxMessage.Enabled = true;

                clientThread = new Thread(new ThreadStart(ListenForMessages));
                clientThread.IsBackground = true;
                clientThread.Start();
            }
            catch (Exception ex)
            {
                UpdateStatus("Sunucuya Bağlanırken Hata Oluştu:  " + ex.Message);
                MessageBox.Show("Sunucuya Bağlanırken Hata Oluştu. \nECG ile İletişime Geçiniz.", "Bağlantı Hatası!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }
        private void ListenForMessages()
        {
            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }

                string msg = Encoding.UTF8.GetString(message, 0, bytesRead);
                UpdateMessagesList(msg);
            }

            client.Close();
            UpdateStatus("Sunucu Bağlantısı Koptu...");
            MessageBox.Show("Sunucu Bağlantısı Koptu. \nECG ile İletişime Geçiniz.", "Bağlantı Hatası!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
        private void UpdateMessagesList(string message)
        {
            string[] parts = message.Split(':');

            // IP part and message part
            string ipPart = parts[0].Trim();
            int lengthIpPart = ipPart.Length;

            int messageLength = message.Length;

            string fullMessage = message.Substring(lengthIpPart + 1, messageLength - 1 - lengthIpPart);
            string messagePart = fullMessage;

            // We split the IP part by the '.' character and get the third and fourth segment
            string[] ipParts = ipPart.Split('.');

            string segment = ipParts[3];

            string result = FindUser(segment);

            string dateTime = DateTime.Now.ToString();

            messagePart = result + " " + messagePart.Trim() + "   " + dateTime;

            fullMessage = fullMessage.Trim();
            if(fullMessage.Length > 1)
            {
                listBoxChat.Items.Add(messagePart);
                
                if (listBoxChat.InvokeRequired)
                {
                    listBoxChat.Invoke(new MethodInvoker(delegate

                    {
                        ShowNotification(result);
                    }));
                }

                listBoxChat.TopIndex = listBoxChat.Items.Count - 1;
            }           
        }
        private void UpdateStatus(string status)
        {
            if (labelStatus.InvokeRequired)
            {
                labelStatus.Invoke(new MethodInvoker(delegate
                {
                    labelStatus.Text = status;
                }));
            }
            else
            {
                labelStatus.Text = status;
            }
        }
        public string FindUser(string ip)
        {
            switch (ip)
            {
                case "92":
                    return "User92";

                case "56":
                    return "User56";

                default:
                    return "172.16.5." + ip + " ";
            }
        }
        private string GetLocalIPAddress()
        {
            string localIP = "";
            try
            {
                // Get the host name of the computer
                string hostName = Dns.GetHostName();
                // Get IP addresses associated with host name
                IPAddress[] addresses = Dns.GetHostAddresses(hostName);

                foreach (var address in addresses)
                {
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localIP = address.ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return localIP;
        }
        private void richTextBoxMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                buttonSend.PerformClick();
            }
        }


        private void InitializeNotifyIcon()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = SystemIcons.Information; // Notification icon
            notifyIcon.Visible = true;
            notifyIcon.BalloonTipTitle = "Yeni Mesaj!";
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
        }
        private void ShowNotification(string message)
        {
            notifyIcon.BalloonTipText = message;
            notifyIcon.ShowBalloonTip(1000); // Show notification for 1 second

            this.Icon = new Icon(@"C:\ChatApplication\notification.ico");
        }


        private void buttonClear_Click(object sender, EventArgs e)
        {
            listBoxChat.Items.Clear();
        }

        private void Chat_Activated(object sender, EventArgs e)
        {
            this.Icon = new Icon(@"C:\ChatApplication\application.ico");
        }
        private void Chat_Resize(object sender, EventArgs e)
        {
            ResizeLeftItems();
            ResizeRightItems();           
        }
        public void ResizeLeftItems()
        {
            int margin = 25;

            // Get the height of the form
            int formHeight = this.ClientSize.Height;

            // Set the height of the RichTextBox by subtracting the margin from the height of the form
            int richTextBoxHeight = formHeight - richTextBoxMessage.Top - margin - buttonSend.Height - margin;

            if (richTextBoxHeight > 0)
            {
                richTextBoxMessage.Height = richTextBoxHeight;
            }

            // Set the position of ButtonSend to the bottom right corner of richTextBoxMessage
            buttonSend.Location = new Point(
                richTextBoxMessage.Right - buttonSend.Width,
                richTextBoxMessage.Bottom + 20
            );


        }
        public void ResizeRightItems()
        {
            int margin = 20;

            // Get the width and height of the form
            int formWidth = this.ClientSize.Width;
            int formHeight = this.ClientSize.Height;

            // Set the width of the ListBox by subtracting the margin from the width of the form
            int listBoxChatWidth = formWidth - listBoxChat.Left - margin;

            // Set the height of the ListBox by subtracting the margin from the form height
            int listBoxChatHeight = formHeight - listBoxChat.Top - margin - buttonClear.Height - margin;

            // Set the width and height of the ListBox
            if (listBoxChatWidth > 0)
            {
                listBoxChat.Width = listBoxChatWidth;
            }

            if (listBoxChatHeight > 0)
            {
                listBoxChat.Height = listBoxChatHeight;

            }

            // Set the position of ButtonClear to the bottom right corner of listBoxChat
            buttonClear.Location = new Point(
                listBoxChat.Right - buttonClear.Width,
                listBoxChat.Bottom + margin
            );

            // Set the position of Label1 above listBoxChat
            labelChat.Location = new Point(
                listBoxChat.Left + listBoxChat.Width / 2 - labelChat.Width / 2,
                listBoxChat.Top - labelChat.Height - 10
            );

            // Set LabelStatus position above listBoxChat
            labelStatus.Location = new Point(
                labelStatus.Location.X,
                listBoxChat.Bottom - labelStatus.Height + 47
            );
        }
    }
}
