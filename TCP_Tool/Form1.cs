using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Globalization;
using System.Threading;

namespace TCP_Tool
{
    public partial class TCPTestTool : Form
    {        
        static AsyncTcpClient client;
        //声明代理
        private delegate void UIProcessFunction(object param);

        public TCPTestTool()
        {
            InitializeComponent();

            //初始化控件
            tbIPAddress.Text = "2001::1:217:88ff:fe0b:d2";
            tbPort.Text = "50000";
            rbTCPClient.Checked = true;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {            
            if (tbIPAddress.Text==""||tbPort.Text=="") 
            {
                MessageBox.Show("Please input both IP Address and Port number.");
                toolStripStatusLabelStatus.Text = "Please input both IP Address and Port number.";
                return;
            }

            if(client==null||client.Connected==false)
            {
                toolStripStatusLabelStatus.Text = "Connecting";
                Application.DoEvents();

                //convert text to IP address, and port
                IPAddress ipAddr = IPAddress.Parse(tbIPAddress.Text);
                int tcpPort = int.Parse(tbPort.Text);

                if (rbTCPServer.Checked == true) tcpServerTest(ipAddr, tcpPort);
                else if (rbTCPClient.Checked == true) tcpClientTest(ipAddr, tcpPort);
            }
            else if (client != null && client.Connected)
            {
                client.Close();
                //toolStripStatusLabelStatus.Text = "Disconnected";
                //btnConnect.Text = "Connect";
            }
        }

        private void tcpClientTest(IPAddress ipAddr, int tcpPort)
        {
            IPEndPoint remoteEP = new IPEndPoint(ipAddr,tcpPort);
            //IPEndPoint localEP =null;
            client = new AsyncTcpClient(remoteEP);
            client.Encoding = Encoding.UTF8;
            client.ServerExceptionOccurred +=
              new EventHandler<TcpServerExceptionOccurredEventArgs>(client_ServerExceptionOccurred);
            client.ServerConnected +=
              new EventHandler<TcpServerConnectedEventArgs>(client_ServerConnected);
            client.ServerDisconnected +=
              new EventHandler<TcpServerDisconnectedEventArgs>(client_ServerDisconnected);
            //client.PlaintextReceived +=
            //  new EventHandler<TcpDatagramReceivedEventArgs<string>>(client_PlaintextReceived);
            //使用原始Data
            client.DatagramReceived +=
                new EventHandler<TcpDatagramReceivedEventArgs<byte[]>>(client_DatagramReceived);
            client.Connect();
            
            //btnConnect.Text = "Disconnect";
        }

        private void tcpServerTest(IPAddress ipAddr,int tcpPort)
        {
            
            
 
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            try
            {
                string text = tbMessageToSend.Text;
                client.Send(text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }               

        static void client_ServerExceptionOccurred(object sender, TcpServerExceptionOccurredEventArgs e)
        {
            //Logger.Debug(string.Format(CultureInfo.InvariantCulture,"TCP server {0} exception occurred, {1}.", e.ToString(), e.Exception.Message));
            MessageBox.Show(string.Format(CultureInfo.InvariantCulture,"TCP server {0} exception occurred, {1}.", e.ToString(), e.Exception.Message));
        }

        private void client_ServerConnected(object sender, TcpServerConnectedEventArgs e)
        {
            //Logger.Debug(string.Format(CultureInfo.InvariantCulture,"TCP server {0} has connected.", e.ToString()));
            string[] serverStatus = { e.ToString(), "1" };
            this.Invoke(new UIProcessFunction(UpdateUIOnServerStatusChanged), new object[] { serverStatus});
        }
    
        private void client_ServerDisconnected(object sender, TcpServerDisconnectedEventArgs e)
        {
            //Logger.Debug(string.Format(CultureInfo.InvariantCulture,"TCP server {0} has disconnected.", e.ToString()));
            string[] serverStatus = { e.ToString(), "0" };
            this.Invoke(new UIProcessFunction(UpdateUIOnServerStatusChanged),new object[]{serverStatus});
        }

        static void client_PlaintextReceived(object sender, TcpDatagramReceivedEventArgs<string> e)
        {
            //Console.Write(string.Format("Server : {0} --> ",e.TcpClient.Client.RemoteEndPoint.ToString()));
            //Console.WriteLine(string.Format("{0}", e.Datagram));
            MessageBox.Show(string.Format("Server : {0} --> ", e.TcpClient.Client.RemoteEndPoint.ToString()) + "\n" + e.Datagram);
            //tbMessageReceived.Text += string.Format("Server : {0} --> ", e.TcpClient.Client.RemoteEndPoint.ToString()) + "\n" + string.Format("{0}", e.Datagram+"\n");
        }

        private void client_DatagramReceived(object sender, TcpDatagramReceivedEventArgs<byte[]> e)
        {           
            //MessageBox.Show(string.Format("Server : {0} --> ", e.TcpClient.Client.RemoteEndPoint.ToString()) + "\n" + byteToHexStr(e.Datagram));
            string[] receivedDatagram = { e.TcpClient.Client.RemoteEndPoint.ToString(), byteToHexStr(e.Datagram) ,e.Datagram.Length.ToString()};
            this.Invoke(new UIProcessFunction(UpdateReceivedMessage), new object[] { receivedDatagram });
        }

        private void UpdateReceivedMessage(object receivedDatagram)
        {
            string serverInfo = ((string[])receivedDatagram)[0];
            string datagram = ((string[])receivedDatagram)[1];
            string datagramLength = ((string[])receivedDatagram)[2];
            this.tbMessageReceived.Text += datagram+Environment.NewLine;
            this.toolStripStatusLabelStatus.Text = string.Format("Server : {0} --> ", serverInfo) + datagramLength+ " bytes.";
        }

        private void UpdateUIOnServerStatusChanged(object server)
        {
            string serverInfo=((string[])server)[0];
            string serverStatus=((string[])server)[1]=="1"?"connected":"disconnected";            
            //update status bar            
            this.toolStripStatusLabelStatus.Text = string.Format(CultureInfo.InvariantCulture, "TCP server {0} has {1}.", serverInfo,serverStatus);
            this.btnConnect.Text=((string[])server)[1]=="1"?"Disconnect":"Connect";
        }
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// a space is insert after each byte
        /// <returns></returns>
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2")+" ";                    
                }
            }
            return returnStr;
        }
    }
}
