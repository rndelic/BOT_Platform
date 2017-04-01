using System.Net;
using System.IO;
using System.Net.NetworkInformation;
using System;

namespace BOT_Platform
{
    public static class ConnectivityChecker
    {
        public static bool CheckConnection()
        {
            IPStatus status = IPStatus.TimedOut;
            try
            {
                Console.WriteLine("[NET_INFO] Попытка подключиться к vk.com...");
                Ping ping = new Ping();
                PingReply reply = ping.Send(@"vk.com");
                status = reply.Status;
            }
            catch
            {
                Console.WriteLine("[NET_ERROR] Непредвиденная ошибка при попытке соединения с vk.com\n");
                return false;
            }
            if (status != IPStatus.Success)
            {
                Console.WriteLine("[NET_ERROR] Не удалось подключиться к vk.com: " + status.ToString() + "\n");
                return false;

            }
            else
            {
                Console.WriteLine("[NET_INFO] Соединение с vk.com установлено.\n");
                return true;
            }
        }
    }
}