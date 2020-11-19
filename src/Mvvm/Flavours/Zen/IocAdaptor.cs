using System;
using SmartDi;

namespace ZenMvvm
{
    public class IocAdaptor : IIoc
    {
        readonly DiContainer container;

        public IocAdaptor(DiContainer container)
        {
            this.container = container;
        }

        public object Resolve(Type typeToResolve)
        {
            return container.Resolve(typeToResolve);
        }
    }
}
