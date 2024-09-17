using System;
using Gtk;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace Samples.GtkSocketClient
{
	public class MainWindow : Gtk.Window
	{
		VBox mainLayout = new VBox();
		HBox controlLayout = new HBox(false, 2);
		HBox connectedLayout = new HBox(false,2);
		Entry txtMsg = new Entry();
		Button btnSend = new Button(Stock.Ok);
		Button btnDisconnect = new Button(Stock.Disconnect);
		TextView txtChat = new TextView();
		TextView txtLog = new TextView();
		Label msgLabel = new Label("Message: ");
		TcpClient client = new TcpClient();
		NetworkStream output = null;
		BinaryWriter writer = null;
		BinaryReader reader = null;
		string message = "";
		Thread readThread = null;


		public MainWindow() : base(WindowType.Toplevel)
		{
			this.Title = "GTK# Network client";
			this.SetDefaultSize(343, 288);
			this.DeleteEvent += new DeleteEventHandler(OnWindowDelete);
			this.btnSend.Clicked += new EventHandler(SendMessage);
			this.btnDisconnect.Clicked += new EventHandler(SendDisconnect);
			mainLayout.BorderWidth = 8;
			connectedLayout.BorderWidth = 8;
			connectedLayout.PackStart(btnDisconnect,false,false,0);

			controlLayout.BorderWidth = 8;
			controlLayout.PackStart(msgLabel,false,true,0);
			controlLayout.PackStart(txtMsg,true,true,0);
			controlLayout.PackStart(btnSend,false,false,0);
			mainLayout.PackStart(connectedLayout,false,true,0);
			mainLayout.PackStart(txtChat,true,true,0);
			mainLayout.PackStart(controlLayout,false,true,0);
			mainLayout.PackStart(new Label("Log"),false,true,0);
			mainLayout.PackStart(txtLog,true,true,0);
			this.Add(mainLayout);
			this.ShowAll();
			readThread = new Thread(new ThreadStart(RunClient));
			readThread.Start();
		}

		protected void OnWindowDelete(object o, DeleteEventArgs args)
		{
			System.Environment.Exit(System.Environment.ExitCode);
		}

		protected void SendDisconnect(object o,EventArgs args)
		{
			if(client.Connected)
			{
				try
				{
				//Send disconnected signal
				writer.Write(TcpFlags.FIN);
				}
				catch(SocketException error)
				{
				LogMessage("Error " + error.Message);
				}
			}
			else
				LogMessage("You are not connected");
		}

		protected void SendMessage(object o,EventArgs args)
		{
			try
			{
				writer.Write(ChatMessage(txtMsg.Text));
			    txtChat.Buffer.Text += ChatMessage(txtMsg.Text);
				txtMsg.Text = string.Empty;
			}
			catch (SocketException error)
			{
				LogMessage("Error " + error.Message);
			}
		}

		String ChatMessage(string msg)
		{
			return string.Format("Client ({0}): {1} {2}",
			DateTime.Now.ToLongTimeString(),
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

		protected void RunClient()
		{
			try
			{
				//step 1 create a local endpoint
				IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Loopback,6000);
				//step 2 create a socket client and connect
				client.Connect(localEndPoint);
				LogMessage(TcpFlags.SYN);
				//step 3 get the network stream associated with tcpclient
				output = client.GetStream();
				//step 4 create the objects for writing and reading across the stream
				using(writer = new BinaryWriter(output))
				{
				using(reader = new BinaryReader(output))
				{
				//Receive message until receive the FIN signal
				do
				{
					try
					{
						message = reader.ReadString();
						if(!message.Equals(TcpFlags.FIN))
							txtChat.Buffer.Text += message;
						else
							LogMessage("Received " + TcpFlags.FIN + " from server");
					}
					catch (System.Exception error)
					{
						LogMessage("Error: " + error.Message);
					}
				} while (message != TcpFlags.FIN);
				LogMessage("Closing connection...");
				}
				}
				output.Close();
				client.Close();	
			}
			catch (System.Exception ex)
			{
				LogMessage(" Ex " + ex.Message);
			}
		}
	}
}

