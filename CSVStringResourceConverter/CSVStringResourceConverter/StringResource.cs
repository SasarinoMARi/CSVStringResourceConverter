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
            this.screen = screen.Trim();
            this.key = key.Trim();
        }

        private string screen = null;
        private string key = null;

        public bool translatable { get; set; } = false;
        public List<KeyValuePair<int, bool>> uses_in { get; set; } = new List<KeyValuePair<int, bool>>();
        public List<KeyValuePair<int, string>> values { get; set; } = new List<KeyValuePair<int, string>>();

        public string id { get { return (!string.IsNullOrEmpty(screen) && !string.IsNullOrEmpty(key)) ? screen + "/" + key : null; } }

        public override string ToString()
        {
            return "key = " + id + "\tvalue1 : " + values[0] + "\tvalue2 : " + values[1];
        }
    }
}
