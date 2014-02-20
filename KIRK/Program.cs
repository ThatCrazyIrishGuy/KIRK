using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.ComponentModel;
using System.Speech.Synthesis;

namespace KIRK
{
    class Program
    {
        public static List<Word> associatedWords = new List<Word>();
        static Random rand = new Random();
        static SpeechSynthesizer synth = new SpeechSynthesizer();
        static List<string> lines = new List<string>();
        static Thread enterpriseload = new Thread(enterpriseloading);
        static int timer = 0;
        static int location = 0;
        static bool TOSstatus = true;


        static void Main(string[] args)
        {
            loadBrain();
            synth.SetOutputToDefaultAudioDevice();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("what would you like to do?                         Text-to-Speech Enabled:" + TOSstatus +"\n");
                Console.WriteLine("1) add text to KIRK's memory");
                Console.WriteLine("2) have a conversation with KIRK");
                Console.WriteLine("3) Erase Kirk's brain");
                Console.WriteLine("T) Toggle text-to-speech");
                Console.WriteLine("Q) to quit (quitting in any other way will not save KIRK's Brain!");
                string input = Console.ReadLine();


                switch (input)
                {
                    case "1":
                        KIRKreader();
                        
                        break;
                    case "2":
                        KIRKtalk();
                        break;

                    case "3":
                        EraseBrain();
                        break;

                    case "T":
                    case "t":
                        TOSstatus = !TOSstatus;
                        break;

                    case "Q":
                    case "q":
                        saveBrain();
                        break;
                }

            }
        }

        static void KIRKreader()
        {
            bool fileExists = false;
            string filepath = "";

            while (fileExists == false)
            {
                Console.WriteLine("Enter the filepath of the .txt file or enter \\q to go back");
                filepath = Console.ReadLine();
                if (File.Exists(filepath))
                {
                    fileExists = true;
                }
                else if (filepath.ToLower() == "\\q")
                {
                    return;
                }
                else
                {
                    Console.WriteLine("The filepath you have entered is invalid, try again");
                }
            }
            
            string[] blocks = System.IO.File.ReadAllLines(filepath);
            foreach (string s in blocks)
            {
               lines.AddRange(s.Split('.'));
            }

            enterpriseload.Start();

            for (int i = 0; i < lines.Count; i++)
            {
                KIRKparse(lines[i]);
                timer++;
                location = i;
            }

        }

        static void enterpriseloading()
        {
            string[] enterprise = System.IO.File.ReadAllLines(@"enterpriseloading.txt");

            int dir = 1;
            int length = 0;
            int currentchar = enterprise[0].Length - 1;
            string spaces = "";

            while (location < lines.Count - 1)
            {
                if (timer % 10 == 0)
                {
                    Console.Clear();
                    Console.WriteLine(Math.Round((float)location / (float)lines.Count * 100) + "% of " + lines.Count + " lines processed");
                    for (int k = 0; k < enterprise.Length; k++)
                    {
                        if (length > 78)
                        {
                            dir = -1;
                        }

                        if (currentchar < 1)
                        {
                            currentchar++;
                            spaces += " ";
                        }

                        if (spaces.Length > 60)
                        {
                            length = 0;
                            currentchar = enterprise[0].Length - 1;
                            spaces = "";
                            dir = 1;
                        }

                        Console.WriteLine(spaces + enterprise[k].Substring(currentchar, length));
                    }
                    currentchar--;
                    length += dir;
                    timer = 0;
                    System.Threading.Thread.Sleep(5);
                }
            }
        }

        static void KIRKparse(string input)
        {
            int[] found = new int[2];
            if (input == "")
            {

            }
            else
            {
                string[] words = input.Split(' ');
                for (int i = 0; i < words.Length; i++)
                {
                    words[i] = RemoveSpecialCharacters(words[i]);
                }
                for (int j = 0; j < words.Length; j++)
                {
                    found[0] = int.MaxValue;
                    found[1] = int.MaxValue;

                    if (j < (words.Length - 1))
                    {
                        found = wordcatalogued(words[j], words[j + 1]);
                    }
                    else
                    {
                        found = wordcatalogued(words[j], "~");
                    }

                    if (found [0] < int.MaxValue && found[1] < int.MaxValue)
                    {
                        associatedWords[found[0]].occuranceCount[found[1]]++;
                        if (j == 0)
                        {
                            associatedWords[found[0]].Start = true;
                        }
                    }
                    else if (found[0] < int.MaxValue && j >= words.Length - 1)
                    {
                        associatedWords[found[0]].End = true;
                    }
                    else if (found[0]< int.MaxValue)
                    {
                        associatedWords[found[0]].postwords.Add(words[j + 1]);
                        associatedWords[found[0]].occuranceCount.Add(1);
                        associatedWords[found[0]].End = false;
                        if (j == 0)
                        {
                            associatedWords[found[0]].Start = true;
                        }
                        
                    }
                    else
                    {
                        Word word = new Word();
                        associatedWords.Add(word);
                        word.Keyword = words[j];
                        if (j == 0)
                        {
                            word.Start = true;
                        }
                        if (j < (words.Length - 1))
                        {
                            if (words[j + 1] != "")
                            {
                                word.postwords.Add(words[j + 1]);
                                word.occuranceCount.Add(1);
                            }
                        }
                        else
                        {
                            word.End = true;
                        }
                    }
                }
            }
        }

