using Microsoft.Extensions.DependencyInjection;

namespace Application.Utilities.IoC
{
    public interface ICoreModule
    {
        void Load(IServiceCollection serviceCollection);
    }
}