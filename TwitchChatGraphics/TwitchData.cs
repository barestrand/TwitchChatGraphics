// Copyright (c) 2016, Henrik Barestrand, All rights reserved.
using ChatGraphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TwitchBot
{

    public class TwitchData
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        List<progSetting> settings;
        IrcClient irc;

        bool writefile = true;
        StreamWriter file;

        List<Preamble> preambles;
        List<string> exceptions;
        List<icon> iconList;

        List<ChatWord> chatStamps;

        Bitmap background;
        Bitmap textBack;

        public TwitchData()
        {
            settings = LoadSettings();

            irc = new IrcClient(getVal("ip"), int.Parse(getVal("port")), getVal("username"), getVal("oauth_password"));
            irc.joinRoom(getVal("channelname"));

            #region ConsoleSettings

            Console.SetWindowSize(50, 60);
            const int SW_HIDE = 0;
            const int SW_SHOW = 5;
            IntPtr consoleHandle = GetConsoleWindow();
            if (getVal("console") == "true")
            {
                ShowWindow(consoleHandle, SW_SHOW);
            }
            else
            {
                ShowWindow(consoleHandle, SW_HIDE);
            }

            #endregion

            preambles = LoadPreambles();
            exceptions = LoadExceptions();
            iconList = loadEmoteIcons();

            file = new StreamWriter(Program.getPath("chatdata.txt"), true);
            file.AutoFlush = true;

            chatStamps = new List<ChatWord>();

            background = new Bitmap(Program.getPath("res/background.png"));
            textBack = new Bitmap(Program.getPath("res/textarea.png"));
        }

        public Bitmap Run()
        {
            string message;
            message = irc.readMessage();


            if (message != null && message.Length >= 1)
            {
                message = message.Substring(message.IndexOf(" :") + 2);

                if (message.Contains("End of /NAMES list"))
                {
                    Console.Clear();
                    Console.WriteLine(getVal("username") + " Connected");
                }

                DateTime time = DateTime.Now;

                if (writefile)
                {
                    file.WriteLine(time.ToString("yyyy-MM-dd HH:mm:ss | ") + message);
                }

                #region handle preambles

                // run message straight through all preambles
                bool processed = false;
                foreach (Preamble item in preambles)
                {
                    switch (item.instruction)
                    {
                        case Preamble.ambleType.SUPRESS:
                            if (Array.Exists(item.args, arg =>
                            {
                                if (IsASCII(arg))// compare with uppercase convention on preamble if it is only ascii
                                    {
                                    return message.ToUpper().Contains(arg.ToUpper());
                                }
                                else // otherwise use direct string
                                    {
                                    return message.Contains(arg);
                                }
                            }))
                            {
                                processed = true; // Don't do anything and just mark message as treated
                            }
                            break;
                        case Preamble.ambleType.OR: // message contains any of provided arguments so replace them in message
                            {
                                foreach (string arg in item.args)
                                {
                                    // TODO fix beginning and end of string comparison
                                    if ((IsASCII(arg) && message.ToUpper().Contains(" " + arg.ToUpper() + " ")) || message.Contains(" " + arg + " "))// compare with uppercase convention on preamble if it is only ascii
                                    {
                                        message = Regex.Replace(message, arg, item.target, RegexOptions.IgnoreCase);
                                    }
                                    else if ((IsASCII(arg) && message.ToUpper().StartsWith(arg.ToUpper() + " ")) || message.StartsWith(arg + " "))// compare with uppercase convention on preamble if it is only ascii
                                    {
                                        message = Regex.Replace(message, arg, item.target, RegexOptions.IgnoreCase);
                                    }
                                    else if ((IsASCII(arg) && message.ToUpper().EndsWith(" " + arg.ToUpper())) || message.EndsWith(" " + arg))// compare with uppercase convention on preamble if it is only ascii
                                    {
                                        message = Regex.Replace(message, arg, item.target, RegexOptions.IgnoreCase);
                                    }
                                }
                            }
                            break;
                        default: // evil enumeration value found
                            break;
                    }
                    if (processed) break; // the preambles were checked and message has been treated
                }

                foreach (Preamble item in preambles)
                {
                    if (item.instruction == Preamble.ambleType.AND)
                    {
                        if (IsASCII(message) && Array.TrueForAll(item.args, arg =>
                        {
                            return message.ToUpper().Contains(arg.ToUpper());
                        }))// Is message ASCII, and if it is, can ALL argument strings be found in message if both are compared with uppercase convention
                        {
                            ChatWord.findAddOrCreate(ref chatStamps, item.target, time);
                            processed = true;
                        }
                        else if (Array.TrueForAll(item.args, arg =>
                        {
                            if (IsASCII(arg))// compare with uppercase convention on preamble if it is only ascii
                                {
                                return message.ToUpper().Contains(arg.ToUpper());
                            }
                            else // otherwise use direct string
                                {
                                return message.Contains(arg);
                            }
                        }))
                        {
                            ChatWord.findAddOrCreate(ref chatStamps, item.target, time);
                            processed = true;
                        }
                    }
                    if (processed) break; // the preambles were checked and message has been treated
                }

                #endregion

                #region default processing of message

                if (!processed && message.Length < int.Parse(getVal("message_maxlength")))
                {

                    string[] splitted = message.ToLower().Split(' '); // split as default with space delimiter

                    List<string> splits = new List<string>();
                    foreach (string item in splitted)
                    {
                        if (!splits.Exists(entry => entry == item)) // Cleanup for multiple entries
                        {
                            splits.Add(item);
                        }
                    }
                    // Add new entries
                    foreach (string item in splits)
                    {
                        if (!exceptions.Exists(entry => entry == item))
                        {
                            ChatWord.findAddOrCreate(ref chatStamps, item, time);
                        }
                    }
                }

                #endregion

                // Update and clean up list by time
                for (int i = 0; i < chatStamps.Count; i++)
                {
                    chatStamps[i].UpdateTimes(time, 0, 0, 20); // TODO: implement seconds counter
                    if (chatStamps[i].timeStamps.Count == 0)
                    {
                        chatStamps.RemoveAt(i);
                    }
                }

                // Sort words in alphabetical order and print to console
                chatStamps.Sort((x, y) => x.word.CompareTo(y.word));
                Console.Clear();
                for (int i = 0; i < chatStamps.Count; i++)
                {
                    Console.WriteLine(chatStamps[i].timeStamps.Count + " : " + chatStamps[i].word);
                }

            }

            #region Generate Graphics

            Bitmap canvas = new Bitmap(background); // rewrite canvas (disposed in other thread)

            // Sort words by occurence
            chatStamps.Sort((x, y) => -x.timeStamps.Count.CompareTo(y.timeStamps.Count));

            int nextTextY = 0, cc = 0;
            int nextIconY = 25;
            for (int i = 0; i < chatStamps.Count; i++)
            {
                int index = -1;
                if ((index = iconList.FindIndex(icon => icon.name == chatStamps[i].word)) != -1)
                {
                    if (nextIconY < 410)
                    {
                        int A = (int)Math.Pow(30 + chatStamps[i].timeStamps.Count * 1.5f, 2); // Area

                        int hh = (int)Math.Sqrt(A * iconList[index].image.Size.Height / iconList[index].image.Size.Width);

                        ppaForm.CopyRegionIntoImage(
                            iconList[index].image,
                            ppaForm.entireRegion(iconList[index].image),
                            ref canvas,
                            new Point(background.Width / 2, nextIconY),
                            new Size(iconList[index].image.Size.Width * hh / iconList[index].image.Size.Height, hh),
                            true);

                        nextIconY += hh;
                    }
                }
                else
                {
                    if ((nextTextY < 210 || cc < 10) && chatStamps[i].timeStamps.Count > 1)
                    {
                        int hh = 20 + chatStamps[i].timeStamps.Count * 1;
                        ppaForm.TextArea(ref textBack, canvas, chatStamps[i].word, hh, new Point(962, nextTextY));
                        nextTextY += hh;
                        cc += 1;
                    }
                }
            }

            #endregion

            return canvas;
        }

        private static List<progSetting> LoadSettings()
        {
            List<progSetting> settings = new List<progSetting>();
            var reader = new StreamReader(File.OpenRead(Program.getPath("settings.csv")));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine().Trim(); // settings are case sensitive!
                string[] args = line.Split(new[] { "-->" }, StringSplitOptions.None); // argument,setting
                if (args[0] != null && args[1] != null)
                {
                    settings.Add(new progSetting(args[0].Trim(), args[1].Trim()));
                }
                else
                {
                    MessageBox.Show("Problem in loading settings, program expected to crash");
                }
            }
            reader.Close();
            return settings;
        }

        private class progSetting
        {
            public string varName;
            public string value;

            public progSetting(string varName, string value)
            {
                this.varName = varName;
                this.value = value;
            }
        }

        private string getVal(string item)
        {
            // find category and add, otherwise create new
            int index = -1;
            if ((index = settings.FindIndex(s => s.varName == item)) != -1)
            {
                return settings[index].value;
            }
            else  // if not found and not in exceptions
            {
                MessageBox.Show("Problem in settings, program expected to crash");
                return null;
            }
        }

        private bool IsASCII(string value)
        {
            // ASCII encoding replaces non-ascii with question marks, so we use UTF8 to see if multi-byte sequences are there
            return Encoding.UTF8.GetByteCount(value) == value.Length;
        }

        private class Preamble
        {
            public ambleType instruction;
            public string[] args;
            public string target;

            public Preamble(ambleType instruction, string[] args, string target)
            {
                this.instruction = instruction;
                this.args = args;
                this.target = target;
            }

            public enum ambleType
            {
                AND,
                OR,
                SUPRESS
            };
        }

        private List<Preamble> LoadPreambles()
        {
            List<Preamble> preambleList = new List<Preamble>();
            var reader = new StreamReader(File.OpenRead(Program.getPath("preambles.csv")));
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] args = line.Split(new[] { "-->" }, StringSplitOptions.None); // --> split
                if ((args[0] != null) && args[0].Contains("SUPRESS")) // check condition supress
                {
                    string[] words = getParanthesisArgs(args[0]);
                    preambleList.Add(new Preamble(Preamble.ambleType.SUPRESS, words, null));
                }
                else if (args.Length == 2 && (args[0] != null) && (args[1] != null))
                {
                    if (args[0].Contains("OR"))
                    {
                        string[] words = getParanthesisArgs(args[0]);
                        preambleList.Add(new Preamble(Preamble.ambleType.OR, words, args[1].Trim()));
                    }
                    else if (args[0].Contains("AND"))
                    {
                        string[] words = getParanthesisArgs(args[0]);
                        preambleList.Add(new Preamble(Preamble.ambleType.AND, words, args[1].Trim()));
                    }
                }
                else
                {
                    MessageBox.Show("Could not load preamble: " + line);
                }
            }
            reader.Close();
            return preambleList;
        }

        private string[] getParanthesisArgs(string line)
        {
            string lhs = line.Split('(', ')')[1];
            string[] words = lhs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = words[i].Trim();
            }
            return words;
        }

        private List<string> LoadExceptions()
        {
            List<string> exceptions = new List<string>();
            var reader = new StreamReader(File.OpenRead(Program.getPath("exceptions.csv")));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine().ToLower().Trim();
                if (line != null)
                {
                    exceptions.Add(line);
                }
            }
            reader.Close();
            return exceptions;
        }

        private List<icon> loadEmoteIcons()
        {
            List<icon> iconList = new List<icon>();

            //Load icons from filename list
            var reader = new StreamReader(File.OpenRead(Program.getPath("res/emotes.csv")));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine().ToLower();
                string[] args = line.Split(new[] { "-->" }, StringSplitOptions.None); // nameID,filename (without spaces)
                if (args[0] != null && args[1] != null)
                {
                    icon item = new icon(args[0].Trim(), new Bitmap(Program.getPath("res/" + args[1].Trim())));
                    iconList.Add(item);
                }
            }
            reader.Close();

            return iconList;
        }

        private class icon
        {
            public string name { get; set; }
            public Bitmap image { get; set; }

            public icon(string id, Bitmap image)
            {
                name = id;
                this.image = image;
            }
        }

        private class ChatWord
        {
            public string word;
            public List<DateTime> timeStamps = new List<DateTime>();

            public ChatWord(string Word, DateTime Time)
            {
                this.word = Word;
                timeStamps.Add(Time);
            }

            public void Add(DateTime Time)
            {
                timeStamps.Add(Time);
            }

            public void UpdateTimes(DateTime currentTime, int hh, int mm, int ss)
            {
                TimeSpan delta = new TimeSpan(hh, mm, ss);

                for (int i = 0; i < timeStamps.Count; i++)
                {
                    if (delta < currentTime.Subtract(timeStamps[i]))
                    {
                        timeStamps.RemoveAt(i);
                    }
                    //TODO: implement decay sorting
                }
            }

            public static void findAddOrCreate(ref List<ChatWord> list, string item, DateTime time)
            {
                // find category and add, otherwise create new
                int index = -1;
                if ((index = list.FindIndex(cw => cw.word.ToUpper() == item.ToUpper())) >= 0) // always compare with uppercase
                {
                    list[index].Add(time);
                }
                else if (index == -1) // if not found and not in exceptions
                {
                    list.Add(new ChatWord(item, time));
                }
            }
        }
    }
}
