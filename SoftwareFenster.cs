using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NATS.Client;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Net.Mail;

namespace AVG_Team7
{
    public partial class SoftwareFenster : Form

    {
        private static bool _exit = false;
        private static IConnection _connection;
        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true)]
        private static extern bool AllocConsole();
        private static int _messagecount = 30;
        public SoftwareFenster()
        {
            InitializeComponent();
        }
        private IConnection ConnectToNats()
        {
            ConnectionFactory factory = new ConnectionFactory();

            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = "nats://localhost:4222";

            return factory.CreateConnection(options);
        }
        private void button1_Click(object sender, EventArgs e)
        {

            using (_connection = ConnectToNats())
            {
                this.Close();
                Process.Start("AVG_Team7.exe");
                AllocConsole();

                Console.WriteLine("Software:");
                string software = Console.ReadLine();

                Console.Clear();
                Console.WriteLine("SoftwareProducer gestartet.");
                Console.WriteLine($"Software: {software}");

                for (int i = 1; i < _messagecount; i++)
                {
                    Random rnd = new Random();
                    string message = Guid.NewGuid().ToString("D").ToUpper();
                    Console.WriteLine($"Software-Id: {message}");

                    byte[] data = Encoding.UTF8.GetBytes(message);

                    _connection.Publish(software, data);

                    Thread.Sleep(6000);

                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (_connection = ConnectToNats())
            {
                this.Close();
                Process.Start("AVG_Team7.exe");
                AllocConsole();

                Console.WriteLine("Software:");
                string software = Console.ReadLine();
                Subscribe(software);

                Console.Clear();
                Console.WriteLine("SoftwareConsumer gestartet.");
                Console.WriteLine($"Software-Id: {software}");
                Console.ReadKey(true);
                _exit = true;

                _connection.Drain(5000);
            }
        }

        public static void Subscribe(string id)
        {
            EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
            {
                string data = Encoding.UTF8.GetString(args.Message.Data);
                LogMessage($"Hardware-Id: {data}");

            };

            IAsyncSubscription s = _connection.SubscribeAsync(id, handler);
        }
        private static void LogMessage(string message)
        {
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fffff")} - {message}");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                // Dialogbox öffnen und Empfänger auswählen
                string recipientEmail = "user@h-ka.de"; // Hier die E-Mail-Adresse des Empfängers eintragen
                string orderId = "12345"; // Hier die Bestell-ID eintragen

                SendConfirmation(orderId, recipientEmail);
            }
        }
        private void SendConfirmation(string orderId, string recipientEmail)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();

                message.From = new MailAddress("AvGHQ@h-ka.de");
                message.To.Add(new MailAddress(recipientEmail));
                message.Subject = "Bestellbestätigung #" + orderId;
                message.Body = "Vielen Dank für Ihre Bestellung #" + orderId + ". Wir werden sie schnellstmöglich bearbeiten.";

                smtp.Host = "sandbox.smtp.mailtrap.io";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("68652141fadc9f", "0c53c2da1f7074");
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.EnableSsl = true;
                smtp.Send(message);

                MessageBox.Show("Bestellbestätigung an " + recipientEmail + " gesendet.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Senden der Bestellbestätigung: " + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
