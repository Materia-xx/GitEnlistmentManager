using GitEnlistmentManager.Globals;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager.ClientServer
{
    public class GemServer
    {
        private static readonly IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        public static int Port { get; } = 8397;
        private TcpListener Listener { get; }
        private Func<GemCSCommand, Task>? CommandProcessor { get; set; }

        // Keep track of the threads and create CancellationTokenSource for each
        private Dictionary<Thread, CancellationTokenSource> m_ThreadDictionary = new Dictionary<Thread, CancellationTokenSource>();

        public GemServer(Func<GemCSCommand, Task> commandProcessor)
        {
            Listener = new TcpListener(ipAddress, Port);
            this.CommandProcessor = commandProcessor;
        }

        /// <summary>
        /// Start the server
        /// </summary>
        public async Task Start()
        {
            // Start the underlying TcpListener
            Listener.Start();

            // Create a thread for the server to listen on
            //Thread t = new Thread(new ThreadStart(StartListener));
            //t.Start();
            await StartListener().ConfigureAwait(false);
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void Stop()
        {
            foreach (KeyValuePair<Thread, CancellationTokenSource> pair in m_ThreadDictionary)
            {
                // Cancel all of the client threads
                pair.Value.Cancel();
            }

            Listener.Stop();
        }

        public async Task StartListener()
        {
            try
            {
                while (true)
                {
                    // Waiting for a Remoting Connection.
                    TcpClient client = Listener.AcceptTcpClient();

                    // Remoting Client Connected from IP Address:{client.Client.RemoteEndPoint}
                    //Thread t = new Thread(new ParameterizedThreadStart(HandleClient)); // TODO: remove the threading layer if this async works
                    await HandleClient(client).ConfigureAwait(false);

                    // Add a thread to handle this command
                    //m_ThreadDictionary.Add(t, new CancellationTokenSource());
                    //t.Start(client);
                }
            }
            catch (SocketException e)
            {
                Debug.WriteLine("SocketException: {0}", e);
            }
        }

        public async Task HandleClient(object? obj)
        {
            if (obj is not TcpClient client)
            {
                return;
            }

            CancellationTokenSource cancelToken = m_ThreadDictionary[Thread.CurrentThread];
            var stream = client.GetStream();
            string remoteCommand;
            byte[] bytes = new byte[512];
            int i;
            try
            {
                while (!cancelToken.IsCancellationRequested && (i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    string hex = BitConverter.ToString(bytes);
                    remoteCommand = Encoding.ASCII.GetString(bytes, 0, i);
                    Debug.WriteLine("{1}: Received: {0}", remoteCommand, Environment.CurrentManagedThreadId);
                    await ProcessCommand(remoteCommand).ConfigureAwait(false);
                }

                m_ThreadDictionary[Thread.CurrentThread].Dispose();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: {0}", e);
            }
            finally
            {
                // Remove this thread from the map
                m_ThreadDictionary.Remove(Thread.CurrentThread);
                client.Close();
            }
        }

        private async Task ProcessCommand(string jsonCommand)
        {
            var cmd = JsonConvert.DeserializeObject<GemCSCommand>(jsonCommand, GemJsonSerializer.Settings);
            if (cmd == null || this.CommandProcessor == null)
            {
                return;
            }

            await this.CommandProcessor(cmd).ConfigureAwait(false);
        }
    }
}
