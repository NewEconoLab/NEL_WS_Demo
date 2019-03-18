using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NEL_WS_Web
{
    /// <summary>
    /// 
    /// 连接处理器
    /// 
    /// </summary>
    class WebSocketHandler
    {
        public static Dictionary<UInt32, WebSocketHandler> socketDict = new Dictionary<uint, WebSocketHandler>();
        public static UInt32 sessionId = 0;

        private HttpContext context;
        private WebSocket ws;
        public UInt32 id { get; }
        private CancellationToken cancelToken;

        public WebSocketHandler(HttpContext context, WebSocket ws, bool cancelTokenFlag = false)
        {
            this.context = context;
            this.ws = ws;
            id = ++sessionId;
            cancelToken = new CancellationToken(cancelTokenFlag);

        }

        public string LogInfo => new JObject() { { "id", id } }.ToString();

        /// <summary>
        /// 
        /// 已建立连接
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task onConnected()
        {
            Console.WriteLine(" session connected,sessionId="+id);
            socketDict.Add(id, this);
            await sendMessageAsync(LogInfo);
        }


        /// <summary>
        /// 
        /// 已断开连接
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task onDisConnect()
        {
            Console.WriteLine(" session disconnect,sessionId=" + id);
            socketDict.Remove(id);
        }

        /// <summary>
        /// 
        /// 接收消息
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<string> recvMessageAsync()
        {
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    var buf = new ArraySegment<byte>(new byte[1024]);
                    result = await ws.ReceiveAsync(buf, cancelToken);
                    ms.Write(buf.Array, buf.Offset, result.Count);

                } while (!result.EndOfMessage);
                //
                using (var rd = new StreamReader(ms, Encoding.UTF8))
                {
                    return await rd.ReadToEndAsync();
                }
            }
        }


        /// <summary>
        /// 
        /// 发送消息
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task sendMessageAsync(string message)
        {
            await ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Binary, true, cancelToken);
        }

    }
}
