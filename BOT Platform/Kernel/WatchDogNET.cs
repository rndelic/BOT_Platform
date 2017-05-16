using System.Net;
using System.IO;
using System.Net.NetworkInformation;
using System;

namespace BOT_Platform
{
    public static class ConnectivityChecker
    {
        public static (bool status, string info) CheckConnection()
        {
            IPStatus status = IPStatus.TimedOut;
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(@"vk.com");
                status = reply.Status;
            }
           
            catch
            {
                return (false, 
                    "[NET_ERROR " + DateTime.Now.ToLongTimeString() + "] Непредвиденная ошибка при попытке соединения с vk.com\n");
            }
            if (status != IPStatus.Success)
            {
                return (false,
                    "[NET_ERROR " + DateTime.Now.ToLongTimeString() + "] Не удалось подключиться к vk.com: " + status.ToString() + "\n");
            }
            else
            {
                return (true,
                    "[NET_INFO " + DateTime.Now.ToLongTimeString() + "] Соединение с vk.com установлено (SUCCSSES).\n");
            }
        }
    }
}