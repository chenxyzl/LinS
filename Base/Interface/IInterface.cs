using System;
using System.Threading.Tasks;

namespace Base.Interface
{
    public interface IBase
    {
        public Task Init();
        public Task Load();
        public Task Start();
        public void Update();
        public Task Stop();
        public Task Destory();
    }

    public interface IService : IBase, IDisposable//因为要热加载 要手动改释放
    {
    }

    public interface IPlayer : IBase
    {
    }

}
