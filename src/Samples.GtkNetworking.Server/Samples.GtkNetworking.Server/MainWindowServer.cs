using System;
using Gtk;
using System.IO;
using System.Threading;
using System.Net;

namespace Samples.GtkNetworking.Server
{
    public class MainWindowServer : Gtk.Window
    {
        VBox mainLayout = new VBox();
        HBox controlLayout = new HBox(false,2);
        Entry txtMsg = new Entry();
        Button btnSend = new Button(Stock.Ok);
        TextView txtChat = new TextView();
        TextView txtLog = new TextView();
        Label msgLabel = new Label("Message: ");
        System.Net.Sockets.Socket connection = null;
        BinaryWriter writer = null;
        System.Net.Sockets.TcpListener listener = null;
        Thread readThread = null;

        public MainWindowServer() : base(WindowType.Toplevel)
        {
            this.Title = "GTK# Network Server";
            this.SetDefaultSize(343, 288);
			this.DeleteEvent += new DeleteEventHandler(OnWindowDelete);
			this.btnSend.Clicked += new EventHandler(SendMessage);
			mainLayout.BorderWidth = 8;
			controlLayout.BorderWidth = 8;
            controlLayout.PackStart(msgLabel,false,true,0);
            controlLayout.PackStart(txtMsg,true,true,0);
            controlLayout.PackStart(btnSend,false,false,0);
            mainLayout.PackStart(controlLayout,false,true,0);
            mainLayout.PackStart(txtChat,true,true,0);
            mainLayout.PackStart(new Label("Log "),false,true,0);
            mainLayout.PackStart(txtLog,true,true,0);
            this.Add(mainLayout);
            this.ShowAll();
            readThread = new Thread(RunServer);
            readThread.Start();
        }

        protected void OnWindowDelete(object o, DeleteEventArgs args)
		{
            //Terminate all
            System.Environment.Exit(System.Environment.ExitCode);
		}

        protected void SendMessage(object o,EventArgs args)
        {
            try
            {
                writer.Write(ChatMessage(txtMsg.Text));
                txtChat.Buffer.Text += ChatMessage(txtMsg.Text);
                txtMsg.Text = string.Empty;
            }
            catch (System.Net.Sockets.SocketException error)
            {
                LogMessage("Error: " + error.Message);
            }
        }

        string ChatMessage(string msg)
        {
            return string.Format("Server ({0}): {1} {2}",
            DateTime.Now.ToShortTimeString(),
            msg,
            Environment.NewLine);
        }

        void LogMessage(string msg)
        {
            txtLog.Buffer.Text += string.Format("{0} : {1}{2}",
            DateTime.Now.ToShortTimeString(),
            msg,
            Environment.NewLine);
        }

        void RunServer()
        {
            int counter = 1;
            try
            {
                //step 1: create endpoint
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any,6000);
                //Step 2: create TcpListener
                listener = new System.Net.Sockets.TcpListener(ipEndPoint);
                //Step 3: waits for connection request
                listener.Start();
                //Step 4: establish connection upon client request
                while(true)
                {
                    LogMessage("Waiting for connection...");
                    //step 5: accept an incoming connection
                    connection = listener.AcceptSocket();
                    //step 6: create the network stream object associated with socket
                    //NOTE: Use the namespace to avoid conflicts with GTK.Socket
                    System.Net.Sockets.NetworkStream socketStream = new System.Net.Sockets.NetworkStream(connection);
                    //step7: create the objects for transferring data across stream
                    using(writer = new BinaryWriter(socketStream))
                    {
                        using(BinaryReader reader = new BinaryReader(socketStream))
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
                                if(!theReply.Equals(TcpFlags.FIN))                                
                                    txtChat.Buffer.Text += theReply;
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