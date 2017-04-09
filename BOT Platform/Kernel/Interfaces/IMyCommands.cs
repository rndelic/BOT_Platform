using VkNet.Model;

namespace BOT_Platform.Interfaces
{
    interface IMyCommands
    {
        void AddMyCommandInPlatform();
        bool NeedCommandInfo(Message message, params object[] p);
    }
}
