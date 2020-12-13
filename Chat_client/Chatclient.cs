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
enum Command
        {               //列舉1~5
            Login,      //Log into the server
            Logout,     //Logout of the server
            Message,    //Send a text message to all the chat clients
            List,       //Get a list of users in the chat room from the server
            Null        //No command
        }
    public partial class Chatclient : Form

    {
        public Socket clientSocket;
        public string strName;
        public EndPoint epServer;

     

        public Chatclient()
        {
            InitializeComponent();
            Console.WriteLine("Potato");
            MessageBox.Show("welcome~", "Client", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Chatclient_Load(object sender, EventArgs e)
        {
            string ipAddress = GetMyIP();
            string macCard = GetMyMACAddress();
            txtIPAddress.Text = ipAddress;  //print IP to textBox
            txtMAC.Text = macCard;          //print MAC to textBox

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

        private void btnLogin_Click(object sender, EventArgs e)
        {
            strName = txtName.Text;
            try
            {
                //Using UDP sockets
                clientSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Dgram, ProtocolType.Udp);

                //IP address of the server machine
                IPAddress ipAddress = IPAddress.Parse(txtServerIP.Text);
                //Server is listening on port 1000
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1000);

                epServer = (EndPoint)ipEndPoint;

                Data msgToSend = new Data();
                msgToSend.cmdCommand = Command.Login;
                msgToSend.strName = strName;
                msgToSend.strMessage = null;

                byte[] byteData = msgToSend.ToByte();

                //Login to the server
                clientSocket.BeginSendTo(byteData, 0, byteData.Length,          //處理多個資訊
                    SocketFlags.None, epServer, new AsyncCallback(OnSend), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SGSclient",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            ClientChat_from clientChat_From = new ClientChat_from();
            clientChat_From.Show();
            this.Hide();    //藏起來


        }

        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                
                MessageBox.Show(ex.Message, "SGSclient: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        class Data
        {
            //Default constructor
            public Data()
            {
              
                this.cmdCommand = Command.Null;
                this.strMessage = null;
                this.strName = null;
            }

            //Converts the bytes into an object of type Data
            public Data(byte[] data)
            {
                //The first four bytes are for the Command
                this.cmdCommand = (Command)BitConverter.ToInt32(data, 0);

                //The next four store the length of the name
                int nameLen = BitConverter.ToInt32(data, 4);

                //The next four store the length of the message
                int msgLen = BitConverter.ToInt32(data, 8);

                //This check makes sure that strName has been passed in the array of bytes
                if (nameLen > 0)
                    this.strName = Encoding.UTF8.GetString(data, 12, nameLen);
                else
                    this.strName = null;

                //This checks for a null message field
                if (msgLen > 0)
                    this.strMessage = Encoding.UTF8.GetString(data, 12 + nameLen, msgLen);
                else
                    this.strMessage = null;
            }

            //Converts the Data structure into an array of bytes
            public byte[] ToByte()
            {
                List<byte> result = new List<byte>();

                //First four are for the Command
                result.AddRange(BitConverter.GetBytes((int)cmdCommand));

                //Add the length of the name
                if (strName != null)
                    //result.AddRange(BitConverter.GetBytes(strName.Length));//Encoding.UTF8.GetBytes(strName).Length
                    result.AddRange(BitConverter.GetBytes(Encoding.UTF8.GetBytes(strName).Length));
                else
                    result.AddRange(BitConverter.GetBytes(0));

                //Length of the message
                if (strMessage != null)
                    //result.AddRange(BitConverter.GetBytes(strMessage.Length));//Encoding.UTF8.GetBytes(strMessage).Length
                    result.AddRange(BitConverter.GetBytes(Encoding.UTF8.GetBytes(strMessage).Length));
                else
                    result.AddRange(BitConverter.GetBytes(0));

                //Add the name
                if (strName != null)
                    result.AddRange(Encoding.UTF8.GetBytes(strName));

                //And, lastly we add the message text to our array of bytes
                if (strMessage != null)
                    result.AddRange(Encoding.UTF8.GetBytes(strMessage));

                return result.ToArray();
            }

            public string strName;      //Name by which the client logs into the room
            public string strMessage;   //Message text
            public Command cmdCommand;  //Command type (login, logout, send message, etcetera)
        }
    }
}
