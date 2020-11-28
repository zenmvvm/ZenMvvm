using System.ComponentModel;
using System.Threading.Tasks;

namespace ZenMvvm
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IOnViewNavigatedGroup<T> { }


    /// <summary>
    /// Implements <see cref="OnViewNavigated(T)"/>
    /// </summary>
    public interface IOnViewNavigated<T> : IOnViewNavigatedGroup<T>
    {
        /// <summary>
        /// Runs automatically when a <see cref="Xamarin.Forms.Page"/>
        /// is pushed onto the <see cref="NavigationService.NavigationStack"/>.
        /// </summary>
        /// <param name="navigationData">Any data required for ViewModel initialisation</param>
        /// <returns></returns>
        void OnViewNavigated(T navigationData);
    }

    /// <summary>
    /// Implements <see cref="OnViewNavigated"/>
    /// </summary>
    public interface IOnViewNavigated
    {
        /// <summary>
        /// Runs automatically when a <see cref="Xamarin.Forms.Page"/>
        /// is pushed onto the <see cref="NavigationService.NavigationStack"/>.
        /// </summary>
        void OnViewNavigated();
    }

    /// <summary>
    /// Implements <see cref="OnViewNavigatedAsync(T)"/>
    /// </summary>
    public interface IOnViewNavigatedAsync<T> : IOnViewNavigatedGroup<T>
    {
        /// <summary>
        /// Runs automatically when a <see cref="Xamarin.Forms.Page"/>
        /// is pushed onto the <see cref="NavigationService.NavigationStack"/>.
        /// </summary>
        /// <param name="navigationData">Any data required for ViewModel initialisation</param>
        /// <returns></returns>
        Task OnViewNavigatedAsync(T navigationData);
    }

    /// <summary>
    /// Implements <see cref="OnViewNavigatedAsync"/>
    /// </summary>
    public interface IOnViewNavigatedAsync
    {
        /// <summary>
        /// Runs automatically when a <see cref="Xamarin.Forms.Page"/>
        /// is pushed onto the <see cref="NavigationService.NavigationStack"/>.
        /// </summary>
        Task OnViewNavigatedAsync();
    }
}
