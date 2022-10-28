using MirkaApi.Lab.AutoQuery.Exceptions;
using System.Linq.Expressions;

namespace MirkaApi.Lab.AutoQuery.OrderBy
{

    public class OrderByApplier<T>
    {
        private readonly AutoQueryableProperties<T> _properties;
        private const string _separator = "thenBy";
        private const string _ascending = "asc";
        private const string _descending = "desc";

        public OrderByApplier(AutoQueryableProperties<T> autoQueryableProperties)
        {
            _properties = autoQueryableProperties;
        }
        public IQueryable<T> ApplyOrderBy(IQueryable<T> queryable, string orderBy)
        {
            var orders = orderBy.Trim().Split(_separator);
            bool orderedOnce = false;
            foreach (var order in orders)
            {
                var orderParts = order.Trim().Split(' ');
                EnsureCorrectPartCount(orderParts);

                string prop = orderParts[0];
                bool ascending = GetAscending(orderParts);

                if (_properties.TryGetInt(prop, out var selectInt))
                {
                    queryable = Apply(queryable, ascending, selectInt, orderedOnce);
                }
                else if (_properties.TryGetString(prop, out var selectString))
                {
                    queryable = Apply(queryable, ascending, selectString, orderedOnce);
                }

                else if (_properties.TryGetNullableInt(prop, out var selectNullableInt))
                {
                    queryable = Apply(queryable, ascending, selectNullableInt, orderedOnce);
                }

                else if (_properties.TryGetNullableBoolean(prop, out var selectNullableBoolean))
                {
                    queryable = Apply(queryable, ascending, selectNullableBoolean, orderedOnce);
                }

                else if (_properties.TryGetDateTime(prop, out var selectDateTime))
                {
                    queryable = Apply(queryable, ascending, selectDateTime, orderedOnce);
                }

                else if (_properties.TryGetNullableDateTime(prop, out var selectNullableDateTime))
                {
                    queryable = Apply(queryable, ascending, selectNullableDateTime, orderedOnce);
                }

                else if (_properties.TryGetDecimal(prop, out var selectDecimal))
                {
                    queryable = Apply(queryable, ascending, selectDecimal, orderedOnce);
                }

                else if (_properties.TryGetNullableDecimal(prop, out var selectNullableDecimal))
                {
                    queryable = Apply(queryable, ascending, selectNullableDecimal, orderedOnce);
                }

                else
                    throw new InvalidAutoQueryArgument($"Property {prop} is not valid for orderBy");

                orderedOnce = true;
            }

            return queryable;
        }

        private static void EnsureCorrectPartCount(string[] orderParts)
        {
            if (orderParts.Length == 0 || orderParts[0] == string.Empty)
                throw new InvalidAutoQueryArgument($"orderBy expected property after {_separator}");

            if (orderParts.Length > 2)
                throw new InvalidAutoQueryArgument($"orderBy expected {_separator} at {orderParts[2]}");
        }

        private static bool GetAscending(string[] orderParts)
        {
            if (orderParts.Length == 1)
                return true;
            else if (orderParts[1] == _ascending)
                return true;
            else if (orderParts[1] == _descending)
                return false;
            else
                throw new InvalidAutoQueryArgument($"orderBy expected {_ascending}, {_descending}, or {_separator} at {orderParts[1]}");
        }

        private IOrderedQueryable<T> Apply<TKey>(IQueryable<T> queryable, bool ascending, Expression<Func<T, TKey>> selectKey, bool orderedOnce)
        {
            if (orderedOnce)
                return ApplyThenBy((IOrderedQueryable<T>)queryable, ascending, selectKey);
            else
                return ApplyOrderBy(queryable, ascending, selectKey);
        }

        private IOrderedQueryable<T> ApplyOrderBy<TKey>(IQueryable<T> queryable, bool ascending, Expression<Func<T, TKey>> selectKey)
        {
            if (ascending)
                return queryable.OrderBy(selectKey);
            else
                return queryable.OrderByDescending(selectKey);
        }


        private IOrderedQueryable<T> ApplyThenBy<TKey>(IOrderedQueryable<T> queryable, bool ascending, Expression<Func<T, TKey>> selectKey)
        {
            if (ascending)
                return queryable.ThenBy(selectKey);
            else
                return queryable.ThenByDescending(selectKey);
        }


    }
}
