using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chat_client
{
    public partial class Chatclient : Form
    {
        public Chatclient()
        {
            InitializeComponent();
            Console.WriteLine("Potato");
        }

        private void Chatclient_Load(object sender, EventArgs e)
        {
            string ipAddress = GetMyIP();
            string macCard = GetMyMACAddress();
            Console.WriteLine("IPv4: " + ipAddress + "\n" + "Maccard: " + macCard);
            
        }

        private string GetMyMACAddress()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            String strMacAddress = string.Empty;
            foreach (NetworkInterface adapter in nics)
            {
                if (strMacAddress == String.Empty)// only return MAC Address from first card  
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    strMacAddress = adapter.GetPhysicalAddress().ToString();    //實體卡號
                }
            }
            return strMacAddress;
        }


        private string GetMyIP()
        {
            
            string hostname = Dns.GetHostName();

            IPAddress[] ip = Dns.GetHostEntry(hostname).AddressList;  //grab the first IP addresses
            foreach (IPAddress it in ip)
            {
                if (it.AddressFamily == AddressFamily.InterNetwork)
                {
                    return it.ToString();  //return string for IPv4 
                }
            }
            return "";  //return nothing
        }



    }
}
