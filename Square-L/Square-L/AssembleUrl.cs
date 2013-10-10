using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Square_L
{
    class AssembleUrl
    {
        private string _protocol;
        private string _domainName;
        private string _buffer;

        public AssembleUrl(string url)
        {
            char[] tokens = { '/', '?', '&' };
            var items = url.Split(tokens);

            _protocol = items[0];
            _domainName = items[2];

            for (var i = 0; i < items.Length; i++)
            {
                if (items[i].StartsWith("d="))
                {
                    _domainName = url.Substring(url.IndexOf(_domainName), _domainName.Length + Convert.ToUInt16(items[i].Substring(2)));
                }
            }

            _buffer = url.Substring(_protocol.Length + 2);

            //If there is no '?' at all
            if (!url.Contains('?')) _buffer += '?';
        }

        public void AddParameter(string name, string value)
        {
            var stub = name + "=" + value;
            if(_buffer[_buffer.Length - 1] != '?') stub = "&" + stub;
            _buffer += stub;
        }

        public bool ValidSQRL()
        {
            return _protocol.Contains("qrl");
        }

        public string Protocol
        {
            get
            {
                return _protocol;
            }
        }
        public string DomainName
        {
            get
            {
                return _domainName;
            }
        }
        public string Buffer
        {
            get
            {
                return _buffer;
            }
        }
    }
}
