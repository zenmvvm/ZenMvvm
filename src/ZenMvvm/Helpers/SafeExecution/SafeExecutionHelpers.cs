using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using Xamarin.Forms;

// Adapted from Brandon Minnick's AsyncAwaitBestPractices
// https://github.com/brminnick/AsyncAwaitBestPractices, 
// which in turn was inspired by John Thiriet's blog post,
//"Removing Async Void": https://johnthiriet.com/removing-async-void/

namespace ZenMvvm.Helpers
{
    /// <summary>
    /// Configurable Exception Handler for the collection of safe helpers
    /// : <see cref="SafeCommand"/>,
    /// <see cref="SafeTaskExtensions"/> and <see cref="SafeActionExtensions"/>
    /// </summary>
    public sealed class SafeExecutionHelpers : ISafeExecutionHelpers
    {
        #region For Unit Testing
        internal SafeExecutionHelpers()
        {
            //Have to use field initializers
        }

        private static readonly Lazy<ISafeExecutionHelpers> defaultImplementation = new Lazy<ISafeExecutionHelpers>(() => new SafeExecutionHelpers());
        /// <summary>
        /// Singleton implementation which may be over-ridden by internal test class for mocking purposes
        /// </summary>        
        internal static ISafeExecutionHelpers Implementation { get; set; }
            = defaultImplementation.Value;

        /// <summary>
        /// Reverts <see cref="Implementation"/> to its default
        /// </summary>
        internal static void RevertToDefaultImplementation()
            => Implementation = defaultImplementation.Value;

        #endregion

        /// <summary>
        /// Configurable Settings for the <see cref="SafeExecutionHelpers"/> class.
        /// </summary>
        internal static ISafeExecutionSettings Settings => Implementation.Settings;
        ISafeExecutionSettings ISafeExecutionHelpers.Settings { get; set; }
            = SafeExecutionSettings.Default;

        /// <summary>
        /// Configure <see cref="SafeExecutionHelpers"/> with the given
        ///  <paramref name="settings"/>
        /// </summary>
        /// <param name="settings">The <see cref="ISafeExecutionSettings"/></param>
        public static void Configure(ISafeExecutionSettings settings)
            => Implementation.Configure(settings);
        void ISafeExecutionHelpers.Configure(ISafeExecutionSettings settings)
        {
            Implementation.Settings = settings;
        }

        /// <summary>
        /// Configure <see cref="SafeExecutionHelpers"/> with the given
        /// <paramref name="configureSettings"/>
        /// </summary>
        /// <param name="configureSettings">Action which selectively configures <see cref="ISafeExecutionSettings"/></param>
        public static void Configure(Action<ISafeExecutionSettings> configureSettings)
            => Implementation.Configure(configureSettings);
        void ISafeExecutionHelpers.Configure(Action<ISafeExecutionSettings> configureSettings)
        {
            configureSettings(Settings);
        }


        /// <summary>
        /// Removes the <see cref="ISafeExecutionSettings.DefaultExceptionHandler"/>
        /// </summary>
        public static void RemoveDefaultExceptionHandler()
            => Implementation.RemoveDefaultExceptionHandler();
        void ISafeExecutionHelpers.RemoveDefaultExceptionHandler()
            => Settings.DefaultExceptionHandler = null;

        /// <summary>
        /// Set the <see cref="ISafeExecutionSettings.DefaultExceptionHandler"/> to the given <paramref name="onException"/>
        /// </summary>
        /// <param name="onException">The default exception handler</param>
        public static void SetDefaultExceptionHandler(Action<Exception> onException)
            => Implementation.SetDefaultExceptionHandler(onException);
        void ISafeExecutionHelpers.SetDefaultExceptionHandler(Action<Exception> onException)
        {
            if (onException is null)
                throw new ArgumentNullException(nameof(onException));

            Settings.DefaultExceptionHandler = onException;
        }

