using DryIoc;

namespace ZenMvvm
{
    public static class Ioc
    {
        public static void Init()
        {
            ViewModelLocator.Ioc = new IocAdaptor(new Container());
        }
        public static void Init(Rules settings)
        {
            ViewModelLocator.Ioc = new IocAdaptor(new Container(settings));
        }
    }
}
