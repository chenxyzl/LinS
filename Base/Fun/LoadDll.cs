using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
namespace Base.Fun
{
    /// <summary>
    /// dll文件的加载
    /// </summary>
    public class LoadDll<T> where T : Base.Interface.IService
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

        private string typeName
        {
            get { return typeof(T).Name; }
        }

        private volatile HashSet<T> _instances = new HashSet<T>();

        public LoadDll(string filepath)
        {
            this.filepath = filepath;
        }

        public K Get<K>() where K : class, T
        {
            foreach (var item in _instances)
            {
                if (item.GetType().GetInterface(typeof(K).Name) != null)
                {
                    item.Load();
                    return item as K;
                }
            }
            var name = typeof(K).Name;
            throw new Exception($"class not found: {name}");
        }

        public void LoadFile()
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
                        if (item.GetInterface(typeName) != null && item.IsClass)
                        {
                            var ins = (T)Activator.CreateInstance(item);
                            ins.Load();
                            _instances.Add(ins);
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); };
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
                foreach (var item in _instances)
                {
                    item.Dispose();
                }
                _instances = new HashSet<T>();
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