using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.Core.Classes
{
    public class ServerResourceInfo
    {
        public string serverName { get; set; }

        public string resourceMethodName { get; set; }

        public bool? isServerUnReachable { get; set; }

        public List<ModuleStatusInfo> moduleStatusInfos { get; set; }
    }


    public class ModuleStatusInfo
    {
        public string moduleName { get; set; }

        public bool? isWorking { get; set; }

        public string exceptionMessage { get; set; }
    }
}
