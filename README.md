# Entendiendo la programación de Sockets con GTK# y .NET

¿Qué son los sockets?
Los Sockets o mejor dicho los Berkeley Sockets (BSD IPC) son la combinación exclusiva de una dirección IP y un numero de puerto TCP que habilita a los servicios (procesos en ejecución) de una computadora el poder intercambiar datos a través de la red. Los servicios de red utilizan los sockets para comunicarse entre los equipos remotos. Por ejemplo el siguiente comando:

        $ telnet 192.168.1.14 80
      
Este comando solicita una conexión desde un puerto aleatorio en el cliente (por ejemplo: 5643) al puerto 80 en el servidor (que es el puerto asignado para el servicio HTTP). Con la siguiente figura (fig 1) se ilustra a detalle este esquema denominado cliente-servidor.

Fig 1 Una comunicación utilizando números de puerto para transmitir datos.



Históricamente los sockets son una API (Application Programming Interface) de programación estándar utilizada para construir aplicaciones de red que vienen desde el sistema UNIX BSD. Esta interface de programación para la capa 4 del modelo OSI (OSI Model Layer 4) permite a un programador tratar una conexión de red como un flujo de bytes que puede escribirse o leerse sin demasiada complejidad. Con un socket se pueden realizar siete operaciones básicas:

Conectarse a una máquina remota.
Enviar datos.
Recibir datos.
Cerrar una conexión.
Escuchar para los datos entrantes.
Aceptar conexiones desde maquinas remotas en el puerto enlazado.
Enlazarse a un puerto.
Los Sockets pueden ser orientados a la conexión (Stream Socket) o no (Message-based Socket).

Los Stream Sockets son ideales para transmitir grandes volúmenes de información de manera confiable. Una conexión Stream Socket se establece mediante el mecanismo three-hand shake de TCP, los datos se transmiten y cada paquete se revisa para asegurarse de la exactitud en la transmisión.

Los Datagram Sockets son apropiados para transferencias de datos cortas, rápidas y sin necesidad de un chequeo de errores. Los desarrolladores de aplicaciones los prefieren por ser rápidos y muy fáciles de programar.

Fig 2 Tipos de Socket



El Framework .NET posee las clases de alto y bajo nivel que encapsulan la funcionalidad de un Socket (tanto TCP como UDP) para construir aplicaciones de red con relativa facilidad y sin preocuparse por todo el intricado mecanismo de comunicación que necesitaría muchas líneas de código. La siguiente lista describe las clases principales:

NetworkStream: Una clase derivada de la clase Stream representa el flujo de datos de entrada o de salida desde la red.
TcpClient: Crea conexiones TCP de red para conectarse a un socket de servidor.
TcpListener: Se utiliza para escuchar peticiones de red TCP.
UdpClient: Crea conexiones UDP de red con posibilidad de multicasting.
Socket: Es una clase de bajo nivel que envuelve a la implementación winsock, las clases TcpClient, TcpListener y UDPClient utilizan esta clase para sus operaciones, se puede afirmar que la clase Socket tiene las operaciones de estas clases más otras funcionalidades mucho más avanzadas y de más bajo nivel.
Pasos para la construcción de un Servidor TCP GTK#
Un servidor TCP siempre está ejecutándose de forma continua hasta que recibe una solicitud de conexión por parte de un cliente, cuando se recibe esta solicitud, el servidor establece una conexión con el cliente y utiliza dicha conexión para el intercambio de datos. Si los programas se comunican a través de TCP los datos que se procesan se envían y se reciben como flujo de bytes.

Para demostrar estos conceptos escribí dos programas: el de un servidor y el de un cliente TCP. Ambos utilizan una interfaz de usuario (GUI) en GTK# para comunicarse entre ellos mediante mensajes de texto.

Fig 3 Ejemplo de un servidor TCP con una GUI GTK#.



Fig 4 Ejemplo de un cliente TCP con una GUI GTK#.



