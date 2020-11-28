namespace ZenMvvm.Helpers
{
    /// <summary>
    /// Commonly used Properties in ViewModel that Raise PropertyChanged event
    /// </summary>
    public interface IViewModelBase : IObservableObject, IBusy
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>The icon.</value>
        string Icon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is not busy.
        /// </summary>
        /// <value><c>true</c> if this instance is not busy; otherwise, <c>false</c>.</value>
        bool IsNotBusy { get; set; }
    }
}