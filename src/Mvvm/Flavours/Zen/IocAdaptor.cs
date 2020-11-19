using System;
using SmartDi;

namespace ZenMvvm
{
    public class IocAdaptor : IIoc
    {
        readonly IDiContainer container;

        public IocAdaptor(IDiContainer container)
        {
            this.container = container;
        }

        public object Resolve(Type typeToResolve)
        {
            return container.Resolve(typeToResolve);
        }
    }
}
