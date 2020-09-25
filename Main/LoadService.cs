using Base.Interface;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
namespace Main
{
    /// <summary>
    /// dll文件的加载
    /// </summary>
    public class LoadDll
    {
        /// <summary>
        /// 任务实体
        /// </summary>
        /// <summary>
        /// 核心程序集加载
        /// </summary>
        public AssemblyLoadContext _AssemblyLoadContext { get; set; }
        /// <summary>
        /// 获取程序集
        /// </summary>
        public Assembly _Assembly { get; set; }
        /// <summary>
        /// 文件地址
        /// </summary>
        public string filepath = string.Empty;
        /// <summary>
        /// 指定位置的插件库集合
        /// </summary>
        AssemblyDependencyResolver resolver { get; set; }


        private volatile bool _changed = false;
        private FileSystemWatcher _watcher;

        public IService service = null;

        public LoadDll(string filepath)
        {
            this.filepath = filepath;
            if (!Path.IsPathRooted(filepath))
            {
                this.filepath = Path.GetFullPath(Path.Combine(System.IO.Directory.GetCurrentDirectory(), this.filepath));
            }
            Load();
            ListenFileChanges();
        }

        private void ListenFileChanges()
        {
            Action<string> onFileChanged = path =>
            {
                    _changed = true;
            };
            var path = Path.GetDirectoryName(filepath);
            _watcher = new FileSystemWatcher(path, "Service.dll");
            _watcher.IncludeSubdirectories = true;
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            _watcher.Changed += (sender, e) => onFileChanged(e.FullPath);
            _watcher.Created += (sender, e) => onFileChanged(e.FullPath);
            _watcher.Deleted += (sender, e) => onFileChanged(e.FullPath);
            _watcher.Renamed += (sender, e) => { onFileChanged(e.FullPath); onFileChanged(e.OldFullPath); };
            _watcher.EnableRaisingEvents = true;
        }

        public bool CheckReload()
        {
            if (!_changed)
            {
                return false;
            }

            _changed = false;
            //卸载老的
            UnLoad();

            //加载新的
            Load();

            return true;
        }

        //todo 需要watch
        private void Load()
        {
            //先卸载老的
            //todo 这里应该是先加载更新的，出了异常就还是用老的，加载成功则用新的，并卸载老的
            UnLoad();
            try
            {
                resolver = new AssemblyDependencyResolver(filepath);
                _AssemblyLoadContext = new AssemblyLoadContext(Guid.NewGuid().ToString("N"), true);
                _AssemblyLoadContext.Resolving += _AssemblyLoadContext_Resolving;

                using (var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    _Assembly = _AssemblyLoadContext.LoadFromStream(fs);
                    var types = _Assembly.GetTypes();
                    for (int i = 0; i < types.Length; i++)
                    {
                        Type item = types[i];
                        if (typeof(Service.Service).FullName == item.GetTypeInfo().FullName && item.IsClass)
                        {
                            service = (IService)Activator.CreateInstance(item);
                        }
                    }
                }
                if (service == null)
                {
                    throw new Exception("not found class Service");
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); };
        }

        private Assembly _AssemblyLoadContext_Resolving(AssemblyLoadContext arg1, AssemblyName arg2)
        {
            Console.WriteLine($"加载{arg2.Name}");
            var path = resolver.ResolveAssemblyToPath(arg2);
            if (!string.IsNullOrEmpty(path))
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    return _AssemblyLoadContext.LoadFromStream(fs);
                }
            }
            return null;
        }
        public bool UnLoad()
        {
            try
            {
                _AssemblyLoadContext?.Unload();
                service?.Dispose();
                _watcher?.Dispose();
            }
            catch (Exception)
            { }
            finally
            {
                _AssemblyLoadContext = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return true;
        }
    }
}