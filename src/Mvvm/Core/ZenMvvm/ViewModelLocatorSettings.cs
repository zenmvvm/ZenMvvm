using System;
namespace ZenMvvm
{
    /// <summary>
    /// Configurable Settings for the ViewModelLocator
    /// </summary>
    public class ViewModelLocatorSettings
    {
        private static readonly Lazy<ViewModelLocatorSettings> DefaultOptions =
            new Lazy<ViewModelLocatorSettings>(() => new ViewModelLocatorSettings());

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelLocatorSettings"/> class.
        /// </summary>
        public ViewModelLocatorSettings()
        {
            ViewSuffix = "Page";
            ViewModelSuffix = "ViewModel";
            ViewSubNamespace = "Views";
            ViewModelSubNamespace = "ViewModels";
        }

        /// <summary>
        /// Gets the default <see cref="NavigationSettings"/>
        /// </summary>
        public static ViewModelLocatorSettings Default => DefaultOptions.Value;

        #region SETTINGS

        /// <summary>
        /// Suffix used to identify a View. For example "Page" from MainPage.
        /// </summary>
        /// <remarks>Default is Page</remarks>
        public string ViewSuffix { get; set; }
        /// <summary>
        /// Suffix used to identify a ViewModel.
        /// For example "ViewModel" from MainViewModel
        /// </summary>
        /// <remarks>Default is ViewModel</remarks>
        public string ViewModelSuffix { get; set; }
        // derived from ViewNamespace
        internal string ViewSubNamespace { get; set; }
        // derived from ViewModelNamespace
        internal string ViewModelSubNamespace { get; set; }
        /// <summary>
        /// Assembly name where the Views reside
        /// </summary>
        /// <remarks>Default is the currently executing assembly</remarks>
        public string ViewAssemblyName { get; set; }
        /// <summary>
        /// Assembly name where the ViewModels reside
        /// </summary>
        /// <remarks>Default is the currently executing assembly</remarks>
        public string ViewModelAssemblyName { get; set; }

        string viewNamespace;
        /// <summary>
        /// Namespace where the Views are located
        /// </summary>
        /// <remarks>By default ZenMvvm locates Views relative to the ViewModel namespace.
        /// Both are assumed to share the same root namespace. If setting ViewNamespace,
        /// use the full namespace.</remarks>
        public string ViewNamespace
        {
            get => viewNamespace;
            set
            {
                viewNamespace = value;
                ViewSubNamespace = GetSubNamespace(value);
            }
        }
        string viewModelNamespace;
        /// <summary>
        /// Namespace where the Views are located
        /// </summary>
        /// <remarks>By default ZenMvvm locates Views relative to the ViewModel namespace.
        /// Both are assumed to share the same root namespace. If setting the ViewModelNamespace,
        ///  use the full namespace.</remarks>
        public string ViewModelNamespace
        {
            get => viewModelNamespace;
            set
            {
                viewModelNamespace = value;
                ViewModelSubNamespace = GetSubNamespace(value);
            }
        }

        #endregion

        private string GetSubNamespace(string @namespace)
            =>@namespace.Substring(@namespace.LastIndexOf(".") + 1);
        
    }
}
