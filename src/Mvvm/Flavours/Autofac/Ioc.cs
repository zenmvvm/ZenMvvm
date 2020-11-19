using Autofac;

namespace ZenMvvm
{
    public static class Ioc
    {
        public static void Init(IContainer container)
        {
            ViewModelLocator.Ioc = new IocAdaptor(container);
        }

        public static void Init(ContainerBuilder builder)
        {
            ViewModelLocator.Ioc = new IocAdaptor(builder.Build());
        }
    }
}
