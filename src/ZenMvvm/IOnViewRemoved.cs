using System.Threading.Tasks;

namespace ZenMvvm
{
    /// <summary>
    /// Implements <see cref="OnViewRemovedAsync"/>
    /// </summary>
    public interface IOnViewRemoved
    {
        /// <summary>
        /// Runs automatically when a <see cref="Xamarin.Forms.Page"/>
        /// is popped or removed from the <see cref="NavigationService.NavigationStack"/>.
        /// </summary>
        /// <returns></returns>
        Task OnViewRemovedAsync();
    }
}
