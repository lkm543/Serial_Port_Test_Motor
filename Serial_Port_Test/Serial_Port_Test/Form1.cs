using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Runtime.InteropServices;

namespace Serial_Port_Test
{
    public partial class Form1 : Form
    {
        delegate void AppendTextCallback(string text);
        SerialPort serialPort = new SerialPort();  
        public Form1()
        {
            InitializeComponent();
        }
        public class Packet
        {
            public byte PacketType { get; set; }
            public byte Ch_Status { get; set; }
            public byte DataLength { get; set; }
            public byte[] Data { get; set; }
            public byte CRC8 { get; set; }
        }
        private void AppendStatusText(string text)
        {
            try
            {
                // InvokeRequired required compares the thread ID of the
                // calling thread to the thread ID of the creating thread.
                // If these threads are different, it returns true.
                if (this.statusbox.InvokeRequired)
                {
                    AppendTextCallback d = new AppendTextCallback(AppendStatusText);
                    this.Invoke(d, new object[] { text });
                }
                else
                {
                    //this.statusbox.SelectionColor = text_color;
                    this.statusbox.AppendText(text);
                    this.statusbox.ScrollToCaret();
                }
            }
            catch { }
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            AppendStatusText(serialPort.ReadLine());
        }

        private byte[] crc8tab = new byte[256] 
		{
		    0x00, 0x07, 0x0E, 0x09, 0x1C, 0x1B, 0x12, 0x15,
			0x38, 0x3F, 0x36, 0x31, 0x24, 0x23, 0x2A, 0x2D,
			0x70, 0x77, 0x7E, 0x79, 0x6C, 0x6B, 0x62, 0x65,
			0x48, 0x4F, 0x46, 0x41, 0x54, 0x53, 0x5A, 0x5D,
			0xE0, 0xE7, 0xEE, 0xE9, 0xFC, 0xFB, 0xF2, 0xF5,
			0xD8, 0xDF, 0xD6, 0xD1, 0xC4, 0xC3, 0xCA, 0xCD,
			0x90, 0x97, 0x9E, 0x99, 0x8C, 0x8B, 0x82, 0x85,
			0xA8, 0xAF, 0xA6, 0xA1, 0xB4, 0xB3, 0xBA, 0xBD,
			0xC7, 0xC0, 0xC9, 0xCE, 0xDB, 0xDC, 0xD5, 0xD2,
			0xFF, 0xF8, 0xF1, 0xF6, 0xE3, 0xE4, 0xED, 0xEA,
			0xB7, 0xB0, 0xB9, 0xBE, 0xAB, 0xAC, 0xA5, 0xA2,
			0x8F, 0x88, 0x81, 0x86, 0x93, 0x94, 0x9D, 0x9A,
			0x27, 0x20, 0x29, 0x2E, 0x3B, 0x3C, 0x35, 0x32,
			0x1F, 0x18, 0x11, 0x16, 0x03, 0x04, 0x0D, 0x0A,
			0x57, 0x50, 0x59, 0x5E, 0x4B, 0x4C, 0x45, 0x42,
			0x6F, 0x68, 0x61, 0x66, 0x73, 0x74, 0x7D, 0x7A,
			0x89, 0x8E, 0x87, 0x80, 0x95, 0x92, 0x9B, 0x9C,
			0xB1, 0xB6, 0xBF, 0xB8, 0xAD, 0xAA, 0xA3, 0xA4,
			0xF9, 0xFE, 0xF7, 0xF0, 0xE5, 0xE2, 0xEB, 0xEC,
			0xC1, 0xC6, 0xCF, 0xC8, 0xDD, 0xDA, 0xD3, 0xD4,
			0x69, 0x6E, 0x67, 0x60, 0x75, 0x72, 0x7B, 0x7C,
			0x51, 0x56, 0x5F, 0x58, 0x4D, 0x4A, 0x43, 0x44,
			0x19, 0x1E, 0x17, 0x10, 0x05, 0x02, 0x0B, 0x0C,
			0x21, 0x26, 0x2F, 0x28, 0x3D, 0x3A, 0x33, 0x34,
			0x4E, 0x49, 0x40, 0x47, 0x52, 0x55, 0x5C, 0x5B,
			0x76, 0x71, 0x78, 0x7F, 0x6A, 0x6D, 0x64, 0x63,
			0x3E, 0x39, 0x30, 0x37, 0x22, 0x25, 0x2C, 0x2B,
			0x06, 0x01, 0x08, 0x0F, 0x1A, 0x1D, 0x14, 0x13,
			0xAE, 0xA9, 0xA0, 0xA7, 0xB2, 0xB5, 0xBC, 0xBB,
			0x96, 0x91, 0x98, 0x9F, 0x8A, 0x8D, 0x84, 0x83,
			0xDE, 0xD9, 0xD0, 0xD7, 0xC2, 0xC5, 0xCC, 0xCB,
			0xE6, 0xE1, 0xE8, 0xEF, 0xFA, 0xFD, 0xF4, 0xF3
		};

        private byte updcrc8(byte crc, byte data)
        {

            crc = crc8tab[crc ^ data]; //look-up CRC-8 table
            return crc;
        }


        private byte Check_CRC8(Packet packet)
        {
            byte CRC8_Temp = 0;
            byte Index;

            CRC8_Temp = updcrc8(CRC8_Temp, (byte)'E');
            CRC8_Temp = updcrc8(CRC8_Temp, (byte)'C');
            CRC8_Temp = updcrc8(CRC8_Temp, (byte)'S');

            CRC8_Temp = updcrc8(CRC8_Temp, packet.PacketType);
            CRC8_Temp = updcrc8(CRC8_Temp, packet.Ch_Status);
            CRC8_Temp = updcrc8(CRC8_Temp, packet.DataLength);

            for (Index = 0; Index < (packet.DataLength - 1); Index++)
            {
                CRC8_Temp = updcrc8(CRC8_Temp, packet.Data[Index]);
            }

            return CRC8_Temp;

        }		
        private void button1_Click(object sender, EventArgs e)
        {
            serialPort.PortName = "COM4";
            serialPort.BaudRate = 4900000;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.Handshake = Handshake.None;
            serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
            serialPort.Open();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Packet AHRSPacket = new Packet();
            //byte_conversion_array DConvert = new byte_conversion_array();
            AHRSPacket.Ch_Status = Convert.ToByte(textBox1.Text);
            AHRSPacket.PacketType = Convert.ToByte(textBox2.Text);
            AHRSPacket.DataLength = Convert.ToByte(textBox2.Text);

            AHRSPacket.Data = new byte[AHRSPacket.DataLength - 1];
            //AHRSPacket.Data[0] = PID[(int)PName.WHO_AM_I];

            AHRSPacket.CRC8 = Check_CRC8(AHRSPacket); // calculation CRC-8

            byte[] packet = new byte[AHRSPacket.DataLength + 6];

            // Build packet header
            packet[0] = (byte)'E';
            packet[1] = (byte)'C';
            packet[2] = (byte)'S';
            packet[3] = AHRSPacket.PacketType;
            packet[4] = AHRSPacket.Ch_Status; // channle/status
            packet[5] = AHRSPacket.DataLength;
            int i;
            // Fill data section
            for (i = 0; i < (AHRSPacket.DataLength - 1); i++)
            {
                packet[6 + i] = AHRSPacket.Data[i];
            }

            // Add CRC-8 to end of packet
            packet[6 + i] = AHRSPacket.CRC8;

            serialPort.Write(packet, 0, AHRSPacket.DataLength + 6);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            serialPort.Close();
        }
    }

}
