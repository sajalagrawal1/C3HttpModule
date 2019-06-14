using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.C3HttpModule
{
    /// <summary>
    /// This is a replacement to Dot Net class Uri Builder. UriBuilder was not accepting the * as HostName
    /// </summary>
    internal class HttpUriBuilder
    {
        public ushort Port { get; set; } = 80;
        public string Host { get; set; } = Constants.ALL_INTERFACES;
        public string Scheme { get; set; } = Constants.HTTP_SCHEME;

        public string UriString => string.Format("{0}://{1}{2}{3}/", Scheme, Host, Port == 0 ? string.Empty : ":", Port == 0 ? string.Empty : Convert.ToString(Port));
    }
}
