using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Utils.Core.Classes;

namespace Utils.Core.Code
{
    internal static class UtilitiesLocal
    {
        internal static List<ModuleStatusInfo> checkModules(List<Func<ModuleStatusInfo>> moduleCheckFuncs, TimeSpan timeOut)
        {
            List<ModuleStatusInfo> moduleStatusInfos = new List<ModuleStatusInfo>();

            if (moduleCheckFuncs == null)
            {
                return moduleStatusInfos;
            }

            var moduleStatusInfoTasks = new Dictionary<string, Task<ModuleStatusInfo>>();

            foreach (var moduleCheckFunc in moduleCheckFuncs)
            {
                if (moduleCheckFunc != null)
                {
                    try
                    {
                        var moduleStatusInfoTask = Task.Run(moduleCheckFunc);

                        moduleStatusInfoTasks.Add($"{moduleCheckFunc.Method.Name}*{Guid.NewGuid()}", moduleStatusInfoTask);
                    }
                    catch (Exception ex)
                    {
                        var moduleStatusInfo = new ModuleStatusInfo
                        {
                            isWorking = false,
                            moduleName = moduleCheckFunc.Method.Name,
                            exceptionMessage = $"{ex.Message} | {ex.InnerException?.Message}"
                        };

                        moduleStatusInfos.Add(moduleStatusInfo);
                    }
                }
            }

            if (moduleStatusInfoTasks.Count > 0)
            {
                try { Task.WhenAll(moduleStatusInfoTasks.Values).Wait(timeOut); } catch { }

                foreach (var moduleStatusInfoTask in moduleStatusInfoTasks)
                {
                    if (moduleStatusInfoTask.Value.IsCompleted && !moduleStatusInfoTask.Value.IsFaulted && !moduleStatusInfoTask.Value.IsCanceled)
                    {
                        moduleStatusInfos.Add(moduleStatusInfoTask.Value.Result);
                    }
                    else
                    {
                        ModuleStatusInfo moduleStatusInfo = null;

                        if (moduleStatusInfoTask.Value.IsFaulted)
                        {
                            moduleStatusInfo = new ModuleStatusInfo
                            {
                                moduleName = moduleStatusInfoTask.Key.Split('*')[0],
                                isWorking = false,
                                exceptionMessage = $"{moduleStatusInfoTask.Value.Exception?.Message} | {moduleStatusInfoTask.Value.Exception?.InnerException?.Message}"
                            };
                        }
                        else
                        {
                            moduleStatusInfo = new ModuleStatusInfo
                            {
                                moduleName = moduleStatusInfoTask.Key.Split('*')[0],
                                isWorking = false,
                                exceptionMessage = $"module execution timed out"
                            };
                        }

                        moduleStatusInfos.Add(moduleStatusInfo);
                    }
                }
            }

            return moduleStatusInfos;
        }
    }
}
