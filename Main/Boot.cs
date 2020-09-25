using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Main
{
    class Boot
    {
        //service加载
        private LoadDll serviceLoad = null;

        //监听退出信号量
        ManualResetEvent exitEvent = new ManualResetEvent(false);

        //主循环timer
        System.Timers.Timer timer = new System.Timers.Timer
        {
            Enabled = true,
            Interval = 50,//执行间隔时间,单位为毫秒;此时时间间隔为50毫秒，即为1分钟执行20次tick
        };

        private bool reloadServiceing = false;

        public void WatchSigal()
        {
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };
        }

        public void StartLoop()
        {
            timer.Elapsed += new ElapsedEventHandler(Loop);
            timer.Start();
        }

        public void StopLoop()
        {
            timer.Stop();
        }

        async public void Run()
        {
            //加载配置

            //加载model

            //加载service
            serviceLoad = new LoadDll("../../../../Dll/Service.dll");

            //运行service
            await RunService();

            //监听退出事情
            WatchSigal();

            //开启无线循环
            StartLoop();

            //等待退出循环
            exitEvent.WaitOne();

            //退出循环
            StopLoop();

            //销毁service
            await StopService();

            //销毁model
            Console.ReadKey();
        }

        async Task RunService()
        {
            if (reloadServiceing)
            {
                await StopService();
            }
            await serviceLoad.service.Init();
            await serviceLoad.service.Load();
            await serviceLoad.service.Start();
            //最后重置回来
            reloadServiceing = false;
        }

        async Task StopService()
        {
            await serviceLoad.service.Stop();
            await serviceLoad.service.Destory();
        }


        void Loop(object source, ElapsedEventArgs e)
        {
            //是否在重新加载service中
            if (reloadServiceing)
            {
                return;
            }

            //是否service有变动
            if (serviceLoad.CheckReload())
            {
                RunService();
                return;
            }

            //service主循环
            serviceLoad.service?.Update(); //主循环，有可能service正在重新加载，导致service为空
        }
    }
}
