using Xamarin.Forms;

namespace ZenMvvm
{
    /// <summary>
    /// An implementation of <see cref="RouteFactory"/> that resolves 
    /// the <see cref="Element"/> (usually a view) from the
    /// dependency injection engine
    /// </summary>
    /// <typeparam name="T">An <see cref="Element"/> (usually a view)</typeparam>
    public class ResolverRouteFactory<T> : RouteFactory where T : Element
    {
        /// <summary>
        /// Fetches or Creates the <see cref="Element"/>
        /// </summary>
        /// <returns><see cref="Element"/> (usually a view)</returns>
        public override Element GetOrCreate()
        {
            return (Element)ViewModelLocator.Ioc.Resolve(typeof(T));
        }
    }
}
