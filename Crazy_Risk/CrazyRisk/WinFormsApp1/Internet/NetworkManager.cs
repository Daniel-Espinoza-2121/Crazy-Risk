using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;


    

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

        // Gestor de red principal
        public class NetworkManager
        {
            private TcpListener servidor;
            private TcpClient cliente;
            private NetworkStream stream;
            private Thread hiloEscucha;
            private bool ejecutando;

            public bool EsServidor { get; private set; }
            public bool Conectado { get; private set; }

            public event Action<MensajeRed> MensajeRecibido;
            public event Action<string> ErrorConexion;
            public event Action ClienteConectado;

            // SERVIDOR: Iniciar servidor
            public void IniciarServidor(int puerto = 5555)
            {
                try
                {
                    servidor = new TcpListener(IPAddress.Any, puerto);
                    servidor.Start();
                    EsServidor = true;
                Conectado = true;
                ejecutando = true;

                    hiloEscucha = new Thread(EsperarCliente);
                    hiloEscucha.IsBackground = true;
                    hiloEscucha.Start();

                    Console.WriteLine($"Servidor iniciado en puerto {puerto}");
                }
            catch (SocketException sex)
            {
                if (sex.SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    throw new Exception($"El puerto {puerto} ya está en uso");
                }
                throw new Exception($"Error al iniciar servidor: {sex.Message}");
            }
            catch (Exception ex)
                {
                    ErrorConexion?.Invoke($"Error al iniciar servidor: {ex.Message}");
                }
            }

            // SERVIDOR: Esperar conexión de cliente
            private void EsperarCliente()
            {
                try
                {
                    cliente = servidor.AcceptTcpClient();
                    stream = cliente.GetStream();
                    Conectado = true;
                    
                    Console.WriteLine("Cliente conectado");
                    ClienteConectado?.Invoke();

                    

                    // Comenzar a escuchar mensajes
                    EscucharMensajes();
                }
                catch (Exception ex)
                {
                    ErrorConexion?.Invoke($"Error al aceptar cliente: {ex.Message}");
                }
            }

            // CLIENTE: Conectar a servidor
            public void ConectarAServidor(string ip, int puerto = 5555)
            {
                try
                {
                    cliente = new TcpClient();

                
                var result = cliente.BeginConnect(ip, puerto, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));

                if (!success)
                {
                    cliente.Close();
                    throw new Exception("Timeout al conectar - verifica IP y puerto");
                }

                //cliente.Connect(ip, puerto);
                cliente.EndConnect(result);
                stream = cliente.GetStream();

                    EsServidor = false;
                    Conectado = true;
                    ejecutando = true;

                    Console.WriteLine($"Conectado a servidor {ip}:{puerto}");

                    // Comenzar a escuchar mensajes
                    hiloEscucha = new Thread(EscucharMensajes);
                    hiloEscucha.IsBackground = true;
                    hiloEscucha.Start();
                }
            catch (SocketException sex)
            {
                Conectado = false;
                if (sex.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    throw new Exception("Conexión rechazada - verifica que el servidor esté ejecutándose");
                }
                else if (sex.SocketErrorCode == SocketError.TimedOut)
                {
                    throw new Exception("Timeout - no se pudo alcanzar el servidor");
                }
                else
                {
                    throw new Exception($"Error de socket: {sex.Message}");
                }
            }

            catch (Exception ex)
                {
                Conectado = false;
                ErrorConexion?.Invoke($"Error al conectar: {ex.Message}");
                }
            }

            // Escuchar mensajes entrantes
            private void EscucharMensajes()
            {
                byte[] buffer = new byte[32768];

                while (ejecutando && Conectado)
                {
                    try
                    {
                        if (stream.DataAvailable)
                        {
                            int bytesLeidos = stream.Read(buffer, 0, buffer.Length);
                            if (bytesLeidos > 0)
                            {
                                string mensajeJson = Encoding.UTF8.GetString(buffer, 0, bytesLeidos);
                                MensajeRed mensaje = JsonConvert.DeserializeObject<MensajeRed>(mensajeJson);

                                MensajeRecibido?.Invoke(mensaje);
                            }
                        }
                        Thread.Sleep(10);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al leer mensaje: {ex.Message}");
                        Desconectar();
                        break;
                    }
                }
            }

            // Enviar mensaje
            public void EnviarMensaje(MensajeRed mensaje)
            {
                if (!Conectado || stream == null)
                {
                    ErrorConexion?.Invoke("No hay conexión activa");
                    return;
                }

                try
                {
                    string mensajeJson = JsonConvert.SerializeObject(mensaje);
                    byte[] datos = Encoding.UTF8.GetBytes(mensajeJson);
                    stream.Write(datos, 0, datos.Length);
                    stream.Flush();
                }
                catch (Exception ex)
                {
                    ErrorConexion?.Invoke($"Error al enviar mensaje: {ex.Message}");
                    Desconectar();
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
                // Obtener IP local
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        sb.AppendLine($"IP Local: {ip}");
                    }
                }

                // Probar conectividad básica
                sb.AppendLine("Estado de red: " + (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() ? "Conectado" : "Desconectado"));

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"Error obteniendo estado: {ex.Message}";
            }
        }

        // Desconectar
        public void Desconectar()
            {
                ejecutando = false;
                Conectado = false;

                try
                {
                    stream?.Close();
                    cliente?.Close();
                    servidor?.Stop();
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
        }
    }

