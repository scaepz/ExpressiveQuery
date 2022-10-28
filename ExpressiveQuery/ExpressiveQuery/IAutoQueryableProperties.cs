using MirkaApi.Lab.AutoQuery.Where;

namespace MirkaApi.Lab.AutoQuery
{
    public interface IAutoQueryableProperties
    {
        IWhereExpressionFactory GetWhereExpressionFactory();
    }
}
