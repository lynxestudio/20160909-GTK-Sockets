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
using System.Threading;

namespace Samples.Networking.Server
{
    public partial class FrmMain : Form
    {
        BinaryWriter writer = null;
        TcpListener listener = null;
        Socket connection = null;
        Thread readThread = null;

        public FrmMain()
        {
            InitializeComponent();
            readThread = new Thread(RunServer);
            readThread.Start();
        }

        private void btnSendClick(object sender, EventArgs e)
        {
            try
            {
                writer.Write(ChatMessage(txtMsg.Text));
            }
            catch (Exception ex)
            {
                LogMessage("Ex " + ex.ToString());
            }
        }

        string ChatMessage(string msg)
        {
            return string.Format("Server ({0}): {1} {2}", DateTime.Now.ToShortTimeString(), msg, Environment.NewLine);
        }

        void LogMessage(string msg)
        {
            txtLog.Text += string.Format("{0} : {1}{2}", DateTime.Now.ToShortTimeString(), msg, Environment.NewLine);
        }

        void RunServer()
        {
            int counter = 1;
            try
            {
                //step 1: create endpoint
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 6000);
                //Step 2: create TcpListener
                listener = new System.Net.Sockets.TcpListener(ipEndPoint);
                //Step 3: waits for connection request
                listener.Start();
                //Step 4: establish connection upon client request
                while (true)
                {
                    LogMessage("Waiting for connection...");
                    //step 5: accept an incoming connection
                    connection = listener.AcceptSocket();
                    //step 6: create the network stream object associated with socket
                    //NOTE: Use the namespace to avoid conflicts with GTK.Socket
                    System.Net.Sockets.NetworkStream socketStream = new System.Net.Sockets.NetworkStream(connection);
                    //step7: create the objects for transferring data across stream
                    using (writer = new BinaryWriter(socketStream))
                    {
                        using (BinaryReader reader = new BinaryReader(socketStream))
                        {
                            LogMessage(TcpFlags.SYN + " : " + counter);
                            //inform client that connection was ACK
                            writer.Write(ChatMessage(TcpFlags.ACK));
                            string theReply = "";
                            //read string data sent from client until receive the FIN signal
                            do
                            {
                                try
                                {
                                    //read the string sent to the server
                                    theReply = reader.ReadString();
                                    //display the message except the FIN 
                                    if (!theReply.Equals(TcpFlags.FIN))
                                        txtChat.Text += theReply;
                                    else
                                    {
                                        LogMessage("Received " + TcpFlags.FIN);
                                        LogMessage("Send " + TcpFlags.FIN + " to Client");
                                        writer.Write(TcpFlags.FIN);
                                    }
                                }
                                catch (System.Exception)
                                {
                                    break;
                                }
                            } while (theReply != TcpFlags.FIN
                    && connection.Connected);
                            //Close connection
                            LogMessage("Close connection");
                        }
                    }
                    socketStream.Close();
                    connection.Close();
                    ++counter;
                }
            }
            catch (System.Exception ex)
            {
                LogMessage("Ex " + ex.ToString());
            }
        }
    }
}
