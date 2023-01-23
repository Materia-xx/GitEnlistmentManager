using System.Net.Sockets;
using System.Text;

namespace GitEnlistmentManager.ClientServer
{
    public class GemClient : TcpClient
    {
        private static readonly string ipAddress = "127.0.0.1";

        public static GemClient Instance { get; } = new();

        public GemClient()
            : base(ipAddress, GemServer.Port)
        {
        }

        public void SendCommand(GemCSCommand cmd)
        {
            NetworkStream stream = GetStream();
            var cmdJson = cmd.Serialize();
            byte[] data = Encoding.ASCII.GetBytes(cmdJson);
            stream.Write(data, 0, data.Length);
            stream.Close();
        }
    }
}
