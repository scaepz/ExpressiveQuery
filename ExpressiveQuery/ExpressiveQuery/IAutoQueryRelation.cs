using MirkaApi.Lab.AutoQuery.Where;
using System.Linq.Expressions;

namespace MirkaApi.Lab.AutoQuery
{
    public interface IAutoQueryRelation<T>
    {
        IAutoQueryableProperties ChildProperties { get; set; }

        Expression<Func<T, bool>> MakeChildExpression(WhereExpressionFactory<T> whereExpressionFactory, QueryPart queryPart);
    }
}