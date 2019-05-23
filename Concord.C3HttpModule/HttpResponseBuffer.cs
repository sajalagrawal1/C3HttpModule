using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Concord.C3HttpModule
{
    internal class HttpResponseBuffer
    {
        /// <summary>
        /// log4net logger instance
        /// </summary>
        private static ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Lock object used to create a thread safe call to file write.
        /// </summary>
        private static readonly object _lock = new object();
        /// <summary>
        /// Enumerator to define response buffer type, File or Buffer (a buffer is simply a string).
        /// </summary>
        public enum BufferType
        {
            FILE , BUFFER
        };
        /// <summary>
        /// Resoure time out, in seconds. The resource should not be served after TimeOut seconds.
        /// </summary>
        public long TimeOut { get; set; }
        /// <summary>
        /// BufferType Enum for object
        /// </summary>
        public BufferType DataType { get; set; }
        /// <summary>
        /// Buffer value. Should contain a string as a valid file system path or a string that need to be served.
        /// </summary>
        public string BufferValue { get; set; }
        /// <summary>
        /// Expiry Time is in Tick Count as Utc. After ExpiryTime tick count is crossed the resource should not be served. Caution : it can be over rided by IgnoreExpiryTime flag.
        /// </summary>
        public long ExpiryTime { get; private set; }
        /// <summary>
        /// If this flag is set, the expiry time is over rided. It happens when the Constructor is called with timeout value as zero. Please note that there is another method ExtendExpiryTime, it does not affect the  IgnoreExpiryTime flag.
        /// </summary>
        public bool IgnoreExpiryTime { get; private set; }
        /// <summary>
        /// Cache name is used to create file name, that shall be written on call of UrlToPath
        /// </summary>
        public string CacheName { get; }

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="type">BufferType : File or string type.</param>
        /// <param name="value"> Value of BufferType, in case of file it should be a readable file system path or string that need to be served as response</param>
        /// <param name="timeOut">Timeout , pass zero if need to set it infinite. (See IgnoreExpiryTime property)</param>
        /// <param name="cacheName">Name of file to be written.</param>
        public HttpResponseBuffer(BufferType type, string value, long timeOut, string cacheName)
        {
            this.TimeOut = timeOut;
            this.DataType = type;
            this.BufferValue = value;
            this.CacheName = cacheName;

            string fileSystemDirectory = Utilities.GetDirectoryPathFromUrlPath(GlobalConfig.RootPath, cacheName);
            if (!Directory.Exists(fileSystemDirectory))
            {
                Directory.CreateDirectory(fileSystemDirectory);
            }

            if (type == BufferType.FILE)
            {
                try
                {
                    string driveFilePath = Path.Combine(fileSystemDirectory, Path.GetFileName(cacheName));
                    File.Copy(value, driveFilePath);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format(Constants.UNABLE_TO_WRITE_FILE, value));
                    _logger.Error(Utilities.ReadableException(ex));
                }
            }
            if (type == BufferType.BUFFER)
            {
                try
                {
                    string driveFilePath = Path.Combine(fileSystemDirectory, Path.GetFileName(cacheName));
                    File.WriteAllText(driveFilePath, value);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format(Constants.UNABLE_TO_WRITE_BUFFER_TO_FILE, value));
                    _logger.Error(Utilities.ReadableException(ex));
                }
            }

            UpdateTimeOut();
        }

        /// <summary>
        /// This method updates the time out, internally from the constructor and externally called when value of VirtualURLLifeSpan changes.
        /// </summary>
        public void UpdateTimeOut()
        {
            if (this.TimeOut == 0)
            {
                if (GlobalConfig.VirtualURLLifeSpan == -1)
                    IgnoreExpiryTime = true;
                else
                {
                    IgnoreExpiryTime = false;
                    ExpiryTime = DateTime.UtcNow.Ticks + (GlobalConfig.VirtualURLLifeSpan * TimeSpan.TicksPerSecond);
                }

            }
            else
            {
                IgnoreExpiryTime = false;
                ExpiryTime = DateTime.UtcNow.Ticks + (this.TimeOut * TimeSpan.TicksPerSecond);
            }
        }



        /// <summary>
        /// Api To Get Data of Buffer, 
        /// </summary>
        /// <returns>If the BufferType is File the API will return file content.  Else will return string it self if BufferType.BUFFER</returns>
        public string GetData()
        {
            string retval = string.Empty;
            switch (DataType)
            {
                case BufferType.FILE:
                    try
                    {
                        retval = System.IO.File.ReadAllText(BufferValue);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.ReadableException(ex));
                    }
                    break;
                case BufferType.BUFFER:
                    retval = BufferValue;
                    URLtoPath();
                    break;
            }
            return retval;
        }
        /// <summary>
        /// Called from PokeUrlWithExpiry. Extends expiry time of the object by 'seconds'. Also, updates TimeOut of the object.
        /// </summary>
        /// <param name="seconds">Extension time in seconds</param>
        public void ExtendExpiryTime(long seconds)
        {
            TimeOut = seconds;
            ExpiryTime = DateTime.UtcNow.Ticks + ((long) seconds  * TimeSpan.TicksPerSecond);
        }
        /// <summary>
        /// Called from PokeUrl. Extends expiry time of the object by TimeOut seconds. But does not update TimeOut of the object. It uses TimeOut seconds. So the new timeout/expiry time will be TimeOut plus CurrentTime.
        /// </summary>
        /// <param name="seconds">Extension time in seconds</param>
        public void ExtendExpiryTime()
        {
            ExpiryTime = DateTime.UtcNow.Ticks + ((long)TimeOut * TimeSpan.TicksPerSecond);
        }

        /// <summary>
        /// If BufferType.BUFFER is set for the object it will write the content of buffer to file name (given in CacheName) in GlobalConfig.RootPath folder. 
        /// </summary>
        private void URLtoPath()
        {
            try
            {
                lock (_lock)
                {
                    string filePathToWrite = System.IO.Path.Combine(GlobalConfig.RootPath, this.CacheName);
                    if (System.IO.File.Exists(filePathToWrite))
                    {
                        File.Delete(filePathToWrite);
                    }
                    System.IO.File.WriteAllText(filePathToWrite, this.BufferValue);
                }
            }
            catch (IOException ioex)
            {
                _logger.Error(Utilities.ReadableException(ioex));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.ReadableException(ex));
            }
        }

        
    }
}
