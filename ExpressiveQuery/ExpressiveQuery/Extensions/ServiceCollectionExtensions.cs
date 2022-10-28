using Microsoft.Extensions.DependencyInjection;
using MirkaApi.Lab.AutoQuery;

namespace MirkaApi.Lab.AutoQuery.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddAutoQuerier<T>(this IServiceCollection services, Action<AutoQueryableProperties<T>> configure, int defaultTake = 30)
    {
        return services.AddTransient<IAutoQuerier<T>>(_ =>
        {
            var props = new AutoQueryableProperties<T>();
            configure(props);
            return new AutoQuerier<T>(props, defaultTake);
        });
    }
}
