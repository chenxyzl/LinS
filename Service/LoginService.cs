using System;
using System.Threading.Tasks;
using Base.Interface;

namespace Service
{
    public interface ILoginService : IService
    {
        public string GetMessage();
    }

    public class LoginService : ILoginService
    {
        private int a = 1;

        public LoginService()
        {
            Console.WriteLine("LoginService LoginService");
        }

        public string GetMessage()
        {
            return "Hello 1";
        }

        public void Dispose()
        {
            Console.WriteLine("LoginService Dispose");
        }

        async public Task Init()
        {
            Console.WriteLine("LoginService Init");
        }
        async public Task Load()
        {
            Console.WriteLine($"LoginService Load: a:{a}");
        }
        async public Task Start()
        {
            Console.WriteLine("LoginService Start");
        }
        public void Update()
        {
            Console.WriteLine("LoginService Update");
        }
        async public Task Stop()
        {
            Console.WriteLine("LoginService Stop");
        }
        async public Task Destory()
        {
            Console.WriteLine("LoginService Destory");
        }
    }
}
