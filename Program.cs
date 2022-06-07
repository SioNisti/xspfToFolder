using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
class Track
{
    public string location { get; set; }
    public string title { get; set; }
    public string creator { get; set; }
    public int duration { get; set; }
}

namespace xspfToFolder
{
    class Program
    {
        public static List<Track> tracks;
        public static string path = "";
        public static string json = "";
        public static string name = "";

        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;

            path = Path.GetDirectoryName(args[0]) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(args[0]) + Path.GetExtension(args[0]);
            name = Path.GetFileName(path);
            //Console.WriteLine(path);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(File.ReadAllText(path));

            json = JsonConvert.SerializeXmlNode(doc);
            json = json.Substring(json.IndexOf('['));
            json = json.Replace("file:///", "");
            json = json.Substring(0, json.LastIndexOf("},\"extension\":"));

            tracks = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Track>>(json);

            Console.WriteLine($"Playlist accepted.\nHit any key to start");
            Console.ReadKey();

            int seconds = 0;
            int repeat = 0;
            int repeat2 = 0;
            int tmpSeconds = 0;
            bool multi = false;

            foreach (Track t in tracks)
            {
                seconds += t.duration;

                repeat += t.duration;
                if (repeat / 1000 > 4740)
                {
                    repeat = 0;
                    repeat2++;
                }
            }

            Directory.CreateDirectory(name.Remove(name.Length - 5, 5));

            if (repeat2 != 0)
            {
                for (var ia = 1; ia <= repeat2 + 1; ia++)
                {
                    Directory.CreateDirectory($"{name.Remove(name.Length - 5, 5)}/CD{ia}");
                    multi = true;
                }
            }

            Random rng = new Random();
            var tracksR = tracks.OrderBy(_ => rng.Next()).ToList();

            int i = 1;
            repeat = 0;
            repeat2 = 1;
            foreach (Track t2 in tracksR)
            {
                var loca = Uri.UnescapeDataString(t2.location);
                tmpSeconds += t2.duration;
                string[] trackName = loca.Split('/');
                Console.WriteLine($"{t2.creator} - {t2.title}");
                Console.WriteLine($"{tmpSeconds}/{seconds} ({i / 2 + 1}/{tracksR.Count})");
                if (multi)
                {
                    repeat += t2.duration;
                    if (repeat / 1000 > 4740)
                    {
                        //Console.ForegroundColor = ConsoleColor.Cyan;
                        //Console.WriteLine($"{repeat/1000} {4740}");
                        //Console.ForegroundColor = ConsoleColor.White;
                        repeat = 0;
                        repeat2++;
                    }

                    System.IO.File.Copy(loca, $"{name.Remove(name.Length - 5, 5)}/CD{repeat2}/{Path.GetFileName(loca)}", true);
                }
                else
                {
                    System.IO.File.Copy(loca, name.Remove(name.Length - 5, 5) + "/" + Path.GetFileName(loca), true);
                }
                i += 2;
                Console.SetWindowPosition(0, i + 1);
            }

            Console.SetWindowPosition(0, i + 1);
            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}
