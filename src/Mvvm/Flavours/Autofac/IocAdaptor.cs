using System;
using Autofac;

namespace ZenMvvm
{
    public class IocAdaptor : IIoc
    {
        readonly IContainer container;

        public IocAdaptor(IContainer container)
        {
            this.container = container;
        }

        public object Resolve(Type typeToResolve)
        {
            return container.Resolve(typeToResolve);
        }
    }
}
