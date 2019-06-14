using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.C3HttpModule
{
    /// <summary>
    /// Class contains various constant resources used across the module.
    /// </summary>
    internal static class Constants
    {
        public const string NOTAVAILABLE = "The Resource is not Available.";
        public const string TIMEDOUT = "The resource has timed out.";
        public const string URL_TIMEDOUT = "The resource {0} has timed out.";
        public const string LOG_RESOURCENOTFOUND = "Could not find the resource {0} in pool.";
        public const string LOG_RESOURCEFOUND = "Retrieved the resource {0} from pool.";
        public const string LOG_RESOURCEREMOVED = "Removed the resource {0} from pool.";
        public const string LOG_RESOURCENOTREMOVED = "Unable to remove the resource {0} from pool.";
        public const string LOG_RESOURCEADDED = "Added resource {0} to pool.";
        public const string LOG_RESOURCEUNABLETOADD = "Unable to add resource {0} to pool.";
        public const string LOG_STARTED_WEBSERVER = "Started Web server";
        public const string HTTP_SCHEME = "http";
        public const string ALL_INTERFACES = "*";
        public const ushort DEFAULT_PORT = 8080;
        public const string ERROR_NEED_PREFIXES = "URI prefixes are required";
        public const string HTTP_GET = "GET";
        public const string EXCEPTION_FORMAT_STRING = "Message : {0} \r\n" + "Stack :{1}\r\n";
        public const string ROOT_FOLDER_DEFAULT = "C3HttpModule";
        public const string STARTING_SERVER = "Starting server with prefixes : ";
        public const string UNSUPPORTED_SYSTEM = "Needs Windows XP SP2, Server 2003 or later.";
        public const string UNABLE_TO_WRITE_FILE = "Unable to write file to {0}";
        public const string UNABLE_TO_WRITE_BUFFER_TO_FILE = "Unable to write file to {0}";
        public const string DIRECTORY_LISTING_HEADER = @"<html><head></head><body><h1>http://{0}/</h1><p>
                                                            The URL specified is a folder that does not contain a default document or application.The following is a listing of what documents and subfolders it does have; please select one:</p><p>
                                                            </p><pre> Last Modified       Size Name
                                                            <hr>";
        public const string DIRECTORY_LISTING_ITEMS = @"{0} {1}    {2} <a href=""{3}"">{4}</a>";
        public const string DIRECTORY_LISTING_FOOTER = @"</pre>
                                                            <hr><em> Do your part to beautify the web! Turn<strong> off</strong> link underlining!</em>
                                                            </body></html>";
        public const string ITEM_FILE = "File";
        public const string ITEM_FOLDER = "Folder";
        public const string NULL_PATH_URL = "Null Path in URL";
    }
}
