using System;
using System.Threading.Tasks;

namespace ZenMvvm.Tests.ViewModels
{
    public class ImplementsBaseViewModel : IOnViewNavigatedAsync<object>, IOnViewRemoved, IOnViewAppearing, IOnViewDisappearing
    {
        public ImplementsBaseViewModel()
        {
        }

        public void OnViewAppearing(object sender, EventArgs e)
        {
        }

        public void OnViewDisappearing(object sender, EventArgs e)
        {
        }

        public Task OnViewNavigatedAsync(object navigationData)
        {
            return Task.FromResult(true);
        }

        public Task OnViewRemovedAsync()
        {
            return Task.FromResult(true);
        }
    }
}
