using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSV2MobileResource
{
    class ParserColumnIndex
    {
        public int screen = -1;
        public int key = -1;
        public int translatable = -1;
        public List<Tuple<int, string>> values { get; private set; } = new List<Tuple<int, string>>();
        public List<Tuple<int, string>> uses_in { get; private set; } = new List<Tuple<int, string>>();
    }
}