        static void KIRKtalk()
        {
            string input;
            string reply;
            string name = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            name = name.Substring(name.LastIndexOf('\\')+1);
            Console.WriteLine("\nEnter \\q to exit");
            Console.WriteLine("\nHello {0}", name);
            while (true)
            {

                input = Console.ReadLine();
                if (input.ToLower() == "\\q")
                {
                    return;
                }
                else if (input == " ")
                {
                    Console.WriteLine("Invalid input, try again");
                    continue;
                }

                KIRKparse(input);
                reply = KIRKreply(input);
                string[] write =reply.Split(' ').Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                reply = string.Join(" ", write);


                Console.WriteLine(reply + "\n");

                if (TOSstatus == true)
                {
                    synth.SpeakAsync(reply);
                }

            }
        }

        static string KIRKreply(string input)
        {
            int index = 0;
            int random = 0;
            string reply = "";
            string[] inputwords = input.Split(' ');

            for (int i = 0; i <inputwords.Length; i++)
            {
                for (int j = 0; j < associatedWords.Count; j++ )
                {
                    if (string.Compare(associatedWords[j].Keyword.ToLower(),inputwords[i].ToLower(), StringComparison.OrdinalIgnoreCase) == 0 && associatedWords[j].Start == true && associatedWords[j].End == false)
                    {
                        index = j;
                        reply += associatedWords[index].Keyword;
                        i = int.MaxValue-1;
                        break;
                    }
                }
            }

            if (reply == "")
            {
                for (int i = 0; i < associatedWords.Count; i++)
                {
                    index = rand.Next(0, associatedWords.Count-1);

                    if (associatedWords[index].Start == true && associatedWords[index].End == false)
                    {
                        reply += associatedWords[i].Keyword;
                        break;
                    }
                }
            }

            while ((reply.Length) < 100)
            {
                List<string> words = new List<string>();

                reply += ' ';

                if (associatedWords[index].postwords.Count < 1)
                {
                    return reply;
                }

                for (int i = 0; i < associatedWords[index].postwords.Count; i++)
                {
                    for (int j = 0; j < associatedWords[index].occuranceCount[i]; j++)
                    {
                        words.Add(associatedWords[index].postwords[i]);
                    }
                }
                index = rand.Next(0, words.Count);
                reply += words[index];

                for (int k = 0; k < associatedWords.Count; k++)
                {
                    if (string.Compare(associatedWords[k].Keyword, words[index], StringComparison.OrdinalIgnoreCase)==0 && associatedWords[k].End== true )
                    {
                        random = rand.Next(0,2);
                        if (random == 0)
                        {
                            return reply;
                        }
                    }
                    
                    if (String.Compare(associatedWords[k].Keyword, words[index], StringComparison.OrdinalIgnoreCase)==0)
                    {
                        index = k;
                        break;
                    }
                }
                words.Clear();
            }
            return reply;
        }

        static void loadBrain()
        {
            if (!File.Exists("kirkBrain.txt"))
            {
                File.Create("kirkBrain.txt");
            }
            else
            {
                string[] synapse = System.IO.File.ReadAllLines("kirkBrain.txt");
                string[] loadWord;
                foreach (string s in synapse)
                {
                    loadWord = s.Split(' ');
                    Word word = new Word();
                    associatedWords.Add(word);
                    word.Keyword = loadWord[0];
                    word.End =bool.Parse(loadWord[1]);
                    word.Start = bool.Parse(loadWord[2]);
                    int start = 0;
                    for (int i = 3; i < loadWord.Length; i++)
                    {
                        word.postwords.Add(loadWord[i].Substring(0, loadWord[i].IndexOf('(')));
                        start = loadWord[i].IndexOf('(') + 1;
                        word.occuranceCount.Add(int.Parse(loadWord[i].Substring(start,1)));

                    }
                }
            }
        }

        static void saveBrain()
        {

            StreamWriter file = new System.IO.StreamWriter("kirkBrain-temp.txt");
            for (int i = 0; i < associatedWords.Count; i++)
            {

                file.WriteLine(associatedWords[i].ToString());
            }
            file.Close();
            File.Delete("kirkBrain.txt");
            File.Move("kirkBrain-temp.txt", "kirkBrain.txt");
            Environment.Exit(0);

        }

        static void EraseBrain()
        {
            string answer = "";
            Console.WriteLine("Are you sure you want to erase kirk's brain,\nall previous conversation data will be gone forever!");
            answer = Console.ReadLine();
            if ( answer.ToLower() == "yes" || answer.ToLower() == "y")
            {
                File.Delete("kirkBrain.txt");
                associatedWords.Clear();
            }
        }

        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || ( c == '\''))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        static int[] wordcatalogued(string keyword, string postword)
        {
            for (int i = 0; i < associatedWords.Count; i++)
            {
                if (string.Compare(associatedWords[i].Keyword.ToLower(),keyword.ToLower(), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    for (int j = 0; j < associatedWords[i].postwords.Count; j++)
                    {
                        if (string.Compare(associatedWords[i].postwords[j].ToLower(),postword.ToLower(), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return new int[] { i, j };
                        }
                    }
                    return new int[] { i, int.MaxValue };
                }
            }
            return new int[] { int.MaxValue, int.MaxValue };
        }
    }
}
