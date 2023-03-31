using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AVG_Team7
{
    public partial class Hauptfenster : Form
    {
        public Hauptfenster()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SoftwareFenster sf = new SoftwareFenster();
            sf.Show();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            HardwareFenster hf = new HardwareFenster();
            hf.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string input = textBox1.Text;

            if (input.ToLower() == "windows")
            {
                SoftwareFenster sf = new SoftwareFenster();
                sf.Show();
            }
            else if (input.ToLower() == "festplatte")
            {
                HardwareFenster hf = new HardwareFenster();
                hf.Show();
            }
            else
            {
                MessageBox.Show("Invalid input");
            }
        }
    }
}
