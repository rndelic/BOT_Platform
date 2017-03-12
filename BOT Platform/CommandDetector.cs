/*
 * Данный класс не доработан.
 * v0.0.0.2
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOT_Platform
{
    static class CommandDetector
    {
        const string ARGS = "[param]";
        static Dictionary<string, CommandsMenu> commandsHeads;

        struct CommandsMenu
        {
            public Prefix prefix;
            public Dictionary<string, string> prefixCommands;

            public CommandsMenu(Prefix p)
            {
                this.prefix = p;
                this.prefixCommands = new Dictionary<string, string>();
            }
        }

        static CommandDetector()
        {
            commandsHeads = new Dictionary<string, CommandsMenu>();
        }

        public static void TryBotCommand(string command)
        {
        }

        public static void TryAddCommand(string command, string description)
        {
            StringBuilder lastAddedCommand = new StringBuilder();
            List<string> keys = ParseCommand(command);

            int index = 0;

            Prefix prefix = new Prefix();
            foreach(string key in keys)
            {
                if (index == 0)
                {
                   prefix = TryAddNewHead(ref prefix, key);
                }
                else if(index == keys.Count - 1)
                {
                    TryAddEnd(ref prefix, key, keys.First(), lastAddedCommand.ToString(), description);
                }
                else
                {
                    prefix = TryAddMiddle(ref prefix, key);
                }
                lastAddedCommand.Append(key + " ");
                ++index;
            }
        }

        private static ref Prefix TryAddNewHead(ref Prefix prefix, string key)
        {
            if (commandsHeads.ContainsKey(key))
            {
                prefix = commandsHeads[key].prefix;
            }
            else
            {
                if (key == ARGS) prefix.IsArgument = true;
                commandsHeads.Add(key, new CommandsMenu(prefix));

            }
            return ref prefix;
        }

        private static void TryAddEnd(ref Prefix prefix, string key, string firstKey, string command, string description)
        {
            if(prefix.NextPrefixsDict.ContainsKey(key))
            {
                Console.WriteLine("Не удалось добавить команду \\" + ".\n" +
                                   "Команда уже определена.\n");
            }
            else
            {
                Prefix p = new Prefix();
                if (key == ARGS) p.IsArgument = true;
                p.IsEnd = true;

                prefix.NextPrefixsDict.Add(key, p);
                prefix = p;

                commandsHeads[firstKey].prefixCommands.Add(command + key, description);
            }
        }

        private static ref Prefix TryAddMiddle(ref Prefix prefix, string key)
        {
            if (prefix.NextPrefixsDict.ContainsKey(key)) prefix = prefix.NextPrefixsDict[key];
            else
            {
                Prefix p = new Prefix();
                if (key == ARGS) p.IsArgument = true;
                prefix.NextPrefixsDict.Add(key, p);
                prefix = p;
            }
            return ref prefix;
        }

        public static List<string> ParseCommand(string command)
        {
            List<string> prefixes = new List<string>();

            for (int i = 0; i < command.Length; i++)
            {
                StringBuilder sB = new StringBuilder();
                for (int j = i; j<command.Length && command[j] != ' '; j++, i++)
                {
                    sB.Append(command[j]);
                }
                if(sB.ToString() != "") prefixes.Add(sB.ToString());
            }

            return prefixes;
        }
        class Prefix
        {
            public Dictionary<string, Prefix> NextPrefixsDict;
            public bool IsEnd     ;
            public bool IsArgument;

            public Prefix()
            {
                this.NextPrefixsDict = new Dictionary<string, Prefix>();
            }

        }

        public static void Main()
        {
            TryAddCommand($"напиши сообщение {ARGS}", "test");
            TryAddCommand($"напиши в чат сообщение {ARGS}", "test");
            TryAddCommand($"анонимно {ARGS} отправь", "test");
            TryAddCommand($"напиши сообщение     {ARGS}", "");
            Console.ReadLine();
        }
    }
}
