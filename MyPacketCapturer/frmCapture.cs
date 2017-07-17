using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PacketDotNet;
using SharpPcap;

namespace MyPacketCapturer
{
    public partial class frmCapture : Form
    {
        CaptureDeviceList devices; //List of devices for this computer
        public static ICaptureDevice device; //The device used to capture
                                             //public static string stringPackets = ""; //Captured data
        public static int numIP4 = 0;
        public static int numIP6 = 0;
        public static int numARP = 0;
        public static int numOther = 0;

        public static int numUDP = 0;
        public static int numTCP = 0;
        public static int numICMP = 0;
        public static int numIGMP = 0;
        public static int numOtherIP = 0;

        public static Boolean running = false;//true; //originally true. hopefully this won't break it
        static int numPackets = 0;
        static int numIPPackets = 0;
        public static Boolean hasPacketData = false;
        public static Boolean hasIPData = false;
        public static String prevGraph = "";
        //frmSend fSend; //Send form

        public frmCapture()
        {
            InitializeComponent();

            //Get list of devices
            devices = CaptureDeviceList.Instance;

            //Make sure there is at least one device
            if(devices.Count < 1)
            {
                MessageBox.Show("No Capture Devices Found!!!!!!!!!!");
                Application.Exit();
            }

            //Add devices to combo box
            foreach (ICaptureDevice dev in devices)
            {
                cmbDevices.Items.Add(dev.Description);
            }

            cmbDevices.Text = "Choose a device.";

           
        }

