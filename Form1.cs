using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModBus_Library;

namespace Modbus
{
    public partial class Form1 : Form
    {
        ModBus_Libra PortA = new ModBus_Libra(new SerialPort());
        List<string> Data = new List<string>();

        public Form1()
        {
            InitializeComponent();
            COMPort.Items.AddRange(SerialPort.GetPortNames());
            COMPort.Text = Properties.Settings.Default.ComPort;
            textBox3.Text = Properties.Settings.Default.Speed.ToString();
            Close.Enabled = false;
            Open.Enabled = true;

            textBox1.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Transmit_Click(null, null); };
            Refresh.Click += (s, e) => { COMPort.Items.Clear(); COMPort.Items.AddRange(SerialPort.GetPortNames()); };
            Close.Click += (s, e) => { PortA.Close(); Open.Enabled = true; Close.Enabled = false; };
            COMPort.TextChanged += (s, e) => { Properties.Settings.Default.ComPort = COMPort.Text;Properties.Settings.Default.Save(); };
            textBox3.TextChanged += (s, e) =>
            {   
                try
                {
                    int a = Convert.ToInt32(textBox3.Text);
                    Properties.Settings.Default.Speed = a;
                    Properties.Settings.Default.Save();
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            };
            Open.Click += (s, e) =>
            {
                try
                {
                    PortA.Name = COMPort.Text;
                    PortA.Speed = Convert.ToInt32(textBox3.Text);
                    PortA.Open();
                    Open.Enabled = false; Close.Enabled = true;
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            };

            PortA.Receive_Event +=()=> BeginInvoke((MethodInvoker)(() =>
            {
                Data.Add(PortA.Receive_Array);
                textBox2.Lines = Data.ToArray();
                textBox2.SelectionStart = textBox2.TextLength;
                textBox2.ScrollToCaret();
                while (Data.Count > 100) Data.RemoveAt(0);
                label1.Text = PortA.Result[0].ToString();
            }));
            PortA.Transmit_Event += () => BeginInvoke((MethodInvoker)(() =>
            {
                Data.Add(PortA.Transmit_Array);
                textBox2.Lines = Data.ToArray();
                textBox2.SelectionStart = textBox2.TextLength;
                textBox2.ScrollToCaret();
                while (Data.Count > 100) Data.RemoveAt(0);
            }));
        }

        private void Transmit_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0) return;
            
            string asa = textBox1.Text.Replace(" ", string.Empty);
            byte[] send = new byte[asa.Length / 2];
            for (int a = 0; a < asa.Length; a += 2)
            {
                try
                {
                    send[a / 2]=Convert.ToByte(asa.Substring(a, 2), 16);

                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            PortA.Transmit(send);
        }
    }
}
