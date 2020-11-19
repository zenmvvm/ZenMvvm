using System;
using SmartDi;

namespace ZenMvvm
{
    public static class Ioc
    {
        public static void Init()
        {
            ViewModelLocator.Ioc = new IocAdaptor(new DiContainer());
        }
        public static void Init(ContainerOptions settings)
        {
            ViewModelLocator.Ioc = new IocAdaptor(new DiContainer(settings));
        }
        public static void Init(Action<ContainerOptions> configureSettings)
        {
            ViewModelLocator.Ioc = new IocAdaptor(new DiContainer(configureSettings));
        }

    }
}
