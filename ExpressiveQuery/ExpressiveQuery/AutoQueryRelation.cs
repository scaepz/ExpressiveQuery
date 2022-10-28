using LinqKit;
using MirkaApi.Lab.AutoQuery.Exceptions;
using MirkaApi.Lab.AutoQuery.Where;
using System.Linq.Expressions;
using Op = MirkaApi.Lab.AutoQuery.Where.WhereOperations;

namespace MirkaApi.Lab.AutoQuery
{
    public class AutoQueryRelation<T, TChild> : IAutoQueryRelation<T>
    {
        private Expression<Func<T, IEnumerable<TChild>>> _selectChildren;

        public IAutoQueryableProperties ChildProperties { get; set; }

        public AutoQueryRelation(Expression<Func<T, IEnumerable<TChild>>> selectChildren, AutoQueryableProperties<TChild> childProperties)
        {
            _selectChildren = selectChildren;
            ChildProperties = childProperties;
        }

        public Expression<Func<IEnumerable<TChild>, bool>> MakeEnumerablePredicate(string operation, Expression<Func<TChild, bool>> itemPredicate)
        {
            if (operation == Op.Any)
            {
                return x => x.Any(i => itemPredicate.Invoke(i));
            }
            else if (operation == Op.All)
            {
                return x => x.All(i => itemPredicate.Invoke(i));
            }
            else
            {
                throw new InvalidAutoQueryArgument($"Expected enumerable operation {Op.Any} or {Op.All}, but got {operation}");
            }
        }

        public Expression<Func<T, bool>> MakeChildExpression(WhereExpressionFactory<T> whereExpressionFactory, QueryPart queryPart)
        {
            var childExpressionFactory = (WhereExpressionFactory<TChild>)ChildProperties.GetWhereExpressionFactory();

            var childExpression = childExpressionFactory.MakeExpression(queryPart.ComparisonValue);

            return x => MakeEnumerablePredicate(queryPart.Operation, childExpression).Invoke(_selectChildren.Invoke(x));
        }
    }
}
