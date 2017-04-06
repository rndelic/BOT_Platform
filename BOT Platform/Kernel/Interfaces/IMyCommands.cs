using VkNet.Model;

namespace BOT_Platform
{
    interface IMyCommands
    {
        void AddMyCommandInPlatform();
        bool NeedCommandInfo(Message message, params object[] p);
    }
}
