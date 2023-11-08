using System.Net.Sockets;
using WebSockets.Server.WebSocket;
using WebSockets.Common;
using WebSockets;

namespace WebSocketsCmd.Server
{
    internal class ChatWebSocketService : WebSocketService
    {
        private readonly IWebSocketLogger _logger;
        private readonly WebServer _server;
        private int _clientId;

        public ChatWebSocketService(WebServer server, Stream stream, TcpClient tcpClient, string header, IWebSocketLogger logger)
            : base(stream, tcpClient, header, true, logger)
        {
            _server = server;
            _logger = logger;
        }

        public int ClientId
        {
            get { return _clientId; }
            set
            {

                _clientId = value;
            }

        }

        public override int GetClientIdentificationId()
        { 
            return _clientId;
        }


        public void SendToCliend(string message)
        {
            base.Send(message);
        }

        protected override void OnTextFrame(string text)
        {
            string response = null;

            // POC
            // STEP 1.1 Validation

            if (text.Contains("AUTH"))
            { 
                // проверяем Token, креды и т.д.
                // получаем ClientId и связываем с коннектом
                string? clientId = text.Split(' ').LastOrDefault()?.ToString().Split('=').LastOrDefault()?.ToString();


                if (!String.IsNullOrEmpty(clientId))
                {
                    this.ClientId = int.Parse(clientId);
                }
                
            }

            // STEP 1.2 Send message from client to client
            // ClientIdSrc:UserIdSrc ClientIdDst:UserIdDst Message:[Text]
            // ClientIdSrc:1 ClientIdDst:2 Message:555
            if (text.Contains("ClientIdDst"))
            {
                int clientDstId = 0;
                string message = string.Empty;

                string? clientDst = text.Split(' ')[1].Split(':').LastOrDefault();
                if (!String.IsNullOrEmpty(clientDst))
                {
                    clientDstId = int.Parse(clientDst);

                    message = text.Split(' ')[2].Split(':').LastOrDefault();
                }

                var clientDstService = _server.GetServiceForClientId(clientDstId);
                if (clientDstService != null && clientDstService is ChatWebSocketService && !string.IsNullOrEmpty(message))
                {
                    (clientDstService as ChatWebSocketService)?.SendToCliend($"Message from client {_clientId}: " + message);

                    response = $"* -----------> SERVER received and sent to user: {clientDstId}";

                    base.Send(response);
                }
                else
                {
                    response = $"Client {clientDstId} is not connected";

                    base.Send(response);

                    // offline mode here...
                }
            }

           
            response = "* -----------> SERVER received and answered: " + text;

                //string response = "Hello from the SERVER!";
            base.Send(response);

          

            // limit the log message size
            string logMessage = !string.IsNullOrEmpty(response) && response.Length > 100 ? response.Substring(0, 100) + "..." : response;
            _logger.Information(this.GetType(), logMessage);
        }
    }
}
