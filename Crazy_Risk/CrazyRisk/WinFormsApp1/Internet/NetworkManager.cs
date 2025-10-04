using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;


    

    namespace CrazyRisk.Network
    {
        // Tipos de mensajes
        public enum TipoMensaje
        {
            ConexionJugador,
            IniciarJuego,
            DistribuirTerritorios,
            ColocarTropaInicial,
            AsignarTropas,
            Ataque,
            Movimiento,
            TerminarFase,
            IntercambiarTarjetas,
            ActualizarEstado,
            Desconexion
        }

        // Estructura del mensaje
        public class MensajeRed
        {
            public TipoMensaje Tipo { get; set; }
            public string Datos { get; set; }
            public string JugadorId { get; set; }
            public DateTime Timestamp { get; set; }
            


        public MensajeRed()
            {
                Timestamp = DateTime.Now;
            }
        }

    public class DatosTerminarFase
    {
        public string FaseActual { get; set; }
        public int TurnoActual { get; set; }
    }

    // Datos específicos para cada acción
    public class DatosConexion
        {
            public string NombreJugador { get; set; }
            public int ColorJugador { get; set; }
        }

        public class DatosAccionTerritorio
        {
            public string NombreTerritorio { get; set; }
            public int Cantidad { get; set; }
        }

        public class DatosAtaque
        {
            public string TerritorioOrigen { get; set; }
            public string TerritorioDestino { get; set; }
            public int TropasAtacante { get; set; }
            public int TropasDefensor { get; set; }
        public int[] DadosAtacante { get; set; }
        public int[] DadosDefensor { get; set; }
    }

        public class DatosMovimiento
        {
            public string TerritorioOrigen { get; set; }
            public string TerritorioDestino { get; set; }
            public int Cantidad { get; set; }
        }

        public class DatosEstadoJuego
        {
            public string EstadoActual { get; set; }
            public int TurnoActual { get; set; }
            public string MapaSerializado { get; set; }
        }

    // Clase para gestionar cada cliente conectado
    public class ClienteInfo
    {
        public TcpClient Cliente { get; set; }
        public NetworkStream Stream { get; set; }
        public string Nombre { get; set; }
        public int ColorJugador { get; set; }
        public Thread HiloEscucha { get; set; }
        public bool Activo { get; set; }
    }

    // Gestor de red principal
    public class NetworkManager
        {
        private TcpListener servidor;
        private TcpClient clienteLocal; // Para cuando somos cliente
        private NetworkStream streamLocal; // Para cuando somos cliente

        // Para servidor: múltiples clientes
        private Dictionary<int, ClienteInfo> clientesConectados;
        private int siguienteIdCliente = 0;
        private object lockClientes = new object();

        private Thread hiloEscucha;
        private bool ejecutando;

        public bool EsServidor { get; private set; }
        public bool Conectado { get; private set; }
        public int ClientesEsperados { get; set; } = 2; // Por defecto 2 clientes para 3 jugadores

        public event Action<MensajeRed> MensajeRecibido;
        public event Action<string> ErrorConexion;
        public event Action ClienteConectado;
        public event Action TodosClientesConectados;

        public NetworkManager()
        {
            clientesConectados = new Dictionary<int, ClienteInfo>();
        }

        
        // SERVIDOR: Iniciar servidor y esperar múltiples clientes
        public void IniciarServidor(int puerto = 5555, int numClientesEsperados = 2)
        {
            try
            {
                ClientesEsperados = numClientesEsperados;
                servidor = new TcpListener(IPAddress.Any, puerto);
                servidor.Start();
                EsServidor = true;
                Conectado = true;
                ejecutando = true;

                hiloEscucha = new Thread(EsperarClientes);
                hiloEscucha.IsBackground = true;
                hiloEscucha.Start();

                Console.WriteLine($"Servidor iniciado - Esperando {ClientesEsperados} clientes...");
            }
            catch (SocketException sex)
            {
                if (sex.SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    throw new Exception($"El puerto {puerto} ya está en uso");
                }
                throw new Exception($"Error al iniciar servidor: {sex.Message}");
            }
        }

        // SERVIDOR: Esperar conexiones de múltiples clientes
        private void EsperarClientes()
        {
            try
            {
                while (ejecutando && clientesConectados.Count < ClientesEsperados)
                {
                    TcpClient nuevoCliente = servidor.AcceptTcpClient();

                    lock (lockClientes)
                    {
                        int idCliente = siguienteIdCliente++;
                        var clienteInfo = new ClienteInfo
                        {
                            Cliente = nuevoCliente,
                            Stream = nuevoCliente.GetStream(),
                            Activo = true,
                            ColorJugador = idCliente + 1 // 1=Azul, 2=Verde
                        };

                        clientesConectados[idCliente] = clienteInfo;

                        // Iniciar hilo de escucha para este cliente
                        clienteInfo.HiloEscucha = new Thread(() => EscucharCliente(idCliente));
                        clienteInfo.HiloEscucha.IsBackground = true;
                        clienteInfo.HiloEscucha.Start();

                        Console.WriteLine($"Cliente {idCliente} conectado ({clientesConectados.Count}/{ClientesEsperados})");
                        ClienteConectado?.Invoke();

                        // Si todos los clientes están conectados
                        if (clientesConectados.Count == ClientesEsperados)
                        {
                            Console.WriteLine("Todos los clientes conectados!");
                          

                            // ESPERAR UN POCO PARA QUE LOS HILOS SE ESTABLEZCAN
                            Thread.Sleep(500);

                            // DISPARAR EVENTO PARA EL SERVIDOR
                            TodosClientesConectados?.Invoke();

                            // ENVIAR MENSAJE A TODOS LOS CLIENTES
                            var mensajeListo = new MensajeRed
                            {
                                Tipo = TipoMensaje.ConexionJugador, // Reutilizar este tipo
                                Datos = Newtonsoft.Json.JsonConvert.SerializeObject(new { TodosConectados = true }),
                                JugadorId = "SERVIDOR"
                            };

                            EnviarATodos(mensajeListo);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorConexion?.Invoke($"Error esperando clientes: {ex.Message}");
            }
        }

        // SERVIDOR: Escuchar mensajes de un cliente específico
        private void EscucharCliente(int idCliente)
        {
            
            ClienteInfo clienteInfo;

            lock (lockClientes)
            {
                if (!clientesConectados.ContainsKey(idCliente))
                    return;
                clienteInfo = clientesConectados[idCliente];
            }

            while (ejecutando && clienteInfo.Activo)
            {
                try
                {
                    MensajeRed mensaje = LeerMensaje(clienteInfo.Stream);
                    if (mensaje != null)
                    {
                        MensajeRecibido?.Invoke(mensaje);

                        // Reenviar a los demás
                        if (EsServidor)
                        {
                            EnviarATodosExcepto(mensaje, idCliente);
                        }
                    }
                }
                catch (IOException)
                {
                    clienteInfo.Activo = false;
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error leyendo cliente {idCliente}: {ex.Message}");
                    clienteInfo.Activo = false;
                    break;
                }
            }
        }
        

        // CLIENTE: Conectar a servidor
        public void ConectarAServidor(string ip, int puerto = 5555)
        {
            try
            {
                clienteLocal = new TcpClient();

                var result = clienteLocal.BeginConnect(ip, puerto, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));

                if (!success)
                {
                    clienteLocal.Close();
                    throw new Exception("Timeout al conectar");
                }

                clienteLocal.EndConnect(result);
                streamLocal = clienteLocal.GetStream();

                EsServidor = false;
                Conectado = true;
                ejecutando = true;

                Console.WriteLine($"Conectado a servidor {ip}:{puerto}");

                hiloEscucha = new Thread(EscucharMensajesCliente);
                hiloEscucha.IsBackground = true;
                hiloEscucha.Start();
            }
            catch (Exception ex)
            {
                Conectado = false;
                throw new Exception($"Error al conectar: {ex.Message}");
            }
        }

        // CLIENTE: Escuchar mensajes del servidor
        private void EscucharMensajesCliente()
        {
            

            while (ejecutando && Conectado)
            {
                try
                {
                    MensajeRed mensaje = LeerMensaje(streamLocal);
                    if (mensaje != null)
                    {
                        MensajeRecibido?.Invoke(mensaje);
                    }
                }
                catch (IOException)
                {
                    Desconectar();
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al leer mensaje: {ex.Message}");
                    Desconectar();
                    break;
                }
            }
        }

        private MensajeRed LeerMensaje(NetworkStream stream)
        {
           
            byte[] bufferLen = new byte[4];
            int read = stream.Read(bufferLen, 0, 4);
            if (read == 0) throw new IOException("Conexión cerrada");

            int longitud = BitConverter.ToInt32(bufferLen, 0);
            if (longitud <= 0) throw new IOException("Mensaje inválido");

            
            byte[] bufferDatos = new byte[longitud];
            int totalLeido = 0;
            while (totalLeido < longitud)
            {
                int leido = stream.Read(bufferDatos, totalLeido, longitud - totalLeido);
                if (leido <= 0) throw new IOException("Conexión cerrada durante lectura");
                totalLeido += leido;
            }

            string mensajeJson = Encoding.UTF8.GetString(bufferDatos);
            return JsonConvert.DeserializeObject<MensajeRed>(mensajeJson);
        }


        // Enviar mensaje (funciona para servidor y cliente)
        public void EnviarMensaje(MensajeRed mensaje)
        {
            if (!Conectado)
            {
                ErrorConexion?.Invoke("No hay conexión activa");
                return;
            }

            try
            {
                string mensajeJson = JsonConvert.SerializeObject(mensaje);
                byte[] datos = Encoding.UTF8.GetBytes(mensajeJson);
                byte[] longitud = BitConverter.GetBytes(datos.Length);

                if (EsServidor)
                {
                    // Enviar a todos los clientes
                    EnviarATodos(mensaje);
                }
                else
                {
                    // Enviar al servidor
                    streamLocal.Write(longitud, 0, longitud.Length);
                    streamLocal.Write(datos, 0, datos.Length);
                    streamLocal.Flush();
                }
            }
            catch (Exception ex)
            {
                ErrorConexion?.Invoke($"Error al enviar: {ex.Message}");
            }
        }

        // SERVIDOR: Enviar a todos los clientes conectados
        public void EnviarATodos(MensajeRed mensaje)
        {
            if (!EsServidor) return;

            string mensajeJson = JsonConvert.SerializeObject(mensaje);
            byte[] datos = Encoding.UTF8.GetBytes(mensajeJson);
            byte[] longitud = BitConverter.GetBytes(datos.Length);

            lock (lockClientes)
            {
                foreach (var cliente in clientesConectados.Values.Where(c => c.Activo))
                {
                    if (!cliente.Activo) continue;
                    try
                    {
                        cliente.Stream.Write(longitud, 0, longitud.Length);
                        cliente.Stream.Write(datos, 0, datos.Length);
                        cliente.Stream.Flush();
                    }
                    catch (IOException)
                    {
                        cliente.Activo = false;
                        try { cliente.Stream?.Close(); cliente.Cliente?.Close(); } catch { }
                        Console.WriteLine("Cliente desconectado (IOException).");
                    }
                    catch (Exception ex)
                    {
                        cliente.Activo = false;
                        Console.WriteLine($"Error enviando a cliente: {ex.Message}");
                    }
                }
            }
        }

        // SERVIDOR: Enviar a todos excepto uno
        private void EnviarATodosExcepto(MensajeRed mensaje, int idExcluido)
        {
            if (!EsServidor) return;

            string mensajeJson = JsonConvert.SerializeObject(mensaje);
            byte[] datos = Encoding.UTF8.GetBytes(mensajeJson);

            lock (lockClientes)
            {
                foreach (var kvp in clientesConectados)
                {
                    if (kvp.Key != idExcluido && kvp.Value.Activo)
                    {
                        try
                        {
                            kvp.Value.Stream.Write(datos, 0, datos.Length);
                            kvp.Value.Stream.Flush();
                        }
                        catch
                        {
                            kvp.Value.Activo = false;
                        }
                    }
                }
            }
        }

        public static bool ProbarPuerto(string ip, int puerto, int timeout = 5000)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(ip, puerto, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout);
                    if (success)
                    {
                        client.EndConnect(result);
                        return true;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static string ObtenerEstadoRed()
        {
            var sb = new StringBuilder();

            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        sb.AppendLine($"IP Local: {ip}");
                    }
                }

                sb.AppendLine("Estado de red: " +
                    (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()
                        ? "Conectado" : "Desconectado"));

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"Error obteniendo estado: {ex.Message}";
            }
        }



        public void Desconectar()
        {
            ejecutando = false;
            Conectado = false;

            try
            {
                if (EsServidor)
                {
                    lock (lockClientes)
                    {
                        foreach (var cliente in clientesConectados.Values)
                        {
                            cliente.Stream?.Close();
                            cliente.Cliente?.Close();
                        }
                        clientesConectados.Clear();
                    }
                    servidor?.Stop();
                }
                else
                {
                    streamLocal?.Close();
                    clienteLocal?.Close();
                }
            }
            catch { }
        }

        // Obtener IP local
        public static string ObtenerIPLocal()
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                return "127.0.0.1";
            }
        public int NumeroClientesConectados()
        {
            lock (lockClientes)
            {
                return clientesConectados.Count(c => c.Value.Activo);
            }
        }
    }
    }

