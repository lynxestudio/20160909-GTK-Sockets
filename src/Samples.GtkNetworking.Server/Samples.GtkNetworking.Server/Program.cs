using System;
using Gtk;

namespace Samples.GtkNetworking.Server
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Application.Init();
			MainWindowServer win = new MainWindowServer();
			win.Show();
			Application.Run();
		}
	}
}