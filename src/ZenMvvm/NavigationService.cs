using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;
using ZenMvvm.Helpers;

//todo
//Ensure BIOMT first checks if on Main
//SafeBeginInvokeOnMainThread
//Make ExtensionClasses mockable
//Helpers in Separate Namespace
//documentation for IoC
//acknowledgements in Licence
//clean old unused code
//Autofac adaptor
namespace ZenMvvm
{
    /// <summary>
    /// Controlls Page Navigation from the ViewModel
    /// </summary>
    public class NavigationService : INavigationService
    {
        /// <summary>
        /// Returns Xamarin.Forms.Shell.Current
        /// </summary>
        public Shell CurrentShell => Shell.Current;

        //todo be more elegant
        INavigation navigation;
        INavigation Navigation => navigation ?? CurrentShell.Navigation;

        /// <summary>
        /// Default Constructor
        /// </summary>
        [ResolveUsing]
        public NavigationService()
        {
            //navigation = Shell.Current.Navigation;
        }

        /// <summary>
        /// Constructor for unit testing
        /// </summary>
        /// <param name="navigation"></param>
        public NavigationService(INavigation navigation)
        {
            this.navigation = navigation;
        }

        /// <summary>
        /// Gets the Current Shell's NavigationStack
        /// </summary>
        public IReadOnlyList<Page> NavigationStack => Navigation.NavigationStack;
        /// <summary>
        /// Gets the Current Shell's ModalStack
        /// </summary>
        public IReadOnlyList<Page> ModalStack => Navigation.ModalStack;

        #region Avoid dependancy on Xamarin.Forms
        /// <summary>
        /// Returns a singleton instance of the MessagingCenter
        /// </summary>
        public ISafeMessagingCenter SafeMessagingCenter => Helpers.SafeMessagingCenter.Instance;

        /// <summary>
        /// Invokes an Action on the device's main (UI) thread.
        /// Wrapper of Xamarin.Forms <see cref="Device.BeginInvokeOnMainThread(Action)"/>
        /// </summary>
        public void BeginInvokeOnMainThread(Action action)
            => Device.BeginInvokeOnMainThread(action);

        #endregion
        #region CONSTRUCTIVE
        //todo should also return viewModel
        /// <summary>
        /// Navigates to a <see cref="Page"/>
        /// </summary>
        /// <param name="state">A URI representing either the current page or a destination for navigation in a Shell application.</param>
        /// <param name="animate"></param>
        /// <returns></returns>
        public Task GoToAsync(ShellNavigationState state, bool animate = true)
        {
            return InternalGoToAsync(state, animate);            
        }

        /// <summary>
        /// Navigates to a <see cref="Page"/> passing data to the target ViewModel
        /// </summary>
        /// <param name="state"></param>
        /// <param name="navigationData"></param>
        /// <param name="animate"></param>
        /// <returns></returns>
        public async Task GoToAsync<TData>(ShellNavigationState state, TData navigationData, bool animate = true)
        {
            var viewModel = await InternalGoToAsync(state, animate).ConfigureAwait(false);
            await RunOnNavigatedsWithDataAsync<TData>(viewModel, navigationData).ConfigureAwait(false);
        }

