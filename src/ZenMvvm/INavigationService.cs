using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ZenMvvm
{
    /// <summary>
    /// Controlls Page Navigation from the ViewModel
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Returns Xamarin.Forms.Shell.Current
        /// </summary>
        Shell CurrentShell { get; }
        /// <summary>
        /// Gets the Current Shell's NavigationStack
        /// </summary>
        IReadOnlyList<Page> NavigationStack { get; }
        /// <summary>
        /// Gets the Current Shell's ModalStack
        /// </summary>
        IReadOnlyList<Page> ModalStack { get; }

        /// <summary>
        /// Returns a singleton instance of the MessagingCenter
        /// </summary>
        Helpers.ISafeMessagingCenter SafeMessagingCenter { get; }

        /// <summary>
        /// Invokes an Action on the device's main (UI) thread.
        /// Wrapper of Xamarin.Forms <see cref="Device.BeginInvokeOnMainThread(Action)"/>
        /// </summary>
        void BeginInvokeOnMainThread(Action action);

        /// <summary>
        /// Navigates to a <see cref="Page"/>
        /// </summary>
        /// <param name="state">A URI representing either the current page or a destination for navigation in a Shell application.</param>
        /// <returns></returns>    
        Task GoToAsync(ShellNavigationState state);

        /// <summary>
        /// Navigates to a <see cref="Page"/>
        /// </summary>
        /// <param name="state">A URI representing either the current page or a destination for navigation in a Shell application.</param>
        /// <param name="animate"></param>
        /// <returns></returns>
        Task GoToAsync(ShellNavigationState state, bool animate = true);

        /// <summary>
        /// Navigates to a <see cref="Page"/> passing data to the target ViewModel
        /// </summary>
        /// <param name="state"></param>
        /// <param name="navigationData"></param>
        /// <param name="animate"></param>
        /// <returns></returns>
        Task GoToAsync<TData>(ShellNavigationState state, TData navigationData, bool animate = true);

        /// <summary>
        /// Navigates to a <see cref="Page"/>
        /// </summary>
        /// <param name="state">A URI representing either the current page or a destination for navigation in a Shell application.</param>
        /// <typeparam name="TViewModel">Expected ViewModel Type for the page at the given <paramref name="state"/></typeparam>
        Task<TViewModel> GoToAsync<TViewModel>(ShellNavigationState state) where TViewModel : class;

        /// <summary>
        /// Navigates to a <see cref="Page"/>
        /// </summary>
        /// <param name="state">A URI representing either the current page or a destination for navigation in a Shell application.</param>
        /// <param name="animate"></param>
        /// <typeparam name="TViewModel">Expected ViewModel Type for the page at the given <paramref name="state"/></typeparam>
        /// <returns></returns>
        Task<TViewModel> GoToAsync<TViewModel>(ShellNavigationState state, bool animate = true) where TViewModel : class;

        /// <summary>
        /// Navigates to a <see cref="Page"/> passing data to the target ViewModel
        /// </summary>
        /// <param name="state"></param>
        /// <param name="navigationData"></param>
        /// <param name="animate"></param>
        /// <typeparam name="TViewModel">Expected ViewModel Type for the page at the given <paramref name="state"/></typeparam>
        /// <returns></returns>
        Task<TViewModel> GoToAsync<TViewModel, TData>(ShellNavigationState state, TData navigationData, bool animate = true) where TViewModel : class;


        /// <summary>
        /// Pops a <see cref="Page"/> off the <see cref="NavigationStack"/>
        /// </summary>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task PopAsync(bool animated = true);

        /// <summary>
        /// Pops a <see cref="Page"/> off the <see cref="ModalStack"/>
        /// </summary>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task PopModalAsync(bool animated = true);

        /// <summary>
        /// Pops all pages <see cref="Page"/> off the <see cref="NavigationStack"/>, leaving only the Root Page
        /// </summary>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task PopToRootAsync(bool animated = true);

        /// <summary>
        /// Pushes a <see cref="Page"/> onto the <see cref="NavigationStack"/>
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel corresponding the the Page to be Pushed</typeparam>
        /// <param name="navigationData">Object that will be recieved by the <see cref="IOnViewNavigatedAsync.OnViewNavigatedAsync(object)"/> method</param>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task<TViewModel> PushAsync<TViewModel,TData>(TData navigationData, bool animated = true) where TViewModel : class, IOnViewNavigatedGroup<TData>;

        /// <summary>
        /// Pushes a <see cref="Page"/> onto the <see cref="NavigationStack"/>
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel corresponding the the Page to be Pushed</typeparam>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task<TViewModel> PushAsync<TViewModel>(bool animated = true) where TViewModel : class;

        /// <summary>
        /// Pushes a <see cref="Page"/> onto the <see cref="ModalStack"/>
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel corresponding the the Page to be Pushed</typeparam>
        /// <param name="navigationData">Object that will be recieved by the <see cref="IOnViewNavigatedAsync.OnViewNavigatedAsync(object)"/> method</param>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task<TViewModel> PushModalAsync<TViewModel,TData>(TData navigationData, bool animated = true) where TViewModel : class, IOnViewNavigatedGroup<TData>;

        /// <summary>
        /// Pushes a <see cref="Page"/> onto the <see cref="ModalStack"/>
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel corresponding the the Page to be Pushed</typeparam>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task<TViewModel> PushModalAsync<TViewModel>(bool animated = true) where TViewModel : class;

        /// <summary>
        /// Removes specific <see cref="Page"/> from the <see cref="NavigationStack"/>
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel corresponding the the Page to be Removed</typeparam>
        /// <returns></returns>
        Task RemovePageFor<TViewModel>();

        /// <summary>
        /// Removes the <see cref="Page"/> underneath the Top Page in the <see cref="NavigationStack"/>
        /// </summary>
        /// <returns></returns>
        Task RemovePreviousPageFromMainStack();

        /// <summary>
        /// Display's an Alert
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        Task DisplayAlert(string title, string message, string cancel);
        /// <summary>
        /// Display's an Alert
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="accept"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        Task<bool> DisplayAlert(string title, string message, string accept, string cancel);
        /// <summary>
        /// Display's an Action Sheet
        /// </summary>
        /// <param name="title"></param>
        /// <param name="cancel"></param>
        /// <param name="destruction"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons);
        /// <summary>
        /// Display's a Prompt to the user
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="accept"></param>
        /// <param name="cancel"></param>
        /// <param name="placeholder"></param>
        /// <param name="maxLength"></param>
        /// <param name="keyboard"></param>
        /// <param name="initialValue"></param>
        /// <returns></returns>
        Task<string> DisplayPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancel", string placeholder = null, int maxLength = -1, Keyboard keyboard = default(Keyboard), string initialValue = "");
    }
}