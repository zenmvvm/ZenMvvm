using System.Threading.Tasks;

namespace ZenMvvm
{
    public interface IInitializeAsync
    {
        bool IsInitialized { get; set; }
        Task InitializeAsync();
    }
}