        private static void device_OnPacketArrival(Object sender, CaptureEventArgs packet)
        {
            //increment number of packets captured
            numPackets++;
            Boolean isIP = false;
            //Boolean protocolRetrieved = false;
            //Put packet number in the capture window
            //stringPackets += "Packet Number: " + Convert.ToString(numPackets);
            //stringPackets += Environment.NewLine;

            //Array to store our data
            byte[] data = packet.Packet.Data;

            //Keep track of the number of bytes displayed per line
            //int byteCounter = 0;


           // stringPackets += "Destination MAC Address: ";
            //Parsing the packets
            //foreach (byte b in data)
            //{
            //    //Add the byte to our string (in hexadecimal)
            //   // if(byteCounter <= 13) stringPackets += b.ToString("X2") + " ";
            //    byteCounter++;

            //    switch(byteCounter)
            //    {
            //        //case 6: stringPackets += Environment.NewLine;
            //        //    stringPackets += "Source MAC Address: ";
            //        //    break;
            //        //case 12: stringPackets += Environment.NewLine;
            //        //    stringPackets += "EtherType: ";
            //        //    break;
            //        case 14: if(data[12] == 8)
            //            {
            //                if (data[13] == 0) {
            //                    numIP4++;
            //                    isIP = true;
            //                }
            //                if (data[13] == 6) numARP++;
            //            } else if (data[12] == 134 && data[13] == 221)
            //            {
            //                numIP6++;
            //                isIP = true;
            //            }
            //            protocolRetrieved = true;
            //            break;
            //     }
            //    if (protocolRetrieved) break;
            //}

            switch(data[12])
            {
                case 8:
                    if(data[13] == 0)
                    {
                        numIP4++;
                        isIP = true;
                    } else if(data[13] == 6)
                    {
                        numARP++;
                    }
                    break;
                case 134: if(data[13] == 221)
                    {
                        numIP6++;
                        isIP = true;
                    }
                    break;
                default: numOther++;
                    break;
            }

            if (!isIP) return;
            numIPPackets++;
            //stringPackets += Environment.NewLine + Environment.NewLine;
            //byteCounter = 0;
            //stringPackets += "Raw Data" + Environment.NewLine;

            //int hlen = data[14] & 0x0F;
            int version = data[14] >> 4;
            byte protocol = 0;
            switch(version)
            {
                case 4: protocol = data[23];
                    break;
                case 6: protocol = data[20];
                    break;
            }

            //tcp = 6, udp = 17
            switch(protocol)
            {
                case 1: numICMP++;
                    break;
                case 2: numIGMP++;
                    break;
                case 6: numTCP++;
                    break;
                case 17: numUDP++;
                    break;
                default: numOtherIP++;
                    break;
            }
            //protocol = data[23];
            //Process each byte in our captured packet

            //foreach (byte b in data)
            //{
            //    byteCounter++; //byte 0 = byteCounter 1, 
            //    if(byteCounter >= 15 & byteCounter < 15+hlen)
            //    {

            //    }

            //}
            //stringPackets += Environment.NewLine;
            //stringPackets += Environment.NewLine;

        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (!running)
            {
                
                if (cmbDevices.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select a device.");
                    return;
                }
                running = true;
                btnStartStop.Text = "Stop";
                timer1.Enabled = true;
                try
                {
                    device.StartCapture();
                } catch(Exception exp)
                {
                    MessageBox.Show("Error");
                    btnStartStop.Text = "Start";
                    running = false;
                }

            }
            else
            {
                //Close device
                running = false;
                btnStartStop.Text = "Start";
                timer1.Enabled = false;
                device.StopCapture();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            txtNumPackets.Text = Convert.ToString(numPackets);
            txtIP.Text = Convert.ToString(numIPPackets);
            if(!hasPacketData)
            {
                hasPacketData = true;
                chartPackets.Series["seriesPackets"]["PieLabelStyle"] = "Outside";
                chartPackets.Series["seriesPackets"].Label = "#PERCENT{P2}";
                chartPackets.Series["seriesPackets"].Points.AddY(0);
                chartPackets.Series["seriesPackets"].Points.AddY(0);
                chartPackets.Series["seriesPackets"].Points.AddY(0);
                chartPackets.Series["seriesPackets"].Points.AddY(0);
                
                chartPackets.Series["seriesPackets"].Points[0].LegendText = "IPv4 (#VAL)";
                chartPackets.Series["seriesPackets"].Points[1].LegendText = "IPv6 (#VAL)";
                chartPackets.Series["seriesPackets"].Points[2].LegendText = "ARP (#VAL)";
                chartPackets.Series["seriesPackets"].Points[3].LegendText = "Other (#VAL)";
            }

            chartPackets.Series["seriesPackets"].Points[0].SetValueY(numIP4);
            chartPackets.Series["seriesPackets"].Points[1].SetValueY(numIP6);
            chartPackets.Series["seriesPackets"].Points[2].SetValueY(numARP);
            chartPackets.Series["seriesPackets"].Points[3].SetValueY(numOther);

            if(!hasIPData)
            {
                hasIPData = true;
                chartPackets.Series["seriesIP"]["PieLabelStyle"] = "Outside";
                chartPackets.Series["seriesIP"].Label = "#PERCENT{P2}";
                chartPackets.Series["seriesIP"].Points.AddY(0); //TCP
                chartPackets.Series["seriesIP"].Points.AddY(0); //UDP
                chartPackets.Series["seriesIP"].Points.AddY(0); //OTHER
                chartPackets.Series["seriesIP"].Points.AddY(0); //IGMP
                chartPackets.Series["seriesIP"].Points.AddY(0); //ICMP


                chartPackets.Series["seriesIP"].Points[0].LegendText = "TCP (#VAL)";
                chartPackets.Series["seriesIP"].Points[1].LegendText = "UDP (#VAL)";
                chartPackets.Series["seriesIP"].Points[2].LegendText = "Other (#VAL)";
                chartPackets.Series["seriesIP"].Points[3].LegendText = "IGMP (#VAL)";
                chartPackets.Series["seriesIP"].Points[4].LegendText = "ICMP (#VAL)";
            }

            chartPackets.Series["seriesIP"].Points[0].SetValueY(numTCP);
            chartPackets.Series["seriesIP"].Points[1].SetValueY(numUDP);
            chartPackets.Series["seriesIP"].Points[2].SetValueY(numOtherIP);
            chartPackets.Series["seriesIP"].Points[3].SetValueY(numIGMP);
            chartPackets.Series["seriesIP"].Points[4].SetValueY(numICMP);

            chartPackets.Refresh();
        }

        private void cmbDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            device = devices[cmbDevices.SelectedIndex];
            cmbDevices.Text = device.Description;
            txtGUID.Text = device.Name;
            //register handler function
            device.OnPacketArrival += new SharpPcap.PacketArrivalEventHandler(device_OnPacketArrival);

            //Open device for capturing
            int readTimeOutMilliseconds = 1000;
            device.Open(DeviceMode.Promiscuous, readTimeOutMilliseconds);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //saveFileDialog1.Filter = "Text Files|*.txt|All Files|*.*";
            //saveFileDialog1.Title = "Save the Captured Packets";
            //saveFileDialog1.ShowDialog();

            ////check to see if filename is given
            //if(saveFileDialog1.FileName!="")
            //{
            //    System.IO.File.WriteAllText(saveFileDialog1.FileName,txtCapturedData.Text);
            //}
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //openFileDialog1.Filter = "Text Files|*.txt|All Files|*.*";
            //openFileDialog1.Title = "Open the Captured Packets";
            //openFileDialog1.ShowDialog();

            ////check to see if filename is given
            //if (openFileDialog1.FileName != "")
            //{
            //    txtCapturedData.Text = System.IO.File.ReadAllText(openFileDialog1.FileName);
            //}
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtNumPackets.Text = "0";
            numPackets = 0;

            numARP = 0;
            numIP4 = 0;
            numIP6 = 0;
            numIPPackets = 0;
            numOther = 0;

            numOtherIP = 0;
            numUDP = 0;
            numTCP = 0;
            numICMP = 0;
            numIGMP = 0;
            chartPackets.Series["seriesPackets"].Points.Clear();
            hasPacketData = false;
            chartPackets.Series["seriesIP"].Points.Clear();
            hasIPData = false;

            //txtCapturedData.Text = "";
        }

        private void sendWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if(frmSend.instantiations == 0)
            //{
            //    fSend = new frmSend();
            //    fSend.Show();
            //}
            //else
            //{
            //    MessageBox.Show("Send window is already open.");
            //}
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

       
    }
}
