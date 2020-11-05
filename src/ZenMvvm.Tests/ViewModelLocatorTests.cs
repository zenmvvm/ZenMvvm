using System;
using Xunit;
using Moq;
using Xamarin.Forms;
using ZenMvvm.Tests.Views;
using ZenMvvm.Tests.ViewModels;

namespace ZenMvvm.Tests
{
    public class ViewModelLocatorTests
    {
        public ViewModelLocatorTests()
        {
        }

        [Fact]
        public void AutoWireViewModel_Always_CallsIIocResolve()
        {
            var mockIoc = new Mock<IIoc>();
            mockIoc.Setup(m => m.Resolve(typeof(EmptyViewModel))).Returns(new EmptyViewModel()).Verifiable();
            ViewModelLocator.ContainerImplementation = mockIoc.Object;

            var page = new EmptyPage();
            //Act
            ViewModelLocator.AutoWireViewModel(page);

            mockIoc.VerifyAll();
            ViewModelLocator.SetDefaultContainerImplementation();
        }


        [Fact]
        public void AutoWireViewModel_ViewModelExists_SetsBindingContextToViewModel()
        {
            //Don't test the ioc
            var mockIoc = new Mock<IIoc>();
            mockIoc.Setup(m => m.Resolve(typeof(EmptyViewModel))).Returns(new EmptyViewModel());            
            ViewModelLocator.ContainerImplementation = mockIoc.Object;

            var page = new EmptyPage();
            //Act
            ViewModelLocator.AutoWireViewModel(page);

            Assert.IsType<EmptyViewModel>(page.BindingContext);
            ViewModelLocator.SetDefaultContainerImplementation();
        }

        [Fact]
        public void WireViewModel_NoArgs_UsesNamingConvention()
        {
            //Don't test the ioc
            var mockIoc = new Mock<IIoc>();
            mockIoc.Setup(m => m.Resolve(typeof(EmptyViewModel))).Returns(new EmptyViewModel());
            ViewModelLocator.ContainerImplementation = mockIoc.Object;

            var view = new EmptyPage();
            //Act
            ViewModelLocator.WireViewModel(view);

            Assert.IsType<EmptyViewModel>(view.BindingContext);
            Assert.Contains(".ViewModels", view.BindingContext.GetType().FullName);
            Assert.Contains(".Views", view.GetType().FullName);

            ViewModelLocator.SetDefaultContainerImplementation();
        }

        [Fact]
        public void WireViewModel_Name_UsesNameSpaceNamingConventionOnly()
        {
            //Don't test the ioc
            var mockIoc = new Mock<IIoc>();
            mockIoc.Setup(m => m.Resolve(typeof(ImplementsBaseViewModel))).Returns(new ImplementsBaseViewModel());
            ViewModelLocator.ContainerImplementation = mockIoc.Object;

            var view = new EmptyPage();
            //Act
            ViewModelLocator.WireViewModel(view, nameof(ImplementsBaseViewModel));
            
            Assert.IsType<ImplementsBaseViewModel>(view.BindingContext);
            Assert.Contains(".ViewModels", view.BindingContext.GetType().FullName);
            Assert.Contains(".Views", view.GetType().FullName);

            ViewModelLocator.SetDefaultContainerImplementation();
        }

        [Fact]
        public void WireViewModel_AssemblyQualifiedName_UsesSpecifiedName()
        {
            //Don't test the ioc
            var mockIoc = new Mock<IIoc>();
            mockIoc.Setup(m => m.Resolve(typeof(TestAssembly.SpecificViewModel))).Returns(new TestAssembly.SpecificViewModel());
            ViewModelLocator.ContainerImplementation = mockIoc.Object;

            var view = new EmptyPage();
            //Act
            ViewModelLocator.WireViewModel(view, typeof(TestAssembly.SpecificViewModel).AssemblyQualifiedName);

            Assert.IsType<TestAssembly.SpecificViewModel>(view.BindingContext);
            Assert.Contains(", ZenMvvm.TestAssembly", view.BindingContext.GetType().AssemblyQualifiedName);
            Assert.Contains(".Views", view.GetType().FullName);

            ViewModelLocator.SetDefaultContainerImplementation();
        }
        //todo public void AutoWireViewModel_VmIAppearing_WiresOnViewAppearingEvent()
        //todo public void AutoWireViewModel_VmIDisappearing_WiresOnViewAppearingEvent()

        //todo public void AutoWireViewModel_VmTypeIsNull_ThrowsViewModelBindingException()
        //todo public void AutoWireViewModel_VmIsNull_ThrowsViewModelBindingException()
    }
}
