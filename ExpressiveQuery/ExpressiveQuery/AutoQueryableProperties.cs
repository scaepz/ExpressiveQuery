using MirkaApi.Lab.AutoQuery.Where;
using System.Linq.Expressions;

namespace MirkaApi.Lab.AutoQuery
{
    public class AutoQueryableProperties<T> : IAutoQueryableProperties
    {
        private readonly Dictionary<string, Expression<Func<T, string>>> _strings = new();

        private readonly Dictionary<string, Expression<Func<T, int>>> _ints = new();
        private readonly Dictionary<string, Expression<Func<T, int?>>> _nullableInts = new();

        private readonly Dictionary<string, Expression<Func<T, decimal>>> _decimals = new();
        private readonly Dictionary<string, Expression<Func<T, decimal?>>> _nullableDecimals = new();

        private readonly Dictionary<string, Expression<Func<T, bool?>>> _nullableBools = new();

        private readonly Dictionary<string, Expression<Func<T, DateTime>>> _dateTimes = new();
        private readonly Dictionary<string, Expression<Func<T, DateTime?>>> _nullableDateTimes = new();

        private Dictionary<string, IAutoQueryRelation<T>> _relations = new();

        public AutoQueryableProperties<T> AddProperty(string property, Expression<Func<T, string>> selectProp)
        {
            _strings[property] = selectProp;
            return this;
        }

        public AutoQueryableProperties<T> AddProperty(string property, Expression<Func<T, int>> selectProp)
        {
            _ints[property] = selectProp;
            return this;
        }

        public AutoQueryableProperties<T> AddProperty(string property, Expression<Func<T, int?>> selectProp)
        {
            _nullableInts[property] = selectProp;
            return this;
        }

        public AutoQueryableProperties<T> AddProperty(string property, Expression<Func<T, decimal>> selectProp)
        {
            _decimals[property] = selectProp;
            return this;
        }

        public AutoQueryableProperties<T> AddProperty(string property, Expression<Func<T, decimal?>> selectProp)
        {
            _nullableDecimals[property] = selectProp;
            return this;
        }

        public AutoQueryableProperties<T> AddProperty(string property, Expression<Func<T, bool?>> selectProp)
        {
            _nullableBools[property] = selectProp;
            return this;
        }

        public AutoQueryableProperties<T> AddProperty(string property, Expression<Func<T, DateTime>> selectProp)
        {
            _dateTimes[property] = selectProp;
            return this;
        }

        public AutoQueryableProperties<T> AddProperty(string property, Expression<Func<T, DateTime?>> selectProp)
        {
            _nullableDateTimes[property] = selectProp;
            return this;
        }

        public AutoQueryableProperties<T> AddChildren<TChild>(string property, Expression<Func<T, IEnumerable<TChild>>> selectChildren, Action<AutoQueryableProperties<TChild>> configureChildProperties)
        {
            AutoQueryableProperties<TChild> context = new();
            configureChildProperties(context);

            _relations[property] = new AutoQueryRelation<T, TChild>(selectChildren, context);

            return this;
        }

        public bool TryGetString(string property, out Expression<Func<T, string>> select)
        {
            return _strings.TryGetValue(property, out select);
        }

        public bool TryGetInt(string property, out Expression<Func<T, int>> select)
        {
            return _ints.TryGetValue(property, out select);
        }

        public bool TryGetNullableInt(string property, out Expression<Func<T, int?>> select)
        {
            return _nullableInts.TryGetValue(property, out select);
        }

        public bool TryGetNullableBoolean(string property, out Expression<Func<T, bool?>> select)
        {
            return _nullableBools.TryGetValue(property, out select);
        }

        public bool TryGetDateTime(string property, out Expression<Func<T, DateTime>> select)
        {
            return _dateTimes.TryGetValue(property, out select);
        }

        public bool TryGetNullableDateTime(string property, out Expression<Func<T, DateTime?>> select)
        {
            return _nullableDateTimes.TryGetValue(property, out select);
        }

        public bool TryGetDecimal(string property, out Expression<Func<T, decimal>> select)
        {
            return _decimals.TryGetValue(property, out select);
        }
        public bool TryGetNullableDecimal(string property, out Expression<Func<T, decimal?>> select)
        {
            return _nullableDecimals.TryGetValue(property, out select);
        }

        public bool TryGetChildren(string contextName, out IAutoQueryRelation<T> autoQueryRelation)
        {
            IAutoQueryRelation<T> relation;
            if (!_relations.TryGetValue(contextName, out relation))
            {
                autoQueryRelation = null;
                return false;
            }
            autoQueryRelation = relation;

            return true;
        }

        public IWhereExpressionFactory GetWhereExpressionFactory()
        {
            return new WhereExpressionFactory<T>(this);
        }
    }
}
