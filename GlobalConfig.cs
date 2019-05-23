using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.C3HttpModule
{
    /// <summary>
    /// Any Global Configuration accross the Module should go in here.
    /// </summary>
    internal static class GlobalConfig
    {
        /// <summary>
        /// Static value of Virtual URL Life span. Created globally to get accessed accross application.
        /// </summary>
        public static long VirtualURLLifeSpan { get; internal set; }

        /// <summary>
        /// Static string Root Path, so that configured root path can be accessed any where across the module.
        /// </summary>
        public static string RootPath { get; set; } = string.Empty;
    }
}
