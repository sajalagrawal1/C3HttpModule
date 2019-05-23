using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Concord.C3HttpModule
{
    /// <summary>
    /// Utility static method can be coded here.
    /// </summary>
    internal static class Utilities
    {
        /// <summary>
        /// Utility method to wrap exception and all its inner exceptions to a string. Concats Message and Stack Trace to a string.
        /// </summary>
        /// <param name="exception"> Exception to string-ify</param>
        /// <returns>String-ified exceptions</returns>
        public static string ReadableException(Exception exception)
        {
            string returnvalue = string.Empty;
            Exception exceptionToLog = exception;
            while(exceptionToLog != null)
            {
                returnvalue += string.Format(Constants.EXCEPTION_FORMAT_STRING, exception.Message, exception.StackTrace);
                exceptionToLog = exceptionToLog.InnerException;
            }
            return returnvalue;
        }
        /// <summary>
        /// Two directories as a string are passed. One is Root Path and another is cacheName. CacheName is converted into windows path. If cachename ends with a file name, it trims of the file name to get directory from the cacheName argument and joins it to RootPath
        /// Example : RootPath = C:\Temp , cacheName = Folder/filename.html => Output : C:\Temp\Folder
        /// </summary>
        /// <param name="rootPath">Path to append folder to</param>
        /// <param name="cacheName">Extract folder from path</param>
        /// <returns>Path as string, joins RootPath and directory of cacheName</returns>
        public static string GetDirectoryPathFromUrlPath(string rootPath, string cacheName)
        {
            string cleanUrl = Utilities.CleanUnixUrl(cacheName);
            string windowsPath = Utilities.UnixToWindowsPath(cleanUrl);
            string directoryPathFromUrl = Path.GetDirectoryName(windowsPath);
            string combinedPath = System.IO.Path.Combine(GlobalConfig.RootPath, directoryPathFromUrl);
            return combinedPath;
        }

        /// <summary>
        /// Returns sperator used in path.
        /// </summary>
        /// <param name="path">Url Path</param>
        /// <returns></returns>
        static Char GetDirectorySeparatorUsedInPath(string path)
        {
            if (path.Contains(Path.AltDirectorySeparatorChar))
                return Path.AltDirectorySeparatorChar;

            return Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Converts slashes to back slashes in a path. Blindly replaces / to \. Suggested that it should be used after CleanUnixUrl call.
        /// </summary>
        /// <param name="nixPath">Unix Path with slashes</param>
        /// <returns>string with backslash.</returns>
        internal static string UnixToWindowsPath(string nixPath)
        {
            if (string.IsNullOrEmpty(nixPath))
                return string.Empty;
            return nixPath.Replace('/', '\\');
        }

        /// <summary>
        /// Returns a clean unix url. Converts multiple slashes to one slash. Removes leading slash. 
        /// </summary>
        /// <param name="rawUrl">Takes a url</param>
        /// <returns>Trimmed well mannered URL</returns>
        internal static string CleanUnixUrl(string rawUrl)
        {
            List<string> parted = rawUrl.Split('/').ToList();
            parted = parted.Where(x => !string.IsNullOrEmpty(x)).ToList();
            string retVal = string.Join("/", parted);
            return retVal;
        }

        /// <summary>
        /// Gets Calling method name
        /// </summary>
        /// <returns>Return method name</returns>
        internal static string GetMethodName()
        {
            return new StackTrace(1).GetFrame(0).GetMethod().Name;
        }


    }
}
