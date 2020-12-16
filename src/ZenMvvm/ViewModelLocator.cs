using System;
using System.Globalization;
using System.Reflection;
using Xamarin.Forms;
using ZenMvvm.Helpers;

namespace ZenMvvm
{
    /// <summary>
    /// Enables a View to find and attach its corresponding ViewModel
    /// </summary>
    public static class ViewModelLocator
    {
        /// <summary>
        /// ViewModelLocator settings
        /// </summary>
        internal static readonly ViewModelLocatorSettings settings = new ViewModelLocatorSettings(); //field initialized before ctor

        #region UnitTesting Helper
        //private static IIoc DefaultContainer() => new SmartDi2IIocAdapter();
        ///// <summary>
        ///// Overrides <see cref="Ioc"/> with the chosen <see cref="IIoc"/>
        ///// </summary>
        //internal static IIoc ContainerImplementation { private get; set; } = DefaultContainer();

        /// <summary>
        /// Dependency injection container
        /// </summary>
        public static IIoc Ioc { get; set; }//=> ContainerImplementation;

        ///// <summary>
        ///// Revert to default IIoc
        ///// </summary>
        //internal static void ResetContainerImplementationToDefault()
        //    => ContainerImplementation = DefaultContainer();
        #endregion


#region AutoWireViewModel
        /// <summary>
        /// Tells the <see cref="ViewModelLocator"/> to attach the corresponding ViewModel
        /// </summary>
        public static readonly BindableProperty AutoWireViewModelProperty =
            BindableProperty.CreateAttached("AutoWireViewModel", typeof(bool), typeof(ViewModelLocator), default(bool)
                , propertyChanged: OnWireViewModelChanged);

        /// <summary>
        /// Gets the <see cref="AutoWireViewModelProperty"/>
        /// </summary>
        /// <param name="bindable"></param>
        /// <returns></returns>
        public static bool GetAutoWireViewModel(BindableObject bindable)
        {
            return (bool)bindable.GetValue(AutoWireViewModelProperty);
        }

        /// <summary>
        /// Sets the <see cref="AutoWireViewModelProperty"/>
        /// </summary>
        /// <param name="bindable"></param>
        /// <param name="value"></param>
        public static void SetAutoWireViewModel(BindableObject bindable, bool value)
        {
            bindable.SetValue(AutoWireViewModelProperty, value);
        }

        /// <summary>
        /// Sets the <see cref="AutoWireViewModelProperty"/> to true
        /// </summary>
        /// <param name="view">The view, commonly <c>this</c></param>
        public static void AutoWireViewModel(Page view)
        {
            if (view != null)
                WireViewModel(view);
        }

#endregion

#region WireSpecificViewModel
        /// <summary>
        /// Tells the <see cref="ViewModelLocator"/> to attach the ViewModel type specified by the given name.
        /// Provide either the ViewModel's simple type name (in which case
        /// the default namespace will be assumed), or the ViewModel's full assembly
        /// qualified name.
        /// </summary>
        public static readonly BindableProperty WireSpecificViewModelProperty =
            BindableProperty.CreateAttached("WireSpecificViewModel", typeof(string), typeof(ViewModelLocator), default(string), propertyChanged: OnWireViewModelChanged);

        /// <summary>
        /// Gets the <see cref="WireSpecificViewModelProperty"/>
        /// </summary>
        /// <param name="bindable"></param>
        /// <returns></returns>
        public static string GetWireSpecificViewModel(BindableObject bindable)
            => (string)bindable.GetValue(WireSpecificViewModelProperty);

        /// <summary>
        /// Sets the <see cref="WireSpecificViewModelProperty"/>
        /// </summary>
        /// <param name="bindable"></param>
        /// <param name="value"></param>
        public static void SetWireSpecificViewModel(BindableObject bindable, string value)
            => bindable.SetValue(WireSpecificViewModelProperty, value);

        /// <summary>
        /// Sets the <paramref name="view"/>'s BindingContext to the specified <paramref name="viewModelName"/>
        /// </summary>
        /// <param name="view"></param>
        /// <param name="viewModelName">Either the ViewModel's simple type name (in which case
        /// the default namespace will be assumed), or the ViewModel's full assembly
        /// qualified name.</param>
        public static void AutoWireViewModel(Page view, string viewModelName)
            => SetWireSpecificViewModel(view, viewModelName);

#endregion

        /// <summary>
        /// PropertyChanged delegate that Wires ViewModel to the View
        /// </summary>
        /// <param name="bindable">The view</param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        private static void OnWireViewModelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is Element view))
                return;

            if(newValue is bool boolean && boolean == true)
                WireViewModel(view);

            if(newValue is string @string)
                WireViewModel(view, @string);
        }

        internal static void WireViewModel(Element view, string viewModelName = null)
        {
            var viewType = view.GetType();

            var viewModelType = GetViewModelTypeForPage(viewType, viewModelName);

            if (viewModelType is null)
                throw new ViewModelBindingException(viewType);

            var viewModel = Ioc.Resolve(viewModelType);

            if (viewModel is null) //unlikely to be true
                throw new ViewModelBindingException(viewType);

            if (view is Page page)
            {
                if (viewModel is IOnViewAppearing appearingVm)
                    page.Appearing += new WeakEventHandler<EventArgs>(
                        appearingVm.OnViewAppearing).HandlerOnMainThread;

                if (viewModel is IOnViewDisappearing disappearingVm)
                    page.Disappearing += new WeakEventHandler<EventArgs>(
                        disappearingVm.OnViewDisappearing).HandlerOnMainThread;
            }

            view.BindingContext = viewModel;
        }

        private static Type GetViewModelTypeForPage(Type pageType, string viewModelName = null)
        {
            // if no name given
            // -> follow conventions to produce assemblyqualifiedname
            if (string.IsNullOrEmpty(viewModelName))
            {
                viewModelName = (pageType.IsGenericType
                    ? pageType
                    .GetGenericTypeDefinition()
                    : pageType)
                    .Name
                    .ReplaceLastOccurrence(
                        settings.ViewSuffix,
                        settings.ViewModelSuffix);
            }

            // if name provided but not assembly qualified
            // -> use namespace conventions to produce assemblyqualifiedname
            if (!viewModelName.Contains(","))
                viewModelName = string.Format(CultureInfo.InvariantCulture
                , "{0}.{1}, {2}"
                , settings.ViewModelNamespace ??
                    pageType.Namespace
                    .Replace(settings.ViewSubNamespace, settings.ViewModelSubNamespace)
                , viewModelName
                , settings.ViewModelAssemblyName ?? pageType.GetTypeInfo().Assembly.FullName);

            var viewModelType = Type.
                    GetType(viewModelName);

            //todo test if still works when generic args are generic in themselves
            return pageType.IsGenericType
                    ? viewModelType.
                        MakeGenericType(pageType.GetGenericArguments())
                    : viewModelType;
        }
    }
}
