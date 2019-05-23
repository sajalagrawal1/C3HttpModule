using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.IO;
using log4net;

namespace Concord.C3HttpModule
{
    /// <summary>
    /// Singleton Thread-safe class. 
    /// </summary>
    public class WebServer : IWebServer
    {
        /// <summary>
        /// Virtual URL Life span local holder.
        /// </summary>
        private long _virualLifeSpan = -1;
        /// <summary>
        /// HttpListener object, the backbone of the application!!!
        /// </summary>
        private readonly HttpListener _listener = new HttpListener();
        /// <summary>
        /// The Method which responds to the HttpListenerContext as the data, which need to be sent.
        /// </summary>
        private readonly Func<HttpListenerContext, string> _responderMethod;
        /// <summary>
        /// The list of Url and HttpResponseBuffer, basically it is a wrapper over ConcurrentDictionary.
        /// </summary>
        private HttpUrlList _httpUrlList = new HttpUrlList();   
        /// <summary>
        /// Set a scheme of the listener, defaults to http
        /// </summary>
        private string _scheme = Constants.HTTP_SCHEME;
        /// <summary>
        /// Sets a port to the listener, defaults to 8080 
        /// </summary>
        private ushort _port = Constants.DEFAULT_PORT;
        /// <summary>
        /// Sets host name for the listener, defaults to *. * will assign all the interfaces to the listener.
        /// </summary>
        private string _server = Constants.ALL_INTERFACES;
        /// <summary>
        /// Root Path for the app to store file.
        /// </summary>
        private string _root = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        /// <summary>
        /// NLOG logger
        /// </summary>
        private static ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Default timeout applied to all URLs. If they are set with no timeout – that timeout applies instead.  Using timeout of ‘-1’ triggers usage of the configured default timeout.
        /// </summary>
        public long VirtualURLLifeSpan
        {   get
            {
                return _virualLifeSpan;
            }
            set
            {
                GlobalConfig.VirtualURLLifeSpan = value;
                _virualLifeSpan = value;
                this.UpdateAllLifeSpans();
            }
        }

        private void UpdateAllLifeSpans()
        {
            _httpUrlList.UpdateTimeOutOfAllUrls();
        }

        /// <summary>
        /// Root directory of the module. 
        /// </summary>
        public string RootDirectory
        {
            get
            {
                return _root;
            }
            set
            {
                if(System.IO.Directory.Exists(value)==false)
                {
                    try
                    {
                        string path = value;
                        System.IO.Directory.CreateDirectory(value);
                        GlobalConfig.RootPath = value;
                        _root = value;
                    }
                    catch(IOException ioex)
                    {
                        _logger.Error(Utilities.ReadableException(ioex));
                    }
                }
            }
        }
        /// <summary>
        /// Builds a Uri from the given properties
        /// </summary>
        private HttpUriBuilder _uri => new HttpUriBuilder() { Scheme = _scheme, Host = _server, Port = _port };

        /// <summary>
        /// More Prefixes (url hosts) can be assigned to the listener. 
        /// </summary>
        List<string> _prefixes = new List<string>();


        /// <summary>
        /// Flag to Allow Browsing, if it is set to false, all the calls should return error 403.
        /// </summary>
        public bool AllowBrowsing { get; set; }
        /// <summary>
        /// Sets a port to the listener, defaults to 8080 
        /// </summary>
        public ushort PortNumber
        {
            get
            {
                return _port;
            }
            set
            {
                _uri.Port = value;
                _port = value;
            }
        }
        /// <summary>
        /// Property for server name/hostname. Defaults to *. * will assign all the interfaces to the listener.
        /// </summary>
        public string ServerAddress
        {
            get
            {
                return _server;
            }
            set
            {
                _uri.Host = value;
                _server = value;
            }
        }
        /// <summary>
        /// Scheme of the listener, defaults to http.
        /// </summary>
        public string Scheme
        {
            get
            {
                return _scheme;
            }
            set
            {
                _uri.Scheme = value;
                _scheme = value;
            }
        }

        /// <summary>
        /// Private Constructor.
        /// </summary>
        public WebServer() 
        {
            
            RootDirectory = _root;
            VirtualURLLifeSpan = _virualLifeSpan;
            GlobalConfig.RootPath = RootDirectory;
            GlobalConfig.VirtualURLLifeSpan = VirtualURLLifeSpan;
            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException(Constants.UNSUPPORTED_SYSTEM);
            }
            _responderMethod = SendResponse;
        }


        /// <summary>
        /// Method to add Host and Port to the listener. The server need to be Stop() ed and Start() again to reflect the changes.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="scheme"></param>
        public void AddHostAndPort(string host, ushort port)
        {
            HttpUriBuilder uri = new HttpUriBuilder();
            uri.Host = host;
            uri.Port = port;
            uri.Scheme = Constants.HTTP_SCHEME;
            _prefixes.Add(uri.UriString);
        }

