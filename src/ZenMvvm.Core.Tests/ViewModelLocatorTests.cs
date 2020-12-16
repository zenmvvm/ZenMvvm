using System;
using Xunit;
using Moq;
using Xamarin.Forms;
using ZenMvvm.Tests.Views;
using ZenMvvm.Tests.ViewModels;
using ZenMvvm.TestAssembly;

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
            
            ViewModelLocator.Ioc = mockIoc.Object;

            var page = new EmptyPage();
            //Act
            ViewModelLocator.AutoWireViewModel(page);

            mockIoc.VerifyAll();
            ViewModelLocator.Ioc = null;
        }


        [Fact]
        public void AutoWireViewModel_ViewModelExists_SetsBindingContextToViewModel()
        {
            //Don't test the ioc
            var mockIoc = new Mock<IIoc>();
            mockIoc.Setup(m => m.Resolve(typeof(EmptyViewModel))).Returns(new EmptyViewModel());            
            ViewModelLocator.Ioc = mockIoc.Object;

            var page = new EmptyPage();
            //Act
            ViewModelLocator.AutoWireViewModel(page);

            Assert.IsType<EmptyViewModel>(page.BindingContext);
            ViewModelLocator.Ioc = null;
        }

        [Fact]
        public void WireViewModel_NoArgs_UsesNamingConvention()
        {
            //Don't test the ioc
            var mockIoc = new Mock<IIoc>();
            mockIoc.Setup(m => m.Resolve(typeof(EmptyViewModel))).Returns(new EmptyViewModel());
            ViewModelLocator.Ioc = mockIoc.Object;

            var view = new EmptyPage();
            //Act
            ViewModelLocator.WireViewModel(view);

            Assert.IsType<EmptyViewModel>(view.BindingContext);
            Assert.Contains(".ViewModels", view.BindingContext.GetType().FullName);
            Assert.Contains(".Views", view.GetType().FullName);

            ViewModelLocator.Ioc = null;
        }

        [Fact]
        public void WireViewModel_Name_UsesNameSpaceNamingConventionOnly()
        {
            //Don't test the ioc
            var mockIoc = new Mock<IIoc>();
            mockIoc.Setup(m => m.Resolve(typeof(ImplementsBaseViewModel))).Returns(new ImplementsBaseViewModel());
            ViewModelLocator.Ioc = mockIoc.Object;

            var view = new EmptyPage();
            //Act
            ViewModelLocator.WireViewModel(view, nameof(ImplementsBaseViewModel));
            
            Assert.IsType<ImplementsBaseViewModel>(view.BindingContext);
            Assert.Contains(".ViewModels", view.BindingContext.GetType().FullName);
            Assert.Contains(".Views", view.GetType().FullName);

            ViewModelLocator.Ioc = null;
        }

        [Fact]
        public void WireViewModel_AssemblyQualifiedName_UsesSpecifiedName()
        {
            //Don't test the ioc
            var mockIoc = new Mock<IIoc>();
            mockIoc.Setup(m => m.Resolve(typeof(TestAssembly.SpecificViewModel))).Returns(new TestAssembly.SpecificViewModel());
            ViewModelLocator.Ioc = mockIoc.Object;

            var view = new EmptyPage();
            //Act
            ViewModelLocator.WireViewModel(view, typeof(TestAssembly.SpecificViewModel).AssemblyQualifiedName);

            Assert.IsType<TestAssembly.SpecificViewModel>(view.BindingContext);
            Assert.Contains(", ZenMvvm.Core.TestAssembly", view.BindingContext.GetType().AssemblyQualifiedName);
            Assert.Contains(".Views", view.GetType().FullName);

            ViewModelLocator.Ioc = null;
        }
        //todo public void AutoWireViewModel_VmIAppearing_WiresOnViewAppearingEvent()
        //todo public void AutoWireViewModel_VmIDisappearing_WiresOnViewAppearingEvent()

        //todo public void AutoWireViewModel_VmTypeIsNull_ThrowsViewModelBindingException()
        //todo public void AutoWireViewModel_VmIsNull_ThrowsViewModelBindingException()
    }
}
