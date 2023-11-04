using WebSocketsCmd.Server;
using WebSocketsCmd.Client;
using WebSockets.Common;
using WebSockets;
using WebSockets.Events;
using System.Security.Cryptography.X509Certificates;

namespace WebSocketsCmd
{
    public class Program
    {
        private static string _settingsCertPassword = null; 
        private static string _settingsWebRoot = null;
        private static string _settingsDefaultCertificateFile = null;
        private static int _settingsDefaultPort=80;

        private static bool startTestClient = false;

        private static IWebSocketLogger _logger;

        private static void TestClient(object state)
        {
            try
            {
                string url = (string) state;
                using (var client = new ChatWebSocketClient(true, _logger))
                {
                    Uri uri = new Uri(url);
                    client.TextFrame += Client_TextFrame;
                    client.ConnectionOpened += Client_ConnectionOpened;

                    // test the open handshake
                    client.OpenBlocking(uri);
                }

                _logger.Information(typeof (Program), "Client finished, press any key");
            }
            catch (Exception ex)
            {
                _logger.Error(typeof (Program), ex.ToString());
                _logger.Information(typeof(Program), "Client terminated: Press any key");
            }

            Console.ReadKey();
        }

        private static void Client_ConnectionOpened(object sender, EventArgs e)
        {
            _logger.Information(typeof(Program), "Hi, I'm a CLIENT! Client: Connection Opened");
            var client = (ChatWebSocketClient)sender;
            
            client.Send("Hi, I'm a CLIENT!");
            
            client.Dispose();
        }

        private static void Client_TextFrame(object sender, TextFrameEventArgs e)
        {
            _logger.Information(typeof(Program), "* -----------> CLIENT received: {0}", e.Text);
            var client = (ChatWebSocketClient)sender;

            // lets test the close handshake
            if (e.Text=="exit")            
                client.Dispose();
        }

        private static void Main(string[] args)
        {
            _logger = new WebSocketLogger();           

            try
            {
                int port = _settingsDefaultPort;
                string webRoot = _settingsWebRoot;
                if (!Directory.Exists(webRoot))
                {
                    string baseFolder = AppDomain.CurrentDomain.BaseDirectory;
                    _logger.Warning(typeof(Program), "Webroot folder {0} not found. Using application base directory: {1}", webRoot, baseFolder);
                    webRoot = baseFolder;
                }

                // used to decide what to do with incoming connections
                ServiceFactory serviceFactory = new ServiceFactory(webRoot, _logger);

                using (WebServer server = new WebServer(serviceFactory, _logger))
                {
                    if (port == 443)
                    {
                        X509Certificate2 cert = GetCertificate();
                        server.Listen(port, cert);    
                    }
                    else
                    {
                        server.Listen(port);  
                    }

                    Console.WriteLine("Server started and wait for the connections");

                    if(startTestClient)
                    { 
                        Console.WriteLine("Test client created");
                        Thread clientThread = new Thread(new ParameterizedThreadStart(TestClient));
                        clientThread.IsBackground = false;

                        // to enable ssl change the port to 443 in the settings file and use the wss schema below
                        // clientThread.Start("wss://localhost/chat");

                        clientThread.Start("ws://localhost/chat");
                    }

                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(typeof(Program), ex);
                Console.ReadKey();
            }
        }


        private static X509Certificate2 GetCertificate()
        {
            // it is clearly WRONG to store the certificate and password insecurely on disk like this but this is a demo
            // you would normally use the built in windows certificate store
            string certFile = _settingsDefaultCertificateFile;
            if (!File.Exists(certFile))
            {
                throw new FileNotFoundException("Certificate file not found: " + certFile);
            }

            string certPassword = _settingsCertPassword;
            var cert = new X509Certificate2(certFile, certPassword);
            _logger.Information(typeof(Program), "Successfully loaded certificate");
            return cert;
        }

    }
}
