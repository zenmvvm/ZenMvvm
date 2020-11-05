using System;
using System.ComponentModel;
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
        #region UnitTesting Helper
        static readonly IIoc defaultContainerImplementation = new SmartDi2IIocAdapter();
        /// <summary>
        /// Overrides <see cref="Ioc"/> with the chosen <see cref="IIoc"/>
        /// </summary>
        public static IIoc ContainerImplementation { private get; set; } = defaultContainerImplementation;

        /// <summary>
        /// Dependency injection container
        /// </summary>
        static IIoc Ioc => ContainerImplementation;

        /// <summary>
        /// Revert to default IIoc
        /// </summary>
        internal static void SetDefaultContainerImplementation()
            => ContainerImplementation = defaultContainerImplementation;
        #endregion

        /// <summary>
        /// Customises configuration
        /// </summary>
        /// <returns></returns>
        public static ConfigOptions Configure()
        {
            return new ConfigOptions();
        }
        #region AutoWireViewModel
        /// <summary>
        /// Tells the <see cref="ViewModelLocator"/> to attach the corresponding ViewModel
        /// </summary>
        public static readonly BindableProperty AutoWireViewModelProperty =
            BindableProperty.CreateAttached("AutoWireViewModel", typeof(bool), typeof(ViewModelLocator), default(bool)
                , propertyChanged: OnAutoWireViewModelChanged);

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

        /// <summary>
        /// Wires ViewModel if the <see cref="AutoWireViewModelProperty"/> is true
        /// </summary>
        private static void OnAutoWireViewModelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is Element view && (bool)newValue == true))
                return;

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
            BindableProperty.CreateAttached("WireSpecificViewModel", typeof(string), typeof(ViewModelLocator), default(string), propertyChanged: OnWireSpecificViewModelChanged);

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

        private static void OnWireSpecificViewModelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is Element view && !string.IsNullOrEmpty((string)newValue)))
                return;

            WireViewModel(view, (string)newValue);
        }


        #endregion

        //todo OnWire.... to rule them all

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
            if(string.IsNullOrEmpty(viewModelName))
                viewModelName = pageType.Name.ReplaceLastOccurrence(
                            NamingConventions.ViewSuffix, NamingConventions.ViewModelSuffix);

            // if name provided but not assembly qualified
            // -> use namespace conventions to produce assemblyqualifiedname
            if (!viewModelName.Contains(","))
                viewModelName = string.Format(CultureInfo.InvariantCulture
                , "{0}.{1}, {2}"
                , NamingConventions.ViewModelNamespace ??
                    pageType.Namespace
                    .Replace(NamingConventions.ViewSubNamespace, NamingConventions.ViewModelSubNamespace)
                , viewModelName
                , NamingConventions.ViewModelAssemblyName ?? pageType.GetTypeInfo().Assembly.FullName);

            return Type.GetType(viewModelName);
        }
    }

    //todo hide with internal?
    /// <summary>
    /// Plumbing for Fluent Api
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class ConfigOptions
    {
        /// <summary>
        /// Plumbing for Fluent Api
        /// </summary>
        public ConfigOptions SetViewSuffix(string suffix)
        {
            NamingConventions.ViewSuffix = suffix;
            return this;
        }

        /// <summary>
        /// Plumbing for Fluent Api
        /// </summary>
        public ConfigOptions SetViewModelSuffix(string suffix)
        {
            NamingConventions.ViewModelSuffix = suffix;
            return this;
        }

        /// <summary>
        /// Plumbing for Fluent Api
        /// </summary>
        public ConfigOptions SetViewAssemblyQualifiedNamespace<TAnyView>()
        {
            SetViewAssemblyQualifiedNamespace(
                typeof(TAnyView).Namespace,
                typeof(TAnyView).Assembly.FullName);
            return this;
        }

        /// <summary>
        /// Plumbing for Fluent Api
        /// </summary>
        public ConfigOptions SetViewAssemblyQualifiedNamespace(string namespaceName, string assemblyName)
        {
            NamingConventions.ViewAssemblyName = assemblyName;
            NamingConventions.ViewNamespace = namespaceName;
            return this;
        }

        /// <summary>
        /// Plumbing for Fluent Api
        /// </summary>
        public ConfigOptions SetViewModelAssemblyQualifiedNamespace<TAnyViewModel>()
        {
            SetViewModelAssemblyQualifiedNamespace(
                typeof(TAnyViewModel).Namespace,
                typeof(TAnyViewModel).Assembly.FullName);
            return this;
        }

        /// <summary>
        /// Plumbing for Fluent Api
        /// </summary>
        public ConfigOptions SetViewModelAssemblyQualifiedNamespace(string namespaceName, string assemblyName)
        {
            NamingConventions.ViewModelAssemblyName = assemblyName;
            NamingConventions.ViewModelNamespace = namespaceName;
            return this;
        }
    }

}
