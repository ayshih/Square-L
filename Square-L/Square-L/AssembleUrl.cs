using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Square_L
{
    class AssembleUrl
    {
        /// <summary>
        /// Creates an AssembleUrl object based on a SQRL url
        /// </summary>
        /// <param name="url">URL</param>
        public AssembleUrl(string url)
        {
            char[] tokens = { '/', '?', '&' };
            var items = url.Split(tokens);

            Protocol = items[0];
            DomainName = items[2];

            foreach (var item in items)
            {
                if (item.StartsWith("d="))
                {
                    DomainName = url.Substring(url.IndexOf(DomainName), DomainName.Length + Convert.ToUInt16(item.Substring(2)));
                }
            }

            Buffer = url.Substring(Protocol.Length + 2);

            //If there is no '?' at all, append one
            if (!url.Contains('?')) Buffer += '?';
        }

        /// <summary>
        /// Adds a URL parameter
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="value">parameter value</param>
        public void AddParameter(string name, string value)
        {
            var stub = name + "=" + value;
            if(Buffer[Buffer.Length - 1] != '?') stub = "&" + stub;
            Buffer += stub;
        }

        /// <summary>
        /// Returns wether the URL specifies a SQRL protocol
        /// </summary>
        /// <returns></returns>
        public bool ValidSQRL()
        {
            return Protocol.Contains("qrl");
        }

        /// <summary>
        /// Ought to be "sqrl:" or "qrl:"
        /// </summary>
        public string Protocol { get; private set; }

        /// <summary>
        /// The domain name used generating the public/private keypair
        /// </summary>
        public string DomainName { get; private set; }

        /// <summary>
        /// The assembly buffer for constructing the SQRL login query
        /// </summary>
        public string Buffer { get; private set; }
    }
}
