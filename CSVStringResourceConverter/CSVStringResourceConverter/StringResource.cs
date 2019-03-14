using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVStringResourceConverter
{
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
