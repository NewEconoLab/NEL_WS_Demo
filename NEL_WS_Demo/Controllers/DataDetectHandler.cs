using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NEL_WS_Web
{
    /// <summary>
    /// 
    /// 数据监测处理器
    /// 
    /// </summary>
    public class DataDetectHandler
    {
        public static void loopProcessTask()
        {
            while (true)
            {
                var now = DateTime.Now;
                Thread.Sleep(1000 * 2);
                try
                {
                    var sessions = WebSocketHandler.socketDict.Values.ToList();
                    if (WebSocketHandler.socketDict.Values.Count > 0 && needToSendNotify(out string message))
                    {
                        broadcast(message, sessions);
                        Console.WriteLine(now + " wsService: cli.count=" + sessions.Count);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(now + " wsService: Error={0}, Stack={1}", ex.Message, ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// 
        /// 广播消息
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessions"></param>
        static async void broadcast(string message, List<WebSocketHandler> sessions)
        {
            //

            Console.WriteLine(" broadcase message=" + message);
            foreach (var session in sessions)
            {
                try
                {
                    await session.sendMessageAsync(message);
                } catch
                {
                    await session.onDisConnect();
                }
            }
        }


        /// <summary>
        /// 
        /// 数据变动监测
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static bool needToSendNotify(out string message)
        {
            long nextblockindex = getBlockCount();
            if (nextblockindex > 0 && nextblockindex > lastblockindex)
            {
                lastblockindex = nextblockindex;
                message = new JObject() { { "lastBlockindex", nextblockindex } }.ToString();
                return true;
            }
            message = "";
            return false;
        }
        static long lastblockindex = 0;
        static long getBlockCount()
        {
            var client = new MongoClient(WsConst.new_block_mongodbConnStr_testnet);
            var database = client.GetDatabase(WsConst.new_block_mongodbDatabase_testnet);
            var collection = database.GetCollection<BsonDocument>("system_counter");

            string findStr = new JObject() { { "counter", "block" } }.ToString();
            var res = collection.Find(findStr).ToList();
            if (res != null && res.Count > 0)
            {
                return long.Parse(res[0]["lastBlockindex"].ToString());
            }
            return 0;
        }
    }
    class WsConst
    {
        public static string new_block_mongodbConnStr_testnet = "mongodb://nelDataStorage:NELqingmingzi1128@dds-bp1df57f935202e41897-pub.mongodb.rds.aliyuncs.com:3717,dds-bp1df57f935202e42907-pub.mongodb.rds.aliyuncs.com:3717/NeoBlockBaseData?replicaSet=mgset-10445701";
        public static string new_block_mongodbDatabase_testnet = "NeoBlockBaseData";

        public static string wsUrl = "http://127.0.0.1:8888/";
    }
}