        /// <summary>
        /// Invoke the given <paramref name="onException"/> callback if
        /// exception is of type <typeparamref name="TException"/>.
        /// If <paramref name="onException"/> is not executed, the
        /// <see cref="ISafeExecutionSettings.DefaultExceptionHandler"/> will be called
        /// if it was Initialized
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="exception">the exception</param>
        /// <param name="onException">the exception handler</param>
        public static void HandleException<TException>(Exception exception, Action<TException> onException)
            where TException : Exception
            => Implementation.HandleException(exception, onException);
        void ISafeExecutionHelpers.HandleException<TException>(Exception exception, Action<TException> onException)
        {
            if (exception is InvalidCommandParameterException)
                throw exception; //internal exception from SafeCommand


            // One of the following
            if (onException != null && exception is TException)
                onException.Invoke(exception as TException);

            else if (Settings.GenericExceptionHandlers.TryGet(exception.GetType(), out Action<Exception> genericHandler))
                genericHandler.Invoke(exception);

            else if (Settings.DefaultExceptionHandler != null)
                Settings.DefaultExceptionHandler.Invoke(exception);

            else
                Device.BeginInvokeOnMainThread(()=>throw new SafeExecutionHelpersException(exception));


            if (Settings.ShouldAlwaysRethrowException)
                Device.BeginInvokeOnMainThread(() => throw exception);
        }

        /// <summary>
        /// Invoke the 
        /// <see cref="ISafeExecutionSettings.DefaultExceptionHandler"/> if it was Initialized
        /// </summary>
        public static void HandleException(Exception exception)
            => Implementation.HandleException(exception);
        void ISafeExecutionHelpers.HandleException(Exception exception)
            => Implementation.HandleException<Exception>(exception, null);
    }

    /// <summary>
    /// Configurable Settings for the <see cref="SafeExecutionHelpers"/>
    /// </summary>
    public interface ISafeExecutionSettings
    {
        /// <summary>
        /// Once handled, the exception will always be re-thrown.
        /// Use for debugging purposes. Default value is false. 
        /// </summary>
        bool ShouldAlwaysRethrowException { get; set; }

        /// <summary>
        /// A collection of default exception handlers for specific exception types
        /// </summary>
        ExceptionHandlerDictionary GenericExceptionHandlers { get; set; }

        /// <summary>
        /// The fallback default exception handler.
        /// Commonly this will log the exception or write to console.
        /// </summary>
        Action<Exception> DefaultExceptionHandler { get; set; }
    }

    ///<inheritdoc/>
    public class SafeExecutionSettings : ISafeExecutionSettings
    {
        private static readonly Lazy<SafeExecutionSettings> defaultSettings =
            new Lazy<SafeExecutionSettings>(() => new SafeExecutionSettings());

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeExecutionSettings"/> class.
        /// </summary>
        public SafeExecutionSettings()
        {
            ShouldAlwaysRethrowException = false;
        }

        /// <summary>
        /// Returns the default settings
        /// </summary>
        public static ISafeExecutionSettings Default => defaultSettings.Value;

        ///<inheritdoc/>
        public bool ShouldAlwaysRethrowException { get; set; }

        ///<inheritdoc/>
        public Action<Exception> DefaultExceptionHandler { get; set; }

        ///<inheritdoc/>
        public ExceptionHandlerDictionary GenericExceptionHandlers { get; set; } = new ExceptionHandlerDictionary();
    }

    public class ExceptionHandlerDictionary
    {
        private readonly ConcurrentDictionary<Type, Action<Exception>> dictionary = new ConcurrentDictionary<Type, Action<Exception>>();

        public void Add<T>(Action<T> action) where T : Exception
            => dictionary.TryAdd(typeof(T), new Action<Exception>(o=>action((T)o)));

        public void Remove<T>() where T : Exception
            => dictionary.TryRemove(typeof(T), out _);

        public bool TryGet<T>(Type exceptionType, out Action<T> handler) where T : Exception
        {
            if (dictionary.TryGetValue(exceptionType, out Action<Exception> value))
            {
                handler = value;
                return true;
            }
            else
            {
                handler = null;
                return false;
            }
        }
    }

    public class SafeExecutionHelpersException : Exception
    {
        public SafeExecutionHelpersException(Exception exception) : base($"Uncaught exception, '{exception.GetType().Name}'. Add the appropriate handler, or set a DefaultExceptionHandler to catch all unhandled exceptions ({nameof(SafeExecutionHelpers)}.{nameof(SafeExecutionHelpers.SetDefaultExceptionHandler)} method)",exception)
        {
        }
    }
}
