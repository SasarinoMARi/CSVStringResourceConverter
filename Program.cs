using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSV2MobileResource
{
    class Program
    {
        const char SPLITTER_CSV = ',';
        const char SPLITTER_TSV = '\t';

        static List<StringResource> dic = new List<StringResource>();

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Program must be running with arguments!");
                return;
            }

            var splitter = (char)0;

            var saveDirBase = Path.Combine(Directory.GetCurrentDirectory(), DateTime.Now.ToString("yyMMdd_HHmm"));
            if (!Directory.Exists(saveDirBase)) Directory.CreateDirectory(saveDirBase);

            for (int i = 0; i < args.Length; i++)
            {
                var path = args[i];
                if (!File.Exists(path)) continue;

                if (path.EndsWith(".csv")) splitter = SPLITTER_CSV;
                else if (path.EndsWith(".tsv")) splitter = SPLITTER_TSV;
                else
                {
                    Console.WriteLine("Invalid file format!");
                    continue;
                }

                var saveDir = Path.Combine(saveDirBase, Path.GetFileName(path));
                if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);

                var lines = getLinesFromFile(path);
                var result = parseLines(lines, splitter);
                dic.AddRange(result.Item2);

                saveAndroid(saveDir, result.Item1, result.Item2);
                saveIOS(saveDir, result.Item1, result.Item2);
            }
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
                    default:
                        if (item.StartsWith("value"))
                        {
                            pi.values.Add(new Tuple<int, string>(i, item));
                        }
                        if (item.StartsWith("uses-in"))
                        {
                            pi.uses_in.Add(new Tuple<int, string>(i, item));
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
                foreach (var j in pi.values)
                {
                    o.values.Add(new KeyValuePair<int, string>(j.Item1, item[j.Item1]));
                }
                foreach (var j in pi.uses_in)
                {
                    o.uses_in.Add(new KeyValuePair<int, bool>(j.Item1, item[j.Item1].ToUpper() == "TRUE"));
                }
                result.Add(o);
            }

            return new Tuple<ParserColumnIndex, List<StringResource>>(pi, result);
        }

        static StringResource include(StringResource obj)
        {
            var fv = obj.values[0].Value;
            if (fv.StartsWith("@"))
            {
                var id = fv.Substring(1);
                return dic.Find(x => x.id == id);
            }
            return obj;
        }

        static void saveIOS(string dir, ParserColumnIndex indexes, List<StringResource> strings)
        {
            var root = Path.Combine(dir, "iOS");
            if (!Directory.Exists(root)) Directory.CreateDirectory(root);

            // 언어 수만큼 반복
            foreach (var value in indexes.values)
            {
                var country = value.Item2 + ".strings";
                var filePath = Path.Combine(root, country);
                var output = new StringBuilder();
                var index = 0;
                // 패키지 수만큼 반복
                foreach (var package in indexes.uses_in)
                {
                    foreach (var s in strings)
                    {
                        if (s.id == null)
                        {
                            Console.WriteLine("정의되지 않은 ID : {0}", s);
                            continue;
                        }

                        // id 참조 처리
                        var cpy = include(s);
                        if (cpy == null)
                        {
                            Console.WriteLine("참조에 실패했습니다. : {0}", s.id);
                            throw new Exception();
                        }

                        if (!s.uses_in.Find(x => x.Key == package.Item1).Value) continue;

                        var localizedString = cpy.values.Find(x => x.Key == value.Item1).Value;
                        // 현재 언어에서 해당 id를 가진 문자열 리소스가 없을 경우 첫 번째 언어의 문자열을 대입
                        if (string.IsNullOrEmpty(localizedString)) localizedString = cpy.values.Find(x => x.Key == indexes.values[0].Item1).Value;

                        #region escape
                        for (int f = 0; ; f++)
                        {
                            var format = "{" + f + "}";
                            if (localizedString.Contains(format))
                                localizedString = localizedString.Replace(format, $"%{f + 1}$@");
                            else break;
                        }
                        #endregion
                        var id = s.id.Replace("/", "_");
                        var dist = package.Item2.Split('-').Distinct().ToArray();
                        if (index > 0) id += '_' + dist[dist.Count() - 1];
                        output.Append(string.Format("\"{0}\" = \"{1}\";\n", id, localizedString));
                    }
                    index++;
                }
                var result = output.ToString();
                File.WriteAllText(filePath, result);
            }
        }


        static void saveAndroid(string dir, ParserColumnIndex indexes, List<StringResource> strings)
        {
            var root = Path.Combine(dir, "Android");
            if (!Directory.Exists(root)) Directory.CreateDirectory(root);

            // 패키지 수만큼 반복
            foreach (var package in indexes.uses_in)
            {
                var path = Path.Combine(root, package.Item2);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                // 언어 수만큼 반복
                for (int i = 0; i < indexes.values.Count; i++)
                {
                    var value = indexes.values[i];
                    var country = value.Item2 + ".xml";
                    var filePath = Path.Combine(path, country);
                    var output = new StringBuilder();
                    output.Append("<resources>\n");
                    foreach (var s in strings)
                    {
                        if (s.id == null)
                        {
                            Console.WriteLine("정의되지 않은 ID : {0}", s);
                            continue;
                        }

                        // id 참조 처리
                        var cpy = include(s);
                        if (cpy == null)
                        {
                            Console.WriteLine("참조에 실패했습니다. : {0}", s.id);
                            throw new Exception();
                        }

                        if (!s.uses_in.Find(x => x.Key == package.Item1).Value) continue;

                        var localizedString = cpy.values.Find(x => x.Key == value.Item1).Value;
                        // 현재 언어에서 해당 id를 가진 문자열 리소스가 없을 경우 첫 번째 언어의 문자열을 대입
                        if (string.IsNullOrEmpty(localizedString)) localizedString = cpy.values.Find(x => x.Key == indexes.values[0].Item1).Value;

                        #region escape
                        for (int f = 0; ; f++)
                        {
                            var format = "{" + f + "}";
                            if (localizedString.Contains(format))
                                localizedString = localizedString.Replace(format, $"%{f + 1}$s");
                            else break;
                        }

                        localizedString = localizedString.Replace("&", "&amp;");
                        #endregion

                        var translatable = string.Empty;
                        if (i == 0 && !cpy.translatable) translatable = " translatable=\"false\"";
                        if ((i == 0 && !cpy.translatable) || cpy.translatable) output.Append(string.Format("\t<string name='{0}'{1}>{2}</string>\n", s.id.Replace("/", "_"), translatable, localizedString));
                    }
                    output.Append("</resources>");
                    var result = output.ToString();
                    File.WriteAllText(filePath, result);
                }


            }


        }
    }

}
