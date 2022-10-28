using LinqKit;
using MirkaApi.Lab.AutoQuery.Exceptions;
using MirkaApi.Lab.AutoQuery.OrderBy;
using MirkaApi.Lab.AutoQuery.Where;

namespace MirkaApi.Lab.AutoQuery
{
    public class AutoQuerier<T> : IAutoQuerier<T>
    {
        private readonly AutoQueryableProperties<T> _properties;
        private readonly int? _defaultTake;
        private const string _where = "where";
        private const string _orderBy = "orderBy";
        private const string _skip = "skip";
        private const string _take = "take";

        public AutoQuerier(AutoQueryableProperties<T> properties, int defaultTake = 30)
        {
            _properties = properties;
            _defaultTake = defaultTake;
        }

        public IQueryable<T> ApplyAll(IQueryable<T> queryable, IAutoQueryParameters parameters)
        {
            queryable = ApplyWhere(queryable, parameters.Where);
            queryable = ApplyOrderBy(queryable, parameters.OrderBy);
            queryable = ApplySkip(queryable, parameters.Skip);
            queryable = ApplyTake(queryable, parameters.Take);

            return queryable;
        }

        public IQueryable<T> ApplyAll(IQueryable<T> queryable, IDictionary<string, string> queryStringDictionary)
        {
            var parameters = ParseQueryStringDict(queryStringDictionary);

            return ApplyAll(queryable, parameters);
        }

        public IQueryable<T> ApplyTake(IQueryable<T> queryable, int? take)
        {
            if (take.HasValue)
            {
                return queryable.Take(take.Value);
            }
            else
            {
                return queryable.Take(_defaultTake.Value);
            }
        }

        public IQueryable<T> ApplySkip(IQueryable<T> queryable, int? skip)
        {
            if (skip == null)
                return queryable;

            return queryable.Skip(skip.Value);
        }

        public IQueryable<T> ApplyWhere(IQueryable<T> queryable, string where)
        {
            if (string.IsNullOrEmpty(where))
                return queryable;

            WhereExpressionFactory<T> whereExpressionFactory = new(_properties);

            var whereExpression = whereExpressionFactory.MakeExpression(where);

            return queryable.AsExpandable().Where(whereExpression);
        }

        public IQueryable<T> ApplyOrderBy(IQueryable<T> queryable, string orderBy)
        {
            if (string.IsNullOrEmpty(orderBy))
                return queryable;

            var applier = new OrderByApplier<T>(_properties);
            return applier.ApplyOrderBy((IOrderedQueryable<T>)queryable, orderBy);
        }

        private static IAutoQueryParameters ParseQueryStringDict(IDictionary<string, string> queryStringDictionary)
        {
            AutoQueryParameters parameters = new();

            if (queryStringDictionary.TryGetValue(_where, out string where))
                parameters.Where = where;

            if (queryStringDictionary.TryGetValue(_orderBy, out string orderBy))
                parameters.OrderBy = orderBy;

            if (queryStringDictionary.TryGetValue(_take, out string takeStr))
            {
                if (int.TryParse(takeStr, out int take))
                    parameters.Take = take;

                else
                    throw new InvalidAutoQueryArgument($"Argument for {_take} was could not be parsed to int. Value: {takeStr}");
            }

            if (queryStringDictionary.TryGetValue(_skip, out string skipStr))
            {
                if (int.TryParse(skipStr, out int skip))
                    parameters.Skip = skip;

                else
                    throw new InvalidAutoQueryArgument($"Argument for {_skip} was could not be parsed to int. Value: {skipStr}");
            }

            return parameters;
        }

        private class AutoQueryParameters : IAutoQueryParameters
        {
            public string Where { get; set; }

            public string OrderBy { get; set; }

            public int? Take { get; set; }

            public int? Skip { get; set; }
        }
    }
}
