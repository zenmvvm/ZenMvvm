# ![Logo](src/ZenMvvm/Art/icon.png) ZenMVVM
Lightweight **ViewModel-First** MVVM framework for Xamarin.Forms

![Coverage](https://raw.githubusercontent.com/zenmvvm/ZenMvvm/master/coverage/badge_linecoverage.svg)

*See the [ZenMvvm Sample App](https://github.com/z33bs/ZenMvvm-Sample-App#readme) and [Zenimals Sample App](https://github.com/z33bs/Zenimals-Sample-App#readme) for ZenMvvm in action*



#### **Contents**

- [Why ZenMvvm?](#Why-ZenMvvm?)
- [Mvvm Quickstart](#Mvvm-Quickstart) 
- [Dependency Injection QuickStart](#Dependency-Injection-QuickStart)
- [Helpers Quickstart](#Helpers-Quickstart)
  - [Safe Execution](#Safe-Execution)
  - [ViewModelBase](#ViewModelBase)
  - [ObservableRangeCollection](#ObservableRangeCollection)
  - [Grouping](#Grouping)
  - [WeakEvents](#WeakEvents)
- [Unit Testing Quickstart](#Unit-Testing-Quickstart)



## Why ZenMvvm?

**More readable and maintainable code**

Xamarin has Mvvm functionality, however it uses View-First navigation and doesn't incorporate dependency injection. With **ZenMvvm** you can use a ViewModel-First pattern while keeping the power and familiarity of Xamarin Forms. ViewModel-First lets you place all functional code in the ViewModels, leaving your `.xaml.cs` code-behind files empty. The result? More readable and maintainable code, and a stronger separation of concerns.

**Minimal overhead**

ZenMVVM is lightweight because it uses Xamarin's own Mvvm engine to achieve ViewModel-First navigation.

**Reduce boilerplate code**

Optional `ZenMvvm.Helpers` save you from writing boilerplate code. The SafeExecution Helpers rewrite `Xamarin.Forms.Command` and `Xamarin.Forms.MessagingCenter` so that they always handle exceptions and remove the need to write try-catch blocks and default exception handling. Similar helpers are provided for invoking Actions, and executing Tasks safely. Other commonly used helpers are bundled-in for convenience.

**Improve performance**

`SafeCommand` ensures that the `ICommand` never blocks the UI thread (unless told to). This also removes the need for repeated `.ConfigureAwait(false)` on Tasks.

**Easier Unit Testing**

ZenMvvm is built with ViewModel Unit Testing in mind. Your ViewModels won't depend on Xamarin.Forms and their components will be easy to mock / stub. 



**Features:**

* Embraces Xamarin.Forms Shell applications
* Uses familiar syntax, e.g. `PushAsync<TViewModel>()` is similar to Xamarin's `PushAsync(Page page)`. You can also use route navigation with `GotoAsync`.
* Effortlessly pass data to the pushed view-model with `PushAsync<TViewModel>(object navigationData)` and `GotoAsync(route, navigationData)`
* Navigation events can be handled in the ViewModel with `OnViewAppearing`, `OnViewDisappearing`, `OnViewNavigated`, and `OnViewRemoved` methods.
* Provides fast built-in dependency injection. Alternatively, the user can elect to run ZenMvvm with their own DI engine of choice (e.g. Autofac or LightInject).
* SafeExecution Helpers reduce boilerplate code and ensure no unhandled exceptions break your app, while enhancing performance by ensuring non-UI tasks are always run on background threads.



## Mvvm Quickstart

The easiest way to get familiar with ZenMvvm is to checkout:

* [ZenMvvm Sample App](https://github.com/z33bs/ZenMvvm-Sample-App#readme), which is simply a refactored version of Xamarin's "Shell Forms App" template, and
* [Zenimals](https://github.com/z33bs/Zenimals-Sample-App#readme), which is a refactored version of Xamarin's Xaminals Sample App.

This QuickStart references code from the above apps.



### Decide on your naming conventions

For convenience, ZenMvvm works by assuming that you name your View and ViewModel classes <u>consistently</u>. You can change the expected naming convention to suit your personal style with the `ViewModelLocator.Configure()` method. You can also break the convention [if needed](#Tell-Pages-to-Wire-their-respective-ViewModels).

The default expectations are:

* Views and ViewModels are in the same assembly. Although custom configuration does allow for them to reside in different assemblies.
* All views are in the `.Views` sub-namespace, and view-models in the `.ViewModels` sub-namespace. For example: MainPage will be in the `MyApp.Views` namespace, and MainViewModel will be in the `MyApp.ViewModels` namespace.
* Views end with the <u>suffix 'Page'</u>, and view-models with the <u>suffix 'ViewModel'</u>. For example `MainPage` and `MainViewModel`. Apart for the suffix, the View and its corresponding ViewModel share the same name.

The SampleApp is structured as follows: 

```
SampleApp
  > Models
  > Services
  > ViewModels
    - AboutViewModel.cs
    - ItemDetailViewModel.cs
    - ...
  > Views
   - AboutPage.xaml
   - ItemDetailPage.xaml
```



### Design your Shell as you normally would

Here's an example of the Sample's  `AppShell.xaml`:

```xaml
<?xml version="1.0" encoding="UTF-8"?>
<Shell ...>
    <TabBar>
        <Tab Title="Browse" Icon="tab_feed.png">
            <ShellContent ContentTemplate="{DataTemplate local:ItemsPage}" />
        </Tab>
        <Tab Title="About" Icon="tab_about.png">
            <ShellContent ContentTemplate="{DataTemplate local:AboutPage}" />
        </Tab>
    </TabBar>
</Shell>
```



### Tell Pages to Wire their respective ViewModels

If you follow the naming conventions, simple use `AutoWireViewModel` in the xaml of your view.

```xaml
<?xml version="1.0" encoding="utf-8"?>
<ContentPage
	...
  xmlns:mvvm="clr-namespace:ZenMvvm;assembly=ZenMvvm"
  mvvm:ViewModelLocator.AutoWireViewModel="True">
  
  <!-- Xaml for Page -->
  
</ContentPage>


```

If you want to bind a ViewModel that doesn't follow the naming conventions, use `WireSpecificViewModel`. 

> Using `WireSpecificViewModel`: If you just type the ViewModel's name, it will assume that the ViewModel is located in the default ViewModel namespace. If you specify an assembly qualified name, you can reference a ViewModel in any namespace.

```xaml
<?xml version="1.0" encoding="utf-8"?>
<ContentPage
	...
  xmlns:mvvm="clr-namespace:ZenMvvm;assembly=ZenMvvm"
  mvvm:ViewModelLocator.WireSpecificViewModel="SpecificViewModel">
  
  <!-- Xaml for Page -->
  
</ContentPage>

```



### Perform Navigation

The navigation syntax is exactly the same as Xamarin.Forms, with the difference being that its ViewModel first. Refer to Xamarin's documentation for more information. One can use two different styles of navigation: [routes](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/shell/navigation#routes) (`GotoAsync`),  and/or [hierarchical navigation](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/navigation/hierarchical) (`PushAsync`).

In order to Navigate, pass INavigationService to the ViewModel's constructor. This is only required if your ViewModel needs to control page navigation:

```c#
using ZenMvvm;

namespace SampleApp.ViewModels
{
    public class ItemsViewModel
    {
        readonly INavigationService navigationService;

        public ItemsViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
          	//...
        }
//...
```

Now you can...

**Push a page onto the `NavigationStack`:**

```c#
AddItemCommand = new Command(async () =>
  {
    	//...
      await navigationService.PushAsync<NewItemViewModel>();
  }
```

**Navigate using [routes](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/shell/navigation#routes):**

```c#
AddItemCommand = new Command(async () =>
  {
    	//...
      await navigationService.GoToAsync($"details");
  }
```

**Pop a page off the stack:**

```c#
CancelCommand = new Command(async () =>
  {
    	//...
      await navigationService.PopAsync();
  }
```



**Pass data while navigating:**

```c#
OnItemSelectedCommand = new Command(async () =>
  {
    	//...
      await navigationService.PushAsync<ItemDetailViewModel, Item>(item);
    	//..OR
    	await navigationService.GotoAsync<Item>($"details", item);
  }
```

The recieving ViewModel must implement `IOnViewNavigated<T>` to recieve the data:

```c#
public class ItemDetailViewModel : IOnViewNavigated<Item>
{
  	//...
  
    public void OnViewNavigated(Item item)
    {        
      //Do something with item
      //...   
    }
}
```



### Automatically Run ViewModel code when events are triggered

The following events are supported by having the ViewModel implement the relevant interface:

* `OnViewAppearing` is called when the bound view appears
* `OnViewDisappearing` is called when the bound view disappears
* `OnViewRemoved` is called when the bound view is popped from the `NavigationStack`
* `OnViewNavigated` overloads are called as soon as navigation has been completed
  * `OnViewNavigated`
  * `Task OnViewNavigatedAsync`
  * `OnViewNavigated<TData>` (example above) 
  * `Task OnViewNavigatedAsync<TData>`



## Dependency Injection QuickStart

ZenMvvm uses dependency injection to resolve ViewModels before binding to their respective views. ZenMvvm can also use dependency injection with the Views [if desired](#Injecting-dependencies-into-Views).

The built-in dependency injection engine is fast and powerful, and has been unbundled into a standalone package, [SmartDi](https://github.com/z33bs/SmartDi). Refer to it's [Readme](https://github.com/z33bs/SmartDi#readme), and [Wiki](https://github.com/z33bs/SmartDi/wiki) for detailed documentation. However, one can easily use a 3rd party engine [if prefered](#Using-3rd-Party-dependency-injection-/-IOC).

> SmartDi strikes a balance between offering rich features while still performing reasonably fast. [Benchmarks](https://danielpalme.github.io/IocPerformance/) put it solidly in the middle of the pack in terms of speed. 



The built-in DI is user friendly, allowing you to resolve dependencies without registering them (see [SmartResolve](https://github.com/z33bs/SmartDi/wiki/Resolution#smart-resolve)). This is great for rapid-prototyping and simple applications. 

>If using ZenMvvm in this way, take note of the default behaviour:
>
> - If the Resolved-Type is an interface or abstract class, it will be registered as a Singleton
> - Otherwise it will be registereed as a Transient an a new instance will be created each time the type is resolved



Consuming the dependency is as simple as:

```c#
//...
public class ItemsViewModel
{
    readonly INavigationService navigationService;
    readonly IDataStore<Item> dataStore;

    public ItemsViewModel(INavigationService navigationService, 
                          IDataStore<Item> dataStore)
    {
        this.navigationService = navigationService;
        this.dataStore = dataStore;
      	//...
    } 	
  	//...
}
```

Both the `INavigationService` and `IDataStore<Item>` classes will be resolved as Singletons.



If you were to register these dependencies, you would do the following in `App.cs`:

```c#
//...
public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        DiContainer.Register<INavigationService, NavigationService>();
        DiContainer.Register<IDataStore<Item>, MockDataStore>();
      	
      	//...
```



If you want ZenMvvm to throw a `ResolveException` when a dependency is not registered, initialize with the following configuration:
```c#
DiContainer.Initialize(o => o.TryResolveUnregistered = false);
```

### Injecting dependencies into Views

By default, ZenMvvm expects the views to have parameterless constructors. It creates a new instance of the view, and doesn't expect views to be registered as dependencies.

If you prefer to incorporate dependencies into some of your views, you will want to resolve the views using the DI engine. This can be achieved by changing the NavigationService settings. When doing so, take note of the following:

* Hierarchical navigation with the `Push` methods will resolve the pushed view using DI.
* A pattern needs to be used to ensure that views are resolved using DI for the Shell Visual Hierarchy.
* Navigating with `GotoAsync` to a route that is NOT in the Shell visual hierarchy requires special Route Registration for the View to be resolved as a dependency.

The suggested pattern is as follows...

*In your AppShell.xaml.cs*

```c#
public partial class AppShell : Shell
{
   //Bindable properties for use in the ContentTemplate of ShellContent
   // in the xaml
    public DataTemplate Tab1 { get; }
    public DataTemplate Tab2 { get; }
    public DataTemplate Tab3 { get; }

  	//Pages will be injected into AppShell
    public AppShell(
        Tab1Page tab1,
        Tab1Page tab1,
        Tab1Page tab1)
    {
       //Assign before InitializeComponent();
        Tab1 = new DataTemplate(() => tab1);
        Tab2 = new DataTemplate(() => tab2);
        Tab3 = new DataTemplate(() => tab3);

        InitializeComponent();
      
        //Use ZenMvvm's ResolverRouteFactory to register routes
        // that are not part of the Shell Visual Hierarchy
        Routing.RegisterRoute(
          "detailspage", 
          new ResolverRouteFactory<DetailPage>());
    }
```

*Now in your AppShell.xaml*

```xaml
<Shell ...>
    <TabBar>
        <Tab Title="Tab1" Icon="tab1.png">
            <ShellContent ContentTemplate="{Binding Tab1}" />
        </Tab>
        <Tab Title="Tab2" Icon="tab2.png">
            <ShellContent ContentTemplate="{Binding Tab2}" />
        </Tab>
        <Tab Title="Tab3" Icon="tab3.png">
            <ShellContent ContentTemplate="{Binding Tab3}" />
        </Tab>
    </TabBar>
</Shell>
```

*In your app.xaml.cs:*

```c#
public partial class App : Application
{
  public App()
  {
	  InitializeComponent();

  	//Register dependencies if you don't want to use Smart Resolve

  	MainPage = DiContainer.Resolve<AppShell>();
  }
}
```

All views will now be created by the DI engine. Also, all navigation with GotoAsync and PushAsync will resolve views from the DI engine.

### Using 3rd Party dependency injection / IOC

You might prefer to use a 3rd-party DI engine. ZenMvvm makes this easy by allowing you to set `ViewModelLocator.ContainerImplementation` to something that implements the `IIoc` interface. ZenMvvm provides a dynamic implementation of the Adapter pattern, the `IocAdapter` class, to assist in this regard. The code below refactors `App.cs` to use [Autofac](https://autofac.org):

```c#
//...
using Autofac;

public partial class App : Application
{
  public App()
  {
      InitializeComponent();
			//Autofac registration
      var containerBuilder = new Autofac.ContainerBuilder();
      containerBuilder.RegisterType<MockDataStore>().As<IDataStore<Item>>();
      containerBuilder.RegisterType<NavigationService>().As<INavigationService>();
			
      ViewModelLocator.ContainerImplementation = new IocAdapter(
          containerBuilder.Build(), 
          typeof(ResolutionExtensions), 
          nameof(ResolutionExtensions.Resolve));
      //...
```



And below for the [LightInject](https://www.lightinject.net) implementation:

```c#
ViewModelLocator.ContainerImplementation = new IocAdapter(
        container, 
        nameof(LightInject.ServiceContainer.GetInstance));
```



> :memo:If the  `IocAdapter` helper doesn't work for your chosen DI engine, you can write your own Adapter that implements the `IIoc` interface.
>



## Helpers Quickstart
> Gratitude goes to [James Montemagno](https://github.com/jamesmontemagno), [John Thieret](https://johnthiriet.com), [Brandon Minnick](https://github.com/brminnick), and the [Xamarin Team](https://github.com/xamarin). This collection of helpers is inspired by, and builds on their work.



`ZenMvvm.Helpers` is an integrated collection of helpers that:

* reduce boilerplate code, 
* prevent app-crashes from unhandled exceptions, 
* and improve performance by running commands on the background thread



Using the helpers, this block of code...

```c#
LoadItemsCommand = new SafeCommand(
    async () => Items.ReplaceRange(await dataStore.GetItemsAsync(true))
		, viewModel:this);
```
...is equivalent to ...
```c#
LoadItemsCommand = new Command(async () =>
{
    if (IsBusy)
        return;

    try
    {
        IsBusy = true;

        Items.Clear();
        var items = await dataStore.GetItemsAsync(true).ConfigureAwait(false);
        foreach (var item in items)
        {
            Items.Add(item);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
    finally
    {
        IsBusy = false;
    }
});
```

... with the **added benefit** that  `dataStore.GetItemsAsync(true)` is executed immediately on the background thread. If you put a breakpoint on the verbose code above, you will see that it begins executing `GetItemsAsync(true)` on the Main Thread. If you don't use `ConfigureAwait(false)` constently in the implementation of `GetItemsAsync`, the UI thread could be blocked for a meaningful amount of time.

The above example, used `SafeCommand` integrated with `ViewModelBase` and James Montemagno's `ObservableRangeCollection` to significantly reduce boilerplate code.



### Safe Execution

The suite of SafeExecution helpers provide a safe, consistent way to handle exceptions in your mobile app while reducing boilerplate code. Xamarin Forms' `Command` and `MessagingCenter` have been refactored to implement "safe execution".

To prevent app crashes from unhandled exceptions, initialise SafeExecutionHelpers with a **default exception handler** that logs the exception.

```c#
SafeExecutionHelpers.SetDefaultExceptionHandler(
    (ex) => Console.WriteLine(ex.Message));
```



`SafeCommand` and `SafeMessagingCentere` are refactored to automatically execute in a try-catch block. The methods have been refactored to include an optional `Action<Exception> onException` argument. Safe Execution applies the following logic:

* If `onException` has been provided and the type of `Exception` thrown matches the type handled in the provided `onException`, execute the provided `onException` handler. 

* Otherwise look for a match in the user-defined `GenericExceptionHandlers`. Generic handlers are initialized as follows

  * ```c#
    SafeExecutionHelpers.Configure(s => s.GenericExceptionHandlers.Add(
      (ArgumentException ex) => 
      {
        //Generic handling of ArgumentException here
      }));
    ```

* If no match is found, execute the `DefaultExceptionHandler` if it has been defined. The default handler is agnostic to the type of exception. If defined, it has the effect of silencing unhandled exceptions.

* Finally, if no handler is found and no default handler is defined, throw a `SafeExecutionHelpersException` with the offending exception as its `InnerException`.



For Debugging purposes, one can configure SafeExecutionHelpers to always rethrow exceptions after they have been handled:

```c#
#if DEBUG
	SafeExecutionHelpers.Configure(s => s.ShouldAlwaysRethrowException = true);
#endif
```



> :memo:Tip: When providing an `onException` delegate, if the developer anticipates several different exception-types, this can be handled by using pattern-matching (available from C# version 9). For example:
>
> ```c#
>   onException: (Exception ex) =>
>   {
>       switch (ex)
>       {
>           //Type matching pattern - C# Version 9
>           case ArgumentException:
>           		// Handle bad argument
>               break;
>   	      case DivideByZeroException:
>           		// Handle divide by zero
>     	      	break;
>   	      case OverflowException:
>           		// Handle when integer is too large to be stored
>           		break;
>           default:
>               Console.WriteLine(ex.Message);
>               break;
>       }            
>   }
> ```
>



#### SafeCommand

In addition to implementing "Safe Execution", the SafeCommand offers the following useful features:

*  If a ViewModel has a bindable `IsBusy` property, will set this to `true` while executing. When using this feature, the default handling of multiple invokations is as follows. If the command is fired multiple times, and the first invokation has not completed the second invokation will be blocked from executing. An example where this is handy is to avoid unstable behaviour from app users double-tapping. 
  * Optionally, the developer can set the `isBlocking` argument to false, in which case every invokation will be executed.
  * The corresponding View can bind to `IsBusy` to show an activity indicator when the Command is running. 
* SafeCommand has been refactored with overloads to execute Asynchronous code. This removes the code-smell  `Command(async () => ExecuteCommandAsync)`, instead writing `SafeCommand(ExecuteCommandAsync)`. 
* Whereas Xamarin.Forms.Command begins executing on the Main thread, SafeCommand begins executing immediately on the background thread. This prevents UI-blocking code. If the developer explicity wants the command to run on the UI-thread, he can set the `mustRunOnCurrentSyncContext` parameter to true.



> :memo:Tip: When letting SafeCommand manipulate IsBusy, ensure that when you use OneWay binding in the `<RefreshView>`.
>
> ```xaml
> <RefreshView IsRefreshing="{Binding IsBusy, Mode=OneWay}" 
>              Command="{Binding LoadItemsCommand}">
> ```



#### SafeMessagingCenter

This class refactors Xamarin's MessagingCenter with the same features described in `SafeCommand` above. In addition SafeMessagingCenter has been extended with `SubscribeAny` and `UnsubscribeAny`. This lets MessagingCenter subscribe to a specified message that may come from any Sender. This prevents unnecessary code repetition if the same Action should be executed in response to a message sent from different classes.



#### SafeTask and SafeAction Extensions

`MyAction.SafeInvoke()` and `MyTask.SafeContinueWith()` will apply the [Safe Execution](#safe-execution) logic when handling exceptions.

`SafeFireAndForget` will safely fire-and-forget a task (instead of awaiting it), applying the [Safe Execution](#safe-execution) logic when handling exceptions. `SafeFireAndForget` is adapted from Brandon Minnick's [AsyncAwaitBestPractices](https://github.com/brminnick/AsyncAwaitBestPractices), which in turn was inspired by John Thiriet's blog post, [Removing Async Void](https://johnthiriet.com/removing-async-void/).



### ViewModelBase

ZenMvvm.Helpers includes a minimalist `ViewModelBase` that implements the methods used to create bindable properties (`SetProperty` and `OnPropertyChanged`). In addition it has the commonly referenced bindable properties: `bool IsBusy`, `bool IsNotBusy`, `string Title`, and `string Icon`. 

If the user wishes to roll a custom base ViewModel, we recommend making use of the components offered in this library. An example...

```c#
public abstract class BaseViewModel : ObservableObject, IsBusyAware
{
    string statusMessage = string.Empty;
    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    /// <value>The message.</value>
    public string StatusMessage
    {
        get => statusMessage;
        set => SetProperty(ref statusMessage, value);
    }
}
```

... will have the methods `OnPropertyChanged`, `SetProperty`, and the bindable property `IsBusy` in addition to the user-defined bindable `StatusMessage`.

**Integration with SafeCommand and SafeMessagingCenter**

`SafeCommand` and `SafeMessagingCenter` offer an optional overload that will set the calling ViewModel's `IsBusy` property to true while the command is executing. This was demonstrated in the opening example (repeated below), where the second argument is a reference to the ViewModel, which must implement `IsBusyAware`. 

```c#
LoadItemsCommand = new SafeCommand(
    async () => Items.ReplaceRange(await dataStore.GetItemsAsync(true))
		, viewModel:this);
```



### ObservableRangeCollection

Taken from James Montemagno's [MvvmHelpers](https://github.com/jamesmontemagno/mvvm-helpers), the ObervableRangeCollection extends ObservableCollection by adding the following methods which raise a CollectionChanged Event:

* `AddRange` Adds the elements of the specified collection to the end of the current collection
* `RemoveRange` Removes the first occurence of each item in the specified collection from the current collection
* `ReplaceRange` / `Replace` Clears the current collection and replaces it with the specified collection / item

In the opening example above, `ReplaceRange` reduced...

```c#
Items.Clear();
foreach (var item in items)
    Items.Add(item);
```

... to

```c#
Items.ReplaceRange(items);
```



 ### Grouping

 Taken from James' [MvvmHelpers](https://github.com/jamesmontemagno/mvvm-helpers), `Grouping`provides a grouped ObservableRangeCollection that allows one to easily organsise their ListView / CollectionView data into grouped headings:

* [Enhancing Xamarin.Forms ListView with Grouping Headers](https://montemagno.com/enhancing-xamarin-forms-listview-with-grouping/)
* [Xamarin.Forms CollectionView Grouping](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/collectionview/grouping)



### WeakEvents

Listening for events can lead to memory leaks. The typical pattern for listening to an event, creates a strong reference from the event source to the event listener. The listener won't be garbage collected until the  event handler is explicitly removed. It is easy to forget to remove listeners, resulting in unintended memory leaks. Furthermore, in certain circumstances, you might want the object lifetime of the listener to be controlled by other factors, such as whether it currently belongs to the visual tree of the application, and not by the lifetime of the source.

If you want to provide a weak event, use WeakEventManager. Listeners can then subscribe using the usual syntax `source.MyWeakEvent += OnSomeEvent;`

If you want to consume a strong event with a weak reference, use WeakEventHandler: `source.SomeStrongEvent += new WeakEventHandler<EventArgs>(OnSomeEvent).Handler;`



#### WeakEventManager

Taken from [Xamarin.Forms.WeakEventManager](https://github.com/xamarin/Xamarin.Forms/blob/master/Xamarin.Forms.Core/WeakEventManager.cs) where Xamarin has kept the class private. ZenMvvm exposes this class publically so that you can use it in your own projects.

Creating events using the WeakEventManager, will ensure that they maintain a weak reference to their listeners:

```c#
readonly WeakEventManager _weakEventManager = new WeakEventManager();

public event EventHandler CanExecuteChanged
{
    add => _weakEventManager.AddEventHandler(value);
    remove => _weakEventManager.RemoveEventHandler(value);
}
public void RaiseCanExecuteChanged() => _weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(CanExecuteChanged));

```



#### WeakEventHandler

From [Paul Stovell's Blog](http://paulstovell.com/blog/weakevents). Instead of subscribing to an event with your handler:

`source.SomeStrongEvent += OnSomeEvent;`

, wrap it in the WeakEventHandler:

`source.SomeStrongEvent += new WeakEventHandler<EventArgs>(OnSomeEvent).Handler;`



If you want your `OnSomeEvent` handler to execute on the main thread, use `.HandlerOnMainThread` in place of `.Handler`.

> :memo: Note that the WeakEventHandler wrapper won't be GC'ed, leaving a small "sacrifice" object alive in place of your listener.



## Unit Testing Quickstart

Dependency injection facilitates easy Unit Testing of your viewmodels. Furthermore, if the developer chooses to use `ZenMvvm.Helpers`, the ViewModel shouldn't depend on Xamarin.Forms, making testing easier.

Refer to [ZenMvvmSampleApp](https://github.com/z33bs/ZenMvvm-Sample-App/blob/master/ZenMvvmSampleApp.UnitTests/ItemsViewModelTests.cs) for an example of ViewModel unit-testing with `XUnit` and `Moq`.

 