        /// <summary>
        /// This method must be called in order to make server in a listening mode. 
        /// </summary>
        private void Run()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                _logger.Info(Constants.LOG_STARTED_WEBSERVER);
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem(c =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                if (ctx == null)
                                {
                                    return;
                                }
                                if (AllowBrowsing == true)
                                {
                                    var rstr = _responderMethod(ctx);
                                    var buf = Encoding.UTF8.GetBytes(rstr);
                                    ctx.Response.ContentLength64 = buf.Length;
                                    ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                                }
                                else
                                {
                                    ctx.Response.StatusCode = 403;
                                }

                            }
                            catch (Exception exception)
                            {
                                _logger.Error(Utilities.ReadableException(exception));
                            }
                            finally
                            {
                                if (ctx != null)
                                {
                                    ctx.Response.OutputStream.Close();
                                }
                            }
                        }, _listener.GetContext());
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.ReadableException(ex));
                }
            });
        }
        /// <summary>
        /// The WebServer should Start() first and then Run(). 
        /// </summary>
        public void Start()
        {
            
            string defaultprefix = _uri.UriString;
            _prefixes.Add(defaultprefix);

            if (_prefixes == null || _prefixes.Count == 0)
            {
                throw new ArgumentException(Constants.ERROR_NEED_PREFIXES);
            }

            foreach (var s in _prefixes)
            {
                _listener.Prefixes.Add(s);
            }

            _logger.Info( Constants.STARTING_SERVER+ string.Join( ",", _prefixes.ToArray())); 
            _listener.Start();

            this.Run();
        }
        /// <summary>
        /// Method to stop Web Server. 
        /// </summary>
        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
        /// <summary>
        /// Method to add Url with expiry. e.g. AddUrlWithExpiry("test.html", "C:\\Temp.html", 0) Will add a url with no expiry. and can be accessed as http://<url>:<port>/test.html
        /// </summary>
        /// <param name="url">Url string to be assigned</param>
        /// <param name="filepath">File path of the file to be read and served back.</param>
        /// <param name="seconds">Number of seconds for expiry. 0 to ignore expiry.</param>
        public bool AddURLWithExpiry(string url, string filepath, long seconds)
        {
            return _httpUrlList.AddUrl(Utilities.CleanUnixUrl( url ),
                                new HttpResponseBuffer(HttpResponseBuffer.BufferType.FILE, filepath, seconds, url));
        }
        /// <summary>
        /// Method to add Url with expiry. e.g. AddUrlWithExpiry("test.html", "This is a response string.", 0) Will add a url with no expiry. and can be accessed as http://<url>:<port>/test.html
        /// </summary>
        /// <param name="url">Url string to be assigned</param>
        /// <param name="value">File path of the file to be read and served back.</param>
        /// <param name="seconds">Number of seconds for expiry. 0 to ignore expiry.</param>
        public bool AddURLBufferWithExpiry(string url, string value, long seconds)
        {
            return _httpUrlList.AddUrl(Utilities.CleanUnixUrl( url ) ,
                                new HttpResponseBuffer(HttpResponseBuffer.BufferType.BUFFER, value, seconds, url));
        }
        /// <summary>
        /// Method to add Url with expiry. e.g. AddUrlWithExpiry("test.html", "C:\\Temp.html", 0) Will add a url with no expiry. and can be accessed as http://<url>:<port>/test.html
        /// </summary>
        /// <param name="url">Url string to be assigned</param>
        /// <param name="filepath">File path of the file to be read and served back.</param>
        /// <param name="seconds">Number of seconds for expiry. 0 to ignore expiry.</param>
        public bool AddURL(string url, string filepath)
        {
            return _httpUrlList.AddUrl(Utilities.CleanUnixUrl(url),
                                new HttpResponseBuffer(HttpResponseBuffer.BufferType.FILE, filepath, 0, url));
        }
        /// <summary>
        /// Method to add Url with expiry. e.g. AddUrlWithExpiry("test.html", "This is a response string.", 0) Will add a url with no expiry. and can be accessed as http://<url>:<port>/test.html
        /// </summary>
        /// <param name="url">Url string to be assigned</param>
        /// <param name="value">File path of the file to be read and served back.</param>
        /// <param name="seconds">Number of seconds for expiry. 0 to ignore expiry.</param>
        public bool AddURLBuffer(string url, string value)
        {
            return _httpUrlList.AddUrl(Utilities.CleanUnixUrl( url ),
                                new HttpResponseBuffer(HttpResponseBuffer.BufferType.BUFFER, value, 0, url));
        }
		
		/// <summary>
        /// Updates the Expiry of a url and updates TimeOut of the HttpResponseBuffer.
        /// </summary>
        /// <param name="url">Url for which expiry times need to updated.</param>
        /// <param name="seconds">Time in seconds.</param>
        public void PokeUrlWithExpiry(string url, long seconds)
        {
            url = Utilities.CleanUnixUrl(url);
            HttpResponseBuffer buffer = _httpUrlList.GetUrl(url);
            if (buffer != null)
            {
                buffer.ExtendExpiryTime(seconds);
            }
            else
            {
                _logger.Error(string.Format(Constants.LOG_RESOURCENOTFOUND, url));
            }
        }

        /// <summary>
        /// Updates the Expiry of a url, by already given TimeOut parameter.
        /// </summary>
        /// <param name="url">Url for which expiry times need to updated.</param>
        /// <param name="seconds">Time in seconds.</param>
        public void PokeUrl(string url)
        {
            url = Utilities.CleanUnixUrl(url);
            HttpResponseBuffer buffer = _httpUrlList.GetUrl(url);
            if (buffer != null)
            {
                buffer.ExtendExpiryTime();
            }
            else
            {
                _logger.Error(string.Format(Constants.LOG_RESOURCENOTFOUND, url));
            }
        }
		
        /// <summary>
        /// Send Response is the method registered in HttpResponseContext to get the response, which needs to be sent on wire, eventually.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private string SendResponse(HttpListenerContext ctx)
        {
            string retVal = string.Empty;
            if (ctx.Request.HttpMethod == Constants.HTTP_GET)
            {
                string cleanUrl = Utilities.CleanUnixUrl(ctx.Request.RawUrl);
                string windowsPath = Utilities.UnixToWindowsPath(cleanUrl);
                string fileSystemPath = System.IO.Path.Combine(GlobalConfig.RootPath, windowsPath );

                if (Directory.Exists(fileSystemPath))
                {
                    retVal = GetDirectoryListingAsHtml(fileSystemPath);
                }
                if (File.Exists( fileSystemPath ))
                {
                    retVal = GetResponseForUrl(Utilities.CleanUnixUrl(ctx.Request.RawUrl), fileSystemPath, ctx);
                }
                
            }
            return retVal;
        }

        /// <summary>
        /// Gets directory listing as HTML format. 
        /// </summary>
        /// <param name="directoryPath">Path for which directory listing has to be created.</param>
        /// <returns>A html formatted string</returns>
        private string GetDirectoryListingAsHtml(string directoryPath)
        {
            DirectoryInfo d = new DirectoryInfo(directoryPath);
            FileInfo[] files = d.GetFiles();
            DirectoryInfo[] directories = d.GetDirectories();

            string directoriesHtml = string.Empty;
            foreach( DirectoryInfo di in directories )
            {
                directoriesHtml += string.Format(Constants.DIRECTORY_LISTING_ITEMS, 
                                                di.LastWriteTime.ToShortDateString() , 
                                                di.LastWriteTime.ToShortTimeString(), 
                                                Constants.ITEM_FOLDER, "./"+di.Name+"/" , di.Name)+ "\r\n";
            }
            string filesHtml = string.Empty;
            foreach (FileInfo fi in files)
            {
                directoriesHtml += string.Format(Constants.DIRECTORY_LISTING_ITEMS, 
                                                fi.LastWriteTime.ToShortDateString(), 
                                                fi.LastWriteTime.ToShortTimeString(), 
                                                Constants.ITEM_FILE, "./"+fi.Name, fi.Name)+ "\r\n";
            }

            string stringToReturn = string.Empty;
            stringToReturn += string.Format(Constants.DIRECTORY_LISTING_HEADER, ServerAddress);
            stringToReturn += filesHtml + "\r\n" + directoriesHtml;
            stringToReturn += Constants.DIRECTORY_LISTING_FOOTER;

            return stringToReturn;


        }

        /// <summary>
        /// Gets the response in string from HttpResponseBuffer.
        /// </summary>
        /// <param name="url">Url for which response is needed</param>
        /// <returns></returns>
        private string GetResponseForUrl(string url, string filePath , HttpListenerContext ctx)
        {
            string retVal = string.Empty;
            HttpResponseBuffer responseBuffer = null;
            responseBuffer = _httpUrlList.GetUrl(url);
            if (responseBuffer != null)
            {
                if ((DateTime.UtcNow.Ticks > responseBuffer.ExpiryTime) && (responseBuffer.IgnoreExpiryTime == false))
                {
                    ctx.Response.StatusCode = 400;
                    _logger.Error(string.Format(Constants.URL_TIMEDOUT, url));
                }
                else
                {
                    retVal = responseBuffer.GetData();
                }
            }
            else
            {
                ctx.Response.StatusCode = 403;
                _logger.Error(string.Format(Constants.LOG_RESOURCENOTFOUND, url));
            }
            return retVal;
        }

    }
}
