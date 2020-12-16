using System;
using Xunit;
using ZenMvvm.Helpers;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ZenMvvm.Tests
{
    public class MockViewModel : ViewModelBase { }

    public class TestOverloads
    {
        public TestOverloads(Action action) {}
        public TestOverloads(Action action, IBusy viewModel) : this(action) { }
        public TestOverloads(Action action, IBusy viewModel, Action<Exception> onException) : this(action,viewModel) { }

        public TestOverloads(Func<Task> func) { }
        public TestOverloads(Func<Task> func, IBusy viewModel) : this(func) { }
        public TestOverloads(Func<Task> func, IBusy viewModel, Action<Exception> onException) : this(func, viewModel) { }

    }

    [Collection("SafeTests")]
    public class SafeCommandTests_IsBusy
    {
        const int DELAY = 50;
        [Fact]
        public void Execute_WithIBusy_IsBusyTrueWhileRunning()
        {
            var vm = new MockViewModel();

            var command = new SafeCommand(executeAction: () => { Assert.True(vm.IsBusy); }, vm);

            Assert.False(vm.IsBusy);
            command.Execute(null);
            //see Assert in command
            Assert.False(vm.IsBusy);
        }

        [Fact]
        public void ExecuteAsync_WithIBusy_IsBusyTrueWhileRunning()
        {
            bool isExecuting = true;

            var vm = new MockViewModel();
            var command = new SafeCommand(async () => { Assert.True(vm.IsBusy); await Task.Delay(DELAY); isExecuting = false; }, vm);

            var thread = new Thread(new ThreadStart(() => command.Execute(null)));

            Assert.False(vm.IsBusy);
            thread.Start();
            //see Assert in command
            while (isExecuting)
                Thread.Sleep(DELAY / 25);

            Thread.Sleep(5);
            Assert.False(vm.IsBusy);
        }

        [Theory]
        [InlineData(7)]
        public void ExecuteT_WithIBusy_IsBusyTrueWhileRunning(int number)
        {
            var vm = new MockViewModel();

            var command = new SafeCommand<int>(executeAction: (i) => { Assert.True(vm.IsBusy); }, vm);

            Assert.False(vm.IsBusy);
            command.Execute(number);
            //see Assert in command
            Assert.False(vm.IsBusy);
        }

        [Theory]
        [InlineData(7)]
        public void ExecuteAsyncT_WithIBusy_IsBusyTrueWhileRunning(int number)
        {
            bool isExecuting = true;

            var vm = new MockViewModel();
            var command = new SafeCommand<int>(async (i) => { Assert.True(vm.IsBusy); await Task.Delay(DELAY); isExecuting = false; }, vm);

            var thread = new Thread(new ThreadStart(() => command.Execute(number)));

            Assert.False(vm.IsBusy);
            thread.Start();
            //see Assert in command
            while (isExecuting)
                Thread.Sleep(DELAY / 25);

            Thread.Sleep(5);
            Assert.False(vm.IsBusy);
        }

        [Fact]
        public async Task Execute_ViewModelIsBusy_NotRun()
        {
            bool hasRun = false;
            var vm = new MockViewModel { IsBusy = true };

            var command = new SafeCommand(executeAction: () => { hasRun = true; }, vm);

            await Task.Run(()=>command.Execute(null));
            Assert.False(hasRun);
        }

        [Theory]
        [InlineData(7)]
        public async Task ExecuteT_ViewModelIsBusy_NotRun(int number)
        {
            bool hasRun = false;
            var vm = new MockViewModel { IsBusy = true };

            var command = new SafeCommand<int>(executeAction: (i) => { hasRun = true; }, vm);

            await Task.Run(()=>command.Execute(number));
            Assert.False(hasRun);
        }

        [Fact]
        public void ExecuteAsync_ViewModelIsBusy_NotRun()
        {
            bool hasRun = false;

            var vm = new MockViewModel { IsBusy = true };
            var command = new SafeCommand(async () => { hasRun = true; await Task.Delay(DELAY); }, vm);

            var thread = new Thread(new ThreadStart(() => command.Execute(null)));

            thread.Start();
            
            Thread.Sleep(DELAY);
            Assert.False(hasRun);
        }

        [Theory]
        [InlineData(7)]
        public void ExecuteAsyncT_ViewModelIsBusy_NotRun(int number)
        {
            bool hasRun = false;

            var vm = new MockViewModel { IsBusy = true };
            var command = new SafeCommand<int>(async (i) => { hasRun = true; await Task.Delay(DELAY); }, vm);

            var thread = new Thread(new ThreadStart(() => command.Execute(number)));

            thread.Start();

            Thread.Sleep(DELAY);
            Assert.False(hasRun);
        }
    }
}
