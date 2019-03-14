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

            var lines = getLinesFromFile(path);
            var result = parseLines(lines, splitter);
            var saveDir = Path.Combine(Directory.GetCurrentDirectory(), DateTime.Now.ToString("yyMMdd_hhmm"));
            if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);
            saveIOS(saveDir, result.Item1, result.Item2);
            saveAndroid(saveDir, result.Item1, result.Item2);
        }

        static string[] getLinesFromFile(string path)
        {
            var lines = File.ReadAllLines(path);
            return lines;
        }

        static Tuple<ParserColumnIndex, List<StringResource>> parseLines(string[] lines, char splitter)
        {
            // get indexes
            var indexes = lines[0].Split(splitter);
            var pi = new ParserColumnIndex();
            for (int i = 0; i < indexes.Length; i++)
            {
                var item = indexes[i];
                switch (item)
                {
                    case "screen":
                        pi.screen = i;
                        break;
                    case "key":
                        pi.key = i;
                        break;
                    case "translatable":
                        pi.translatable = i;
                        break;
                    case "uses-in-android":
                        pi.usesAndroid = i;
                        break;
                    case "uses-in-ios":
                        pi.usesIos = i;
                        break;
                    default:
                        if (item.StartsWith("value"))
                        {
                            pi.values.Add(new Tuple<int, string>(i, item));
                        }
                        break;
                }
            }

            // parsing.
            var result = new List<StringResource>();
            for (int i = 1; i < lines.Length; i++)
            {
                var item = lines[i].Split(splitter);
                var o = new StringResource(item[pi.screen], item[pi.key]);
                o.translatable = item[pi.translatable].ToUpper() == "TRUE";
                o.android = item[pi.usesAndroid].ToUpper() == "TRUE";
                o.ios = item[pi.usesIos].ToUpper() == "TRUE";
                foreach (var j in pi.values)
                {
                    o.values.Add(new KeyValuePair<int, string>(j.Item1, item[j.Item1]));
                }
                result.Add(o);
            }

            return new Tuple<ParserColumnIndex, List<StringResource>>(pi, result);
        }

        static void saveIOS(string dir, ParserColumnIndex indexes, List<StringResource> strings)
        {
            var path = Path.Combine(dir, "iOS");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            foreach (var value in indexes.values)
            {
                var country = value.Item2 + ".strings";
                var filePath = Path.Combine(path, country);
                var output = new StringBuilder();
                foreach (var sr in strings)
                {
                    if (!sr.ios) continue;
                    var localizedString = sr.values.Find(x => x.Key == value.Item1).Value;
                    // 현재 언어에서 해당 id를 가진 문자열 리소스가 없을 경우 첫 번째 언어의 문자열을 대입
                    if (string.IsNullOrEmpty(localizedString)) localizedString = sr.values.Find(x => x.Key == indexes.values[0].Item1).Value;

                    output.Append(string.Format("\"{0}\" = \"{1}\";\n", sr.id, localizedString));
                }
                var result = output.ToString();
                File.WriteAllText(filePath, result);
            }
        }
        static void saveAndroid(string dir, ParserColumnIndex indexes, List<StringResource> strings)
        {
            var path = Path.Combine(dir, "Android");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);


            for (int i = 0; i < indexes.values.Count; i++)
            {
                var value = indexes.values[i];
                var country = value.Item2 + ".xml";
                var filePath = Path.Combine(path, country);
                var output = new StringBuilder();
                output.Append("<resources>\n");
                foreach (var sr in strings)
                {
                    if (!sr.android) continue;
                    var localizedString = sr.values.Find(x => x.Key == value.Item1).Value;
                    // 현재 언어에서 해당 id를 가진 문자열 리소스가 없을 경우 첫 번째 언어의 문자열을 대입
                    if (string.IsNullOrEmpty(localizedString)) localizedString = sr.values.Find(x => x.Key == indexes.values[0].Item1).Value;

                    var translatable = string.Empty;
                    if (i == 0 && !sr.translatable) translatable = " translatable=\"false\"";
                    if ((i == 0 && !sr.translatable) || sr.translatable) output.Append(string.Format("\t<string name='{0}'{1}>{2}</string>\n", sr.id, translatable, localizedString));
                }
                output.Append("</resources>");
                var result = output.ToString();
                File.WriteAllText(filePath, result);
            }
        }
    }

}
