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
    public partial class HardwareFenster : Form
    {
        private static bool _exit = false;
        private static IConnection _connection;
        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true)]
        //AllocConsole wird für die Konsolenausgabe verwendet.
        private static extern bool AllocConsole();
        private static int _messagecount = 30;

        public HardwareFenster()
        {
            InitializeComponent();
        }


        // Diese Methode stellt eine Verbindung zum NATS-Server her und gibt die Verbindung zurück.
        private IConnection ConnectToNats()
        {
            ConnectionFactory factory = new ConnectionFactory();

            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = "nats://localhost:4222";

            IConnection connection = null;

            try
            {
                connection = factory.CreateConnection(options);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Konnte keine Verbindung zum NATS Server aufbauen. Stellen Sie sicher, dass der NATS Client läuft.", ex);
            }

            return connection;
        }


        //Sie stellt eine Verbindung zu einem NATS-Server her, öffnet eine Konsole und erwartet eine Eingabe für Hardware-Informationen.
        private void button1_Click(object sender, EventArgs e)
        {

            using (_connection = ConnectToNats())
            {
                this.Close();
                Process.Start("AVG_Team7.exe");
                AllocConsole();

                Console.WriteLine("Hardware:");
                string hardware = Console.ReadLine();

                Console.Clear();
                Console.WriteLine("Verbindung hergestellt");
                Console.WriteLine("HardwareProducer gestartet.");
                Console.WriteLine($"Hardware: {hardware}");

                for (int i = 1; i < _messagecount; i++)
                {
                    Random rnd = new Random();
                    string message = Guid.NewGuid().ToString("D").ToUpper();
                    Console.WriteLine($"Hardware-Id: {message}");
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    _connection.Publish(hardware, data);
                    Thread.Sleep(6000);

                }
            }
        }


        //Die Methode stellt eine Verbindung zu einem NATS-Server her, öffnet eine Konsole und erwartet eine Eingabe für eine Hardware-ID.
        //Dann wird die Methode Subscribe aufgerufen, um auf eingehende Nachrichten für die angegebene Hardware-ID zu hören.
        private void button2_Click(object sender, EventArgs e)
        {
            using (_connection = ConnectToNats())
            {
                this.Hide();
                Process.Start("AVG_Team7.exe");
                AllocConsole();

                Console.WriteLine("Hardware:");
                string hardware = Console.ReadLine();
                Subscribe(hardware);

                Console.Clear();
                Console.WriteLine("Verbindung hergestellt");
                Console.WriteLine("HardwareConsumer gestartet.");
                Console.WriteLine($"Hardware-Id: {hardware}");
                Console.ReadKey(true);
                _exit = true;

                _connection.Drain(5000);
            }
        }


        //Diese Methode abonniert ein bestimmtes NATS-Topic für eingehende Nachrichten und führt die Methode LogMessage aus, wenn eine neue Nachricht empfangen wird.
        public static void Subscribe(string id)
        {
            EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
            {
                string data = Encoding.UTF8.GetString(args.Message.Data);
                LogMessage($"Hardware-Id: {data}");
            };
            IAsyncSubscription s = _connection.SubscribeAsync(id, handler);
        }


        //Diese Methode schreibt eine formatierte Nachricht auf die Konsole, einschließlich des aktuellen Zeitstempels und der übergebenen Nachricht.
        private static void LogMessage(string message)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.fffff")}] [DEBUG] \u001b{message}\u001b");
        }


        //Wenn das Kontrollkästchen aktiviert ist, wird die Methode SendConfirmation() aufgerufen, um eine Bestellbestätigung per E-Mail zu senden.
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                // Dialogbox öffnen und Empfänger auswählen
                string recipientEmail = "user@h-ka.de";
                string orderId = "51E6BE52-29EC-4BFD-AD42-3582B8576BB5";

                SendConfirmation(orderId, recipientEmail);
            }
        }


        //Diese Methode sendet eine Bestellbestätigungs-E-Mail an eine angegebene E-Mail-Adresse. 
        //Sie verwendet den SMTP-Server von Gmail und erfordert Anmeldeinformationen für ein Gmail-Konto.
        //Wenn die E-Mail erfolgreich gesendet wurde, wird eine Bestätigungsdialogbox angezeigt.
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