Pasos para la construcción de un Servidor TCP GTK#
El proyecto del servidor TCP GTK# se compone de 2 clases:

La clase MainWindowServer.cs es la clase que construye la GUI del programa, maneja los eventos para enviar los mensajes al cliente y las excepciones o mensajes que el programa notifique.
La clase Program.cs es la clase principal que donde se ejecuta el servidor.
Para construir el servidor TCP se requieren de los siguientes pasos:
1) Crear un objeto IPEndpoint que asocia una dirección IP y un número de puerto.

IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any,6000);
2) Crear un objeto TcpListener que reciba como argumento un objeto IPEndpoint. (Aquí el objeto TcpListener oculta la intricada programación de un Server Socket para una facilidad en la programación)

listener = new System.Net.Sockets.TcpListener(ipEndPoint); 
3) Iniciar el objeto TcpListener para que escuche las peticiones.

listener.Start();
4) Se utilizar un ciclo para que el Server Socket escuche o espere indefinidamente hasta recibir una petición, cuando el servidor recibe la petición crea una conexión hacia el cliente y regresa un objeto Socket del ensamblado System.Net.Sockets.Socket.

connection = listener.AcceptSocket();
5) Se obtiene el flujo de comunicación del Socket.

System.Net.Sockets.NetworkStream socketStream = 
new System.Net.Sockets.NetworkStream(connection);
6) Finalmente se asocia el flujo de comunicación del Server Socket con un escritor y un lector binario para transferir y recibir datos a través del flujo de comunicación.

using(writer = new BinaryWriter(socketStream))
{
using(BinaryReader reader = new BinaryReader(socketStream))
{
//the stream goes here 
}
}
Es muy importante que una vez que se finaliza la comunicación con el cliente cerrar el flujo y la conexión mediante con el método Close de cada uno de los objetos.

socketStream.Close();
connection.Close(); 
Pasos para la construcción de un cliente TCP GTK#
El proyecto del cliente TCP GTK# se compone de 2 clases:

La clase MainWindow.cs es la clase que construye la GUI del cliente, maneja los eventos para recibir y enviar los mensajes al servidor, y mostrar las excepciones o mensajes que ocurran.
La clase Program.cs es la clase principal del cliente
Para construir el cliente TCP se requieren de los siguientes pasos, algunos son idénticos a los que se escribieron para el servidor:

1) Crear un objeto IPEndpoint que asocia la dirección IP y el número de puerto del servidor, generalmente estos datos son fijos ya que los servidores se configuran para tenerlos de manera estática. (en este ejemplo utilice una sola máquina como servidor y como cliente)

  IPEndPoint localEndPoint = 
  new IPEndPoint(IPAddress.Loopback,6000);
  
2) Se crea un Socket de cliente y se conecta al puerto del servidor.

  client.Connect(localEndPoint);
  
3) Se obtiene el flujo de comunicación del Socket

  output = client.GetStream();
  
4) Se crean los objetos lector y escritor para trabajar con el flujo de comunicación.

   using(writer = new BinaryWriter(output))
   {
    using(reader = new BinaryReader(output))
    {
//the stream goes here 
    }
  }
  
Finalmente cuando se termina la comunicación con el servidor, se cierra el flujo de datos y la conexión con el método Close de cada objeto.

    output.Close();
    client.Close();
  
La clase TcpFlags
Ambos proyectos utilizan la clase TcpFlags, la cual pretende ilustrar básicamente como son las banderas TCP (aquí más detalles de las TCP Flags) que utiliza la capa de transporte (layer 4) para manejar la comunicación entre dos máquinas.

A continuación unas imágenes del cliente y servidor comunicándose entre si.
Fig 5 Enviando un mensaje desde el cliente al servidor.



Fig 6 Recibiendo el mensaje del cliente.



Fig 7 Enviándole un mensaje al cliente desde el servidor.



Fig 8 Desconectándose del servidor.

