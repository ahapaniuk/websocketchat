using System.Net.Sockets;
using WebSockets.Server.WebSocket;
using WebSockets.Common;

namespace WebSocketsCmd.Server
{
    internal class ChatWebSocketService : WebSocketService
    {
        private readonly IWebSocketLogger _logger;

        public ChatWebSocketService(Stream stream, TcpClient tcpClient, string header, IWebSocketLogger logger)
            : base(stream, tcpClient, header, true, logger)
        {
            _logger = logger;
        }

        protected override void OnTextFrame(string text)
        {

            string response = "-----------> SERVER received and answered: " + text;

            //string response = "Hello from the SERVER!";
            base.Send(response);

            // limit the log message size
            string logMessage = response.Length > 100 ? response.Substring(0, 100) + "..." : response;
            _logger.Information(this.GetType(), logMessage);
        }
    }
}
