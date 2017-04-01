using VkNet.Model;

namespace BOT_Platform
{
    interface IMyCommands
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        void AddMyCommandInPlatform();
        bool NeedCommandInfo(Message message, params object[] p);
    }
}
