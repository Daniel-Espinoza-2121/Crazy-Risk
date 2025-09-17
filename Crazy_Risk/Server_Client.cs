using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;

namespace Crazy_Risk
{
    class Server
    {
        public string IP { get; private set; } = ObtenerIPLocal();
        public int Puerto { get; private set; }
        public TcpListener CanalServer { get; private set; }


        public Thread? HiloAceptarClientes { get; private set; }

        public LinkedListJugadores Clientes = new LinkedListJugadores();

        public Server(int puerto)
        {
            this.Puerto = puerto;
            this.CanalServer = new TcpListener(IPAddress.Parse(this.IP), this.Puerto);
            this.CanalServer.Start();
            this.HiloAceptarClientes = new Thread(this._AceptarClientes);
            this.HiloAceptarClientes.Start();
        }

        static string ObtenerIPLocal()
        {
            string host = Dns.GetHostName();         // Nombre del equipo
            IPHostEntry ipHost = Dns.GetHostEntry(host);

            foreach (IPAddress direccion in ipHost.AddressList)
            {
                if (direccion.AddressFamily == AddressFamily.InterNetwork) // Solo IPv4
                {
                    return direccion.ToString(); // Devuelve la primera IP válida
                }
            }
            return "127.0.0.1"; // Valor por defecto si no encuentra
        }
        private void _AceptarClientes()
        {
            while (true)
            {
                this.Clientes.Add(this.CanalServer.AcceptTcpClient());
                Console.WriteLine("Cliente conectado!");
            }
        }
    }
    class Cliente
    {
        public string? IP { get; private set; }
        public int Puerto { get; private set; }
        public TcpClient Canal;
        public Thread? HiloEscucharMensaje;

        public Cliente(string ipServer, int puerto)
        {
            this.IP = ipServer;
            this.Puerto = puerto;
            this.Canal = new TcpClient(this.IP, this.Puerto);
            this._IniciarLectura();
        }

        private void _ejecutarComando(Mensaje mensaje)
        {
            
        }

        private void _IniciarLectura()
        {
            this.HiloEscucharMensaje = new Thread(_leerMensaje);
            this.HiloEscucharMensaje.Start();
        }

        private void _leerMensaje()
        {
            try
            {
                NetworkStream stream = this.Canal.GetStream();
                byte[] buffer = new byte[1024];

                while (true)
                {
                    if (stream.DataAvailable)
                    {
                        int bytesLeidos = stream.Read(buffer, 0, buffer.Length);
                        if (bytesLeidos == 0) break; // Server desconectado
                        string jsonRecibido = Encoding.UTF8.GetString(buffer, 0, bytesLeidos);
                        Mensaje mensaje = JsonSerializer.Deserialize<Mensaje>(jsonRecibido)!;
                        this._ejecutarComando(mensaje);
                    }
                    Thread.Sleep(50);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en cliente: " + ex.Message);
            }
            finally
            {
                this.Canal.Close();
                Console.WriteLine("Canal cerrado");
            }
        }

        public void enviarmensaje(Mensaje mensaje)
        {
            string json = JsonSerializer.Serialize(mensaje);
            NetworkStream stream = this.Canal.GetStream();
            byte[] datos = Encoding.UTF8.GetBytes(json);
            stream.Write(datos, 0, datos.Length);
        }
    }

    class LinkedListJugadores
    {
        public Jugador? Head;
        public int Length { get; private set; }

        public LinkedListJugadores()
        {
            this.Head = null;
            this.Length = 0;
        }

        public void Add(TcpClient valor)
        {
            Jugador nuevo = new Jugador(valor);

            if (Head == null)
            {
                this.Head = nuevo;
                nuevo.Next = this.Head;
            }
            else
            {
                Jugador actual = Head;
                while (actual.Next != this.Head)
                {
                    actual = actual.Next!;
                }
                actual.Next = nuevo;
                nuevo.Next = this.Head;
            }
            Length++;
            nuevo.IniciarLectura();
        }

        public void Delete(Jugador valor)
        {
            if (Head == null) return;

            Jugador actual = Head;
            Jugador previo = null!;

            do
            {
                if (actual == valor)
                {
                    if (previo != null)
                        previo.Next = actual.Next;
                    else
                        Head = actual.Next;

                    Length--;
                    return;
                }

                previo = actual;
                actual = actual.Next!;
            }
            while (actual != Head);
        }
    }

    class Jugador
    {
        string? IdJugador;
        public Jugador? Next;
        public TcpClient Canal { get; private set; }

        public Thread? HiloLector { get; private set; }

        public Jugador(TcpClient canal)
        {
            Canal = canal;
            Next = null;
        }

        ~Jugador()
        {
            this.Canal.Close();
        }

        private void _ejecutarComando(Mensaje mensaje)
        {
            try
            {
                if ((mensaje != null) && mensaje.Comando != null)
                {
                    //ejemplo de uso
                    if (mensaje.Comando == "printInt")
                    {
                        Console.WriteLine(mensaje.Int1);
                    }
                    else if (mensaje.Comando == "setIdJugador")
                    {
                        this._setIdJugador(mensaje.Texto1!);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al ejecutar comando: {ex.Message}");
            }
        }

        public void enviarMensaje(Mensaje mensaje)
        {
            string json = JsonSerializer.Serialize(mensaje);
            NetworkStream stream = this.Canal.GetStream();
            byte[] datos = Encoding.UTF8.GetBytes(json);
            stream.Write(datos, 0, datos.Length);
        }

        public void IniciarLectura()
        {
            this.HiloLector = new Thread(_leerMensaje);
            this.HiloLector.Start();
        }

        private void _leerMensaje()
        {
            try
            {
                NetworkStream stream = this.Canal.GetStream();
                byte[] buffer = new byte[1024];

                while (true)
                {
                    if (stream.DataAvailable) // Solo leer si hay datos
                    {
                        int bytesLeidos = stream.Read(buffer, 0, buffer.Length);
                        if (bytesLeidos == 0) break; // Cliente desconectado
                        string jsonRecibido = Encoding.UTF8.GetString(buffer, 0, bytesLeidos);
                        Mensaje mensaje = JsonSerializer.Deserialize<Mensaje>(jsonRecibido)!;
                        this._ejecutarComando(mensaje);
                    }
                    Thread.Sleep(50); // Evita consumir 100% CPU
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en cliente: " + ex.Message);
            }
            finally
            {
                this.Canal.Close();
                Console.WriteLine("Canal cerrado");
            }
            
        }

        
        //esto se puede cambiar más adelante
        private void _setIdJugador(string name)
        {
            this.IdJugador = name;
        }
    }

    class Mensaje
    {
        public string? Comando;
        public string? Texto1;
        public string? Texto2;
        public int Int1 = 0;
        public int Int2 = 0;

        public Mensaje(string comando, string texto1, string Texto2)
        {
            this.Comando = comando;
            this.Texto1 = texto1;
            this.Texto2 = Texto2;
        }
        public Mensaje(string comando, string texto1, string Texto2, int Int1)
        {
            this.Comando = comando;
            this.Texto1 = texto1;
            this.Texto2 = Texto2;
            this.Int1 = Int1;
        }
        public Mensaje(string comando, string texto1, string texto2, int int1, int int2)
        {
            this.Comando = comando;
            this.Texto1 = texto1;
            this.Texto2 = texto2;
            this.Int1 = int1;
            this.Int2 = int2;
        }
    }
}