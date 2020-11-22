using System;
using SmartDi;

namespace ZenMvvm
{
    public static class Ioc
    {
        public static IDiContainer Container { get; private set; }

        public static void Init()
        {
            Container = new DiContainer();
            ViewModelLocator.Ioc = new IocAdaptor(Container);
        }
        public static void Init(ContainerOptions settings)
        {
            Container = new DiContainer(settings);
            ViewModelLocator.Ioc = new IocAdaptor(Container);
        }
        public static void Init(Action<ContainerOptions> configureSettings)
        {
            Container = new DiContainer(configureSettings);
            ViewModelLocator.Ioc = new IocAdaptor(Container);
        }

    }
}
