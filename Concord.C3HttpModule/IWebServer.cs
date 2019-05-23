namespace Concord.C3HttpModule
{
    /// <summary>
    /// Public Interface for Web Server. Why? In case if in future the Module needs a factory implementation, user has an interface already available. 
    /// </summary>
    public interface IWebServer
    {
        /// <summary>
        /// default timeout applied to all URLs. If they are set with no timeout – that timeout applies instead.  Using timeout of ‘-1’ triggers usage of the configured default timeout.
        /// </summary>
        long VirtualURLLifeSpan { get; set; }
        /// <summary>
        /// Root directory of the module. 
        /// </summary>
        string RootDirectory { get; set; }
        /// <summary>
        /// Implement to set a port to the server.
        /// </summary>
        ushort PortNumber { get; set; }
        /// <summary>
        /// Implement to set server name/hostname.
        /// </summary>
        string ServerAddress { get; set; }
        /// <summary>
        /// Implement to allow browsing from underlying server, in either case should return an error code like 403 (forbidden)
        /// </summary>
        bool AllowBrowsing { get; set; }
        /// <summary>
        /// Implement the method for Adding Host and port. 
        /// </summary>
        /// <param name="host">Hostname</param>
        /// <param name="port">Port number</param>
        void AddHostAndPort(string host, ushort port);
        /// <summary>
        /// Interface method to add a Url to underlying webserver which has an expiry time and also has string, which should be served back to the user.
        /// </summary>
        /// <param name="url">Path to resource.</param>
        /// <param name="value">string value</param>
        /// <param name="seconds">Time out in seconds. </param>
        /// <returns>Should return false when error occurs.</returns>
        bool AddURLBufferWithExpiry(string url, string value, long seconds);
        /// <summary>
        /// Interface method to add a Url to underlying webserver which has expiry time and also has a file. The contents of the files shall be served back to the user.
        /// </summary>
        /// <param name="url">Path thru which the data shall be accessed</param>
        /// <param name="filepath">Path of resource</param>
        /// <param name="seconds">Time out in Seconds</param>
        /// <returns>Should return true when successfull. Else should return false </returns>
        bool AddURLWithExpiry(string url, string filepath, long seconds);
        /// <summary>
        /// Interface method to add a Url to underlying webserver which has no expiry time and also has a file. The contents of the files shall be served back to the user.
        /// </summary>
        /// <param name="url">Path to resource</param>
        /// <param name="filepath">File path as value</param>
        /// <returns>Should return true when successfull. Else should return false</returns>
        bool AddURL(string url, string filepath);
        /// <summary>
        /// Method shall be implemented to access the string, given as value. By accessing it as path given as url.
        /// </summary>
        /// <param name="url">Path to reource.</param>
        /// <param name="value">Value to return.</param>
        /// <returns>Should return true when successfull. Else should return false</returns>
        bool AddURLBuffer(string url, string value);
        /// <summary>
        /// Method shall be implemented to start the server.
        /// </summary>
        void Start();
        /// <summary>
        /// Method shall be implement to stop the server.
        /// </summary>
        void Stop();
        /// <summary>
        /// Method shall be implemented to renew the URLs expiry time. API shall 'renew' the expiry time, from current time to seconds.
        /// </summary>
        /// <param name="url">Path to resource</param>
        /// <param name="seconds">Timeout to extend in seconds.</param>
        void PokeUrlWithExpiry(string url, long seconds);
        /// <summary>
        /// Method shall be implemented to extend the expiry time of the Url by already present TimeOut time.
        /// </summary>
        /// <param name="url">Path to resource</param>
        void PokeUrl(string url);
    }
}

