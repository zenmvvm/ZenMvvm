using System;
namespace ZenMvvm
{
    /// <summary>
    /// Configurable settings for the NavigationService
    /// </summary>
    public class NavigationSettings
    {
        private static readonly Lazy<NavigationSettings> DefaultOptions =
            new Lazy<NavigationSettings>(() => new NavigationSettings());

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationSettings"/> class.
        /// </summary>
        public NavigationSettings()
        {
            ShouldResolveViews = false;
        }

        /// <summary>
        /// Gets the default <see cref="NavigationSettings"/>
        /// </summary>
        public static NavigationSettings Default => DefaultOptions.Value;

        /// <summary>
        /// Determines whether Views should be resolved from the dependency injection container
        /// when navigating with the PushAsync and PushModalAsync methods
        /// </summary>
        /// <remarks>Default is false</remarks>
        public bool ShouldResolveViews { get; set; }

    }
}
