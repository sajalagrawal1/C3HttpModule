using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.C3HttpModule
{
    /// <summary>
    /// Class is a wrapper / manager of a ConcurrentDictionary. This class is used as register of all available HttpResponseBuffer(s) as session. 
    /// </summary>
    internal class HttpUrlList
    {
        /// <summary>
        /// log4net logger, used for logging.
        /// </summary>
        private static ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// private object of ConcurrentDictionary.
        /// </summary>
        private ConcurrentDictionary<string, HttpResponseBuffer> _httpUrlList = new ConcurrentDictionary<string, HttpResponseBuffer>();

        /// <summary>
        /// Adds Url and object of HttpResponseBuffer to ConcurrentDictionary
        /// </summary>
        /// <param name="Url"> Url that is called by the user agent.</param>
        /// <param name="session">Session is an object HttpResponseBuffer. See HttpResponseBuffer documentation for more.</param>
        //ToDo : Failure checks
        public bool AddUrl(string Url, HttpResponseBuffer session)
        {
            bool retVal = false;
            if (string.IsNullOrEmpty(Url))
            {
                _logger.Error(string.Format(Constants.LOG_RESOURCEUNABLETOADD, Url));
            }
            else
            {
                Url = Url.ToLower();
                if ( _httpUrlList.ContainsKey(Url))
                {
                    HttpResponseBuffer val;
                    _httpUrlList.TryGetValue(Url, out val);
                    retVal = _httpUrlList.TryUpdate(Url, session, val);
                }
                else
                {
                    retVal = _httpUrlList.TryAdd(Url, session);
                }
                if (retVal == true)
                {
                    _logger.Info(string.Format(Constants.LOG_RESOURCEADDED, Url));
                }
                else
                {
                    _logger.Error(string.Format(Constants.LOG_RESOURCEUNABLETOADD, Url));
                }
            }
            return retVal;
        }
        /// <summary>
        /// Gets HttpResponseBuffer object aligned with strUrl.  
        /// </summary>
        /// <param name="strUrl">Url against which the response Buffer is extracted.</param>
        /// <returns></returns>
        public HttpResponseBuffer GetUrl(string strUrl)
        {
            HttpResponseBuffer responseBuffer = null;
            if (strUrl==null)
            {
                _logger.Error(Constants.NULL_PATH_URL + Utilities.GetMethodName() );
            }
            else
            {
                strUrl = strUrl.ToLower();

                if (_httpUrlList.TryGetValue(strUrl, out responseBuffer) == true)
                {
                    _logger.Info(string.Format(Constants.LOG_RESOURCEFOUND, strUrl));
                }
                else
                {
                    _logger.Error(string.Format(Constants.LOG_RESOURCENOTFOUND, strUrl));
                }
            }

            return responseBuffer;
        }
        /// <summary>
        /// Removes the Url and the object from ConcurrentDictionary.
        /// </summary>
        /// <param name="Url">Url which needs to be removed</param>
        public bool RemoveUrl(string Url)
        {
            bool retVal = false;
            HttpResponseBuffer responseBuffer = null;
            if (Url  == null)
            {
                _logger.Error(Constants.NULL_PATH_URL + Utilities.GetMethodName());
            }
            else
            {
                Url = Url.ToLower();
                if (_httpUrlList.TryRemove(Url, out responseBuffer) == true)
                {
                    retVal = true;
                    _logger.Info(string.Format(Constants.LOG_RESOURCEREMOVED, Url));
                }
                else
                {
                    _logger.Error(string.Format(Constants.LOG_RESOURCENOTREMOVED, Url));
                }
            }
            return retVal;
        }
        /// <summary>
        /// Checks if Url exists in the Concurrent dictionary as a key. It does not checks for a sanity of the HttpResponseBuffer object.
        /// </summary>
        /// <param name="url">URL to check</param>
        /// <returns></returns>
        public bool UrlExists(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;
            url = url.ToLower();
            bool contains = _httpUrlList.ContainsKey(url);
            return contains;
        }

        /// <summary>
        /// Called from setter of VirtualURLLifeSpan. Changes the time span of all the objects associated with the Url. 
        /// Warning : It will refresh the timeout from DateTime.Now.
        /// </summary>
        public void UpdateTimeOutOfAllUrls()
        {
            IEnumerator<KeyValuePair<string, HttpResponseBuffer>> enumerator = _httpUrlList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<string,HttpResponseBuffer> value = enumerator.Current;
                value.Value.UpdateTimeOut();
            }
        }
    }
}
