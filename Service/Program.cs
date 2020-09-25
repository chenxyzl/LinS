using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base.Interface;

namespace Service
{
    public class Service : IService
    {
        private int a = 1;
        private List<IService> services = new List<IService>();

        public Service()
        {
            services.Add(new LoginService());
        }

        public void Dispose()
        {
            Console.WriteLine("Service Dispose");
            foreach (var s in services)
            {
                s.Dispose();
            }
        }

        async public Task Init()
        {
            Console.WriteLine("Service Init");
            foreach (var s in services)
            {
                await s.Init();
            }
        }
        async public Task Load()
        {
            Console.WriteLine($"Service Load: a:{a}");
            foreach (var s in services)
            {
                await s.Load();
            }
        }
        async public Task Start()
        {
            Console.WriteLine("Service Start");
            foreach (var s in services)
            {
                await s.Start();
            }
        }
        public void Update()
        {
            Console.WriteLine("Service Update");
            foreach (var s in services)
            {
                s.Update();
            }
        }
        async public Task Stop()
        {
            Console.WriteLine("Service Stop");
            foreach (var s in services)
            {
                await s.Stop();
            }
        }
        async public Task Destory()
        {
            Console.WriteLine("Service Destory");
            foreach (var s in services)
            {
                await s.Destory();
            }
        }
    }
}
