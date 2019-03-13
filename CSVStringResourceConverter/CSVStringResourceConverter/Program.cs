using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVStringResourceConverter
{
    class Program
    {
        const char SPLITTER_CSV = ',';
        const char SPLITTER_TSV = '\t';

        static void Main(string[] args)
        {
            stuff(new string[] { "C:/Users/USER/Desktop/strings.tsv" });
            Console.ReadLine();
        }

        static void stuff(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Program must be running with arguments!");
                return;
            }
            var path = args[0];
            var splitter = (char)0;
            if (path.EndsWith(".csv")) splitter = SPLITTER_CSV;
            else if (path.EndsWith(".tsv")) splitter = SPLITTER_TSV;
            else
            {
                Console.WriteLine("Invalid file format!");
                return;
            }

            var lines = File.ReadAllLines(path);

            // get indexes
            var indexes = lines[0].Split(splitter);
            int indexOfScreen, indexOfKey, indexOfTranslatable, indexOfAndroid, indexOfIos;
            indexOfScreen = indexOfKey = indexOfTranslatable = indexOfAndroid = indexOfIos = -1;
            var indexOfValues = new List<int>();
            for (int i = 0; i < indexes.Length; i++)
            {
                var item = indexes[i];
                switch (item)
                {
                    case "screen":
                        indexOfScreen = i;
                        break;
                    case "key":
                        indexOfKey = i;
                        break;
                    case "translatable":
                        indexOfTranslatable = i;
                        break;
                    case "uses-in-android":
                        indexOfAndroid = i;
                        break;
                    case "uses-in-ios":
                        indexOfIos = i;
                        break;
                    default:
                        if (item.StartsWith("value"))
                        {
                            indexOfValues.Add(i);
                        }
                        break;
                }
            }

            // parsing.
            var result = new List<StringResource>();
            for (int i = 1; i < lines.Length; i++)
            {
                var item = lines[i].Split(splitter);
                var o = new StringResource(item[indexOfScreen], item[indexOfKey]);
                o.translatable = item[indexOfTranslatable].ToUpper() == "TRUE";
                o.android = item[indexOfAndroid].ToUpper() == "TRUE";
                o.ios = item[indexOfIos].ToUpper() == "TRUE";
                foreach (var j in indexOfValues)
                {
                    o.values.Add(new KeyValuePair<int, string>(j, item[j]));
                }
                result.Add(o);
            }

            // export
            foreach (var item in result)
            {
                Console.WriteLine(item.ToString());
            }
           
        }
    }

    class StringResource
    {
        public StringResource(string screen, string key)
        {
            this.screen = screen;
            this.key = key;
        }

        private string screen = null;
        private string key = null;

        public bool translatable { get; set; } = false;
        public bool android { get; set; } = false;
        public bool ios { get; set; } = false;
        public List<KeyValuePair<int, string>> values { get; set; } = new List<KeyValuePair<int, string>>();

        public string id { get { return screen + "_" + key; } }

        public override string ToString()
        {
            return "key = " + id + "\tvalue1 : " + values[0] + "\tvalue2 : " + values[1];
        }
    }

}
