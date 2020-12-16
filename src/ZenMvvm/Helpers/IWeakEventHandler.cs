using System;

namespace ZenMvvm.Helpers
{
    /// <summary>
    /// Weakly subscribe to events
    /// </summary>
    public interface IWeakEventHandler<TEventArgs> where TEventArgs : EventArgs
    {
        /// <summary>
        /// Invokes the callback
        /// </summary>
        void Handler(object sender, TEventArgs e);
    }
}