        private Task<object> InternalGoToAsync(ShellNavigationState state, bool animate = true)
        {
            var isPushed = new TaskCompletionSource<object>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Shell.Current.GoToAsync(state, animate).ConfigureAwait(false);
                    var viewModel = (Shell.Current?.CurrentItem?.CurrentItem as IShellSectionController)?
                        .PresentedPage
                        .BindingContext;
                    await RunOnNavigatedsAsync(viewModel);
                    isPushed.SetResult(viewModel);
                }
                catch (Exception ex)
                {
                    isPushed.SetException(ex);
                }
            });

            return isPushed.Task;
        }

        private async Task RunOnNavigatedsAsync(object viewModel)
        {
            if (viewModel is IOnViewNavigatedAsync)
                await (viewModel as IOnViewNavigatedAsync).OnViewNavigatedAsync().ConfigureAwait(false);
            if (viewModel is IOnViewNavigated)
                (viewModel as IOnViewNavigated).OnViewNavigated();

        }

        private async Task RunOnNavigatedsWithDataAsync<T>(object viewModel, T data)
        {
            //data can be null

            if (viewModel is not IOnViewNavigatedGroup<T>)
                throw new ArgumentException($"You are trying to pass {nameof(T)}"
                    + $" to a ViewModel that doesn't implement {nameof(IOnViewNavigatedAsync<T>)}");

            if (viewModel is IOnViewNavigatedAsync<T>)
                await (viewModel as IOnViewNavigatedAsync<T>).OnViewNavigatedAsync(data).ConfigureAwait(false);
            if (viewModel is IOnViewNavigated<T>)
                (viewModel as IOnViewNavigated<T>).OnViewNavigated(data);
        }

        /// <summary>
        /// Navigates to a <see cref="Page"/> passing data to the target ViewModel
        /// </summary>
        /// <param name="state"></param>
        public Task GoToAsync(ShellNavigationState state)
        {
            return GoToAsync(state, true);
        }

        /// <summary>
        /// Pushes a <see cref="Page"/> onto the <see cref="NavigationStack"/>
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel corresponding the the Page to be Pushed</typeparam>
        /// <param name="navigationData">Object that will be recieved by the <see cref="IOnViewNavigatedAsync.OnViewNavigatedAsync(object)"/> method</param>
        /// <param name="animated"></param>
        /// <returns></returns>
        public async Task<TViewModel> PushAsync<TViewModel, TData>(
            TData navigationData, bool animated = true)
            where TViewModel : class, IOnViewNavigatedGroup<TData>
        {
            var viewModel = await InternalPushAsync<TViewModel>(animated, isModal:false).ConfigureAwait(false);
            await RunOnNavigatedsWithDataAsync<TData>(viewModel, navigationData).ConfigureAwait(false);
            return viewModel; //can be null if no viewModel resolved
        }

        /// <summary>
        /// Pushes a <see cref="Page"/> onto the <see cref="NavigationStack"/>
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel corresponding the the Page to be Pushed</typeparam>
        /// <param name="animated"></param>
        /// <returns></returns>
        public Task<TViewModel> PushAsync<TViewModel>(
            bool animated = true)
            where TViewModel : class
        {
            return InternalPushAsync<TViewModel>(animated, isModal:false); //can be null if no viewModel resolved
        }


        /// <summary>
        /// Pushes a <see cref="Page"/> onto the <see cref="ModalStack"/>
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel corresponding the the Page to be Pushed</typeparam>
        /// <param name="navigationData">Object that will be recieved by the <see cref="IOnViewNavigatedAsync.OnViewNavigatedAsync(object)"/> method</param>
        /// <param name="animated"></param>
        /// <returns></returns>
        public async Task<TViewModel> PushModalAsync<TViewModel, TData>(
            TData navigationData, bool animated = true)
            where TViewModel : class, IOnViewNavigatedGroup<TData>
        {
            var viewModel = await InternalPushAsync<TViewModel>(animated, isModal: true).ConfigureAwait(false);
            await RunOnNavigatedsWithDataAsync<TData>(viewModel, navigationData).ConfigureAwait(false);
            return viewModel;
        }

        /// <summary>
        /// Pushes a <see cref="Page"/> onto the <see cref="ModalStack"/>
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel corresponding the the Page to be Pushed</typeparam>
        /// <param name="animated"></param>
        /// <returns></returns>
        public Task<TViewModel> PushModalAsync<TViewModel>(
            bool animated = true)
            where TViewModel : class
        {
            return InternalPushAsync<TViewModel>(animated, isModal: true);
        }

        private Task<TViewModel> InternalPushAsync<TViewModel>(
            bool animated,
            bool isModal)
            where TViewModel : class
        {
            var page = Activator.CreateInstance(
                    GetPageTypeForViewModel<TViewModel>()) as Page;

            var isPushed = new TaskCompletionSource<TViewModel>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (isModal)
                        await Navigation.PushModalAsync(
                            page, animated).ConfigureAwait(false);
                    else
                        await Navigation.PushAsync(page, animated).ConfigureAwait(false);

                    var viewModel = page.BindingContext as TViewModel;

                    await RunOnNavigatedsAsync(viewModel).ConfigureAwait(false);
                    isPushed.SetResult(viewModel);
                }
                catch (Exception ex)
                {
                    isPushed.SetException(ex);
                }
            });

            return isPushed.Task;
        }

        private Type GetPageTypeForViewModel<TViewModel>()
        {
            return GetPageTypeForViewModel(typeof(TViewModel));
        }

        private Type GetPageTypeForViewModel(Type viewModelType)
        {
            var name =
                viewModelType.IsInterface && viewModelType.Name.StartsWith("I")
                ? viewModelType.Name.Substring(1).ReplaceLastOccurrence(
                            NamingConventions.ViewModelSuffix, NamingConventions.ViewSuffix)
                : viewModelType.Name.ReplaceLastOccurrence(
                            NamingConventions.ViewModelSuffix, NamingConventions.ViewSuffix);

            var viewAssemblyName = string.Format(CultureInfo.InvariantCulture
                , "{0}.{1}, {2}"
                , NamingConventions.ViewNamespace ??
                    viewModelType.Namespace
                    .Replace(NamingConventions.ViewModelSubNamespace, NamingConventions.ViewSubNamespace)
                , name
                , NamingConventions.ViewAssemblyName ?? viewModelType.GetTypeInfo().Assembly.FullName);

            return Type.GetType(viewAssemblyName);
        }


        #endregion

        #region DESTRUCTIVE
        /// <summary>
        /// Removes the <see cref="Page"/> underneath the Top Page in the <see cref="NavigationStack"/>
        /// </summary>
        /// <returns></returns>
        public async Task RemovePreviousPageFromMainStack()
        {
            var viewModel = NavigationStack.GetPreviousViewModel();

            if (NavigationStack.Count > 1)
            {
                var isRemoved = new TaskCompletionSource<bool>();
                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        Navigation.RemovePage(
                                NavigationStack.GetPreviousPage());
                        isRemoved.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        isRemoved.SetException(ex);
                    }
                });

                if (await isRemoved.Task.ConfigureAwait(false) && viewModel is IOnViewRemoved removedViewModel)
                    await removedViewModel.OnViewRemovedAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Removes specific <see cref="Page"/> from the <see cref="NavigationStack"/>
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel corresponding the the Page to be Removed</typeparam>
        /// <returns></returns>
        public async Task RemovePageFor<TViewModel>()
        {
            var pageType = GetPageTypeForViewModel(typeof(TViewModel));

            foreach (var item in NavigationStack)
            {
                if (item?.GetType() == pageType)
                {
                    var isRemoved = new TaskCompletionSource<bool>();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            Navigation.RemovePage(item);
                            isRemoved.SetResult(true);
                        }
                        catch (Exception ex)
                        {
                            isRemoved.SetException(ex);
                        }
                    });

                    if (await isRemoved.Task.ConfigureAwait(false)
                        && item.BindingContext is IOnViewRemoved viewModel)
                        await viewModel.OnViewRemovedAsync().ConfigureAwait(false);

                    break;
                }
            }
        }

        /// <summary>
        /// Pops a <see cref="Page"/> off the <see cref="NavigationStack"/>
        /// </summary>
        /// <param name="animated"></param>
        /// <returns></returns>
        public async Task PopAsync(bool animated = true)
        {
            var viewModel = NavigationStack.GetCurrentViewModel();

            var isPopped = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Navigation.PopAsync(animated).ConfigureAwait(false);
                    isPopped.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPopped.SetException(ex);
                }
            });

            if (await isPopped.Task.ConfigureAwait(false)
                && viewModel is IOnViewRemoved removedViewModel)
                await removedViewModel.OnViewRemovedAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Pops all pages <see cref="Page"/> off the <see cref="NavigationStack"/>, leaving only the Root Page
        /// </summary>
        /// <param name="animated"></param>
        /// <returns></returns>
        public async Task PopToRootAsync(bool animated = true)
        {
            for (int i = Navigation.NavigationStack.Count - 1; i > 0; i--)
            {
                await PopAsync(animated).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Pops a <see cref="Page"/> off the <see cref="ModalStack"/>
        /// </summary>
        /// <param name="animated"></param>
        /// <returns></returns>
        public async Task PopModalAsync(bool animated = true)
        {
            if (!ModalStack.Any())
                throw new InvalidOperationException("Modal Stack is Empty");

            var viewModel = ModalStack.GetCurrentViewModel();

            var isPopped = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Navigation.PopModalAsync(animated);
                    isPopped.SetResult(true);
                }
                catch (Exception ex)
                {
                    isPopped.SetException(ex);
                }
            });

            if (await isPopped.Task.ConfigureAwait(false)
                && viewModel is IOnViewRemoved removedViewModel)
                await removedViewModel.OnViewRemovedAsync().ConfigureAwait(false);
        }
        #endregion

        #region Dialogues/Popups
        /// <summary>
        /// Display's an Alert
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        public Task DisplayAlert(string title, string message, string cancel)
        {
            return DisplayAlert(title, message, null, cancel);
        }

        /// <summary>
        /// Display's an Alert
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="accept"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        public Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
        {
            var hasDisplayed = new TaskCompletionSource<Task<bool>>();
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    hasDisplayed.SetResult(CurrentShell.DisplayAlert(title, message, accept, cancel));
                }
                catch (Exception ex)
                {
                    hasDisplayed.SetException(ex);
                }
            });

            return hasDisplayed.Task.Result;
        }

        /// <summary>
        /// Display's an Action Sheet
        /// </summary>
        /// <param name="title"></param>
        /// <param name="cancel"></param>
        /// <param name="destruction"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        public Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
        {
            var hasDisplayed = new TaskCompletionSource<Task<string>>();
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    hasDisplayed.SetResult(CurrentShell.DisplayActionSheet(title, cancel, destruction, buttons));
                }
                catch (Exception ex)
                {
                    hasDisplayed.SetException(ex);
                }
            });

            return hasDisplayed.Task.Result;
        }

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
        public Task<string> DisplayPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancel", string placeholder = null, int maxLength = -1, Keyboard keyboard = default, string initialValue = "")
        {
            var hasDisplayed = new TaskCompletionSource<Task<string>>();
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    hasDisplayed.SetResult(CurrentShell.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength, keyboard, initialValue));
                }
                catch (Exception ex)
                {
                    hasDisplayed.SetException(ex);
                }
            });

            return hasDisplayed.Task.Result;
        }
        #endregion

    }
}
