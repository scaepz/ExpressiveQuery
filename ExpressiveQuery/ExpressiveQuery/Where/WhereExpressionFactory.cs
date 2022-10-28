using System.Linq.Expressions;
using LinqKit;
using MirkaApi.Lab.AutoQuery.Exceptions;
using Op = MirkaApi.Lab.AutoQuery.Where.WhereOperations;

namespace MirkaApi.Lab.AutoQuery.Where
{
    public class WhereExpressionFactory<T> : IWhereExpressionFactory
    {
        private readonly AutoQueryableProperties<T> _properties;

        public WhereExpressionFactory(AutoQueryableProperties<T> autoQueryableProperties)
        {
            _properties = autoQueryableProperties;
        }

        public Expression<Func<T, bool>> MakeExpression(string query)
        {
            Expression<Func<T, bool>> expr = null;

            var queryParser = new WhereQueryParser(query);
            while (queryParser.HasMore)
            {
                var firstBooleanOperator = queryParser.GetNextBooleanOperator();
                var secondBooleanOperator = queryParser.GetNextBooleanOperator();

                var parenthesis = queryParser.GetNextParenthesisPart();

                Expression<Func<T, bool>> partExpression = null;

                if (parenthesis != null)
                {
                    partExpression = MakeExpression(parenthesis.ContainedQuery);
                }
                else
                {
                    var queryPart = queryParser.GetNextQueryPart();
                    partExpression = MakeComparisonExpression(queryPart);
                }

                if (firstBooleanOperator == Op.Not || secondBooleanOperator == Op.Not)
                {
                    var prev = partExpression;
                    partExpression = x => prev.Invoke(x) == false;
                }

                expr = CombineExpressions(expr, partExpression, firstBooleanOperator);
            }

            return expr ?? (x => true);
        }

        private Expression<Func<T, bool>> CombineExpressions(Expression<Func<T, bool>> first, Expression<Func<T, bool>> second, string combineOperator)
        {
            if (first == null)
                return second;

            var previous = first;

            if (combineOperator == Op.And)
                return x => previous.Invoke(x) && second.Invoke(x);
            else if (combineOperator == Op.Or)
                return x => previous.Invoke(x) || second.Invoke(x);
            else
                throw new InvalidAutoQueryArgument($"Expected 'and' or 'or' between expressions, but got '{combineOperator}'");
        }

        private Expression<Func<T, bool>> MakeComparisonExpression(QueryPart queryPart)
        {
            if (_properties.TryGetChildren(queryPart.Prop, out IAutoQueryRelation<T> relation))
            {
                return relation.MakeChildExpression(this, queryPart);
            }
            else if (_properties.TryGetString(queryPart.Prop, out var selectString))
            {
                var predicate = MakeStringPredicate(queryPart);

                return x => predicate.Invoke(selectString.Invoke(x));
            }

            else if (_properties.TryGetInt(queryPart.Prop, out var selectInt))
            {
                var predicate = MakeIntPredicate(queryPart);

                return x => predicate.Invoke(selectInt.Invoke(x));
            }

            else if (_properties.TryGetNullableInt(queryPart.Prop, out var selectNullableInt))
            {
                var predicate = MakeIntPredicate(queryPart);

                return x => predicate.Invoke(selectNullableInt.Invoke(x));
            }

            else if (_properties.TryGetDecimal(queryPart.Prop, out var selectDecimal))
            {
                var predicate = MakeDecimalPredicate(queryPart);

                return x => predicate.Invoke(selectDecimal.Invoke(x));
            }

            else if (_properties.TryGetNullableDecimal(queryPart.Prop, out var selectNullableDecimal))
            {
                var predicate = MakeDecimalPredicate(queryPart);

                return x => predicate.Invoke(selectNullableDecimal.Invoke(x));
            }

            else if (_properties.TryGetNullableBoolean(queryPart.Prop, out var selectNullableBool))
            {
                var predicate = MakeBoolPredicate(queryPart);

                return x => predicate.Invoke(selectNullableBool.Invoke(x));
            }

            else if (_properties.TryGetDateTime(queryPart.Prop, out var selectDateTime))
            {
                var predicate = MakeDateTimePredicate(queryPart);

                return x => predicate.Invoke(selectDateTime.Invoke(x));
            }

            else if (_properties.TryGetDateTime(queryPart.Prop, out var selectNullableDateTime))
            {
                var predicate = MakeDateTimePredicate(queryPart);

                return x => predicate.Invoke(selectNullableDateTime.Invoke(x));
            }

            else throw new InvalidAutoQueryArgument($"{queryPart.Prop} is not filterable");
        }

        private Expression<Func<string, bool>> MakeStringPredicate(
            QueryPart queryPart)
        {
            string comparisonValue = queryPart.ComparisonValue;

            IEnumerable<string> strings = null;
            if (queryPart.Operation == Op.In)
            {
                strings = queryPart.ArrayValueItems.Select(x => x.Trim());
            }

            return queryPart.Operation switch
            {
                Op.EqualTo => x => x == comparisonValue,

                Op.GreaterThan => x => x.CompareTo(comparisonValue) > 0,
                Op.GreaterThanOrEqual => x => x.CompareTo(comparisonValue) >= 0,

                Op.LessThan => x => x.CompareTo(comparisonValue) < 0,
                Op.LessThanOrEqual => x => x.CompareTo(comparisonValue) <= 0,

                Op.In => x => strings.Contains(x),

                Op.Contains => x => x.Contains(comparisonValue),
                Op.StartsWith => x => x.StartsWith(comparisonValue),
                Op.EndsWith => x => x.EndsWith(comparisonValue),

                _ => throw new InvalidAutoQueryArgument($"operation {queryPart.Operation} is not supported for strings")
            };

        }
        private Expression<Func<bool?, bool>> MakeBoolPredicate(QueryPart queryPart)
        {
            bool? value;
            if (queryPart.ComparisonValue == "null")
            {
                value = null;
            }
            else if (bool.TryParse(queryPart.ComparisonValue, out bool notNullBool))
            {
                value = notNullBool;
            }
            else
            {
                throw new InvalidAutoQueryArgument($"Could not parse boolean {queryPart.ComparisonValue}");
            }

            return queryPart.Operation switch
            {
                Op.EqualTo => x => x == value,

                _ => throw new InvalidAutoQueryArgument($"operation {queryPart.Operation} is not supported for booleans")
            };
        }

        private Expression<Func<int?, bool>> MakeIntPredicate(QueryPart queryPart)
        {
            int? intValue = null;
            IEnumerable<int?> ints = null;
            if (queryPart.Operation == Op.In)
            {
                ints = queryPart.ArrayValueItems.Select(x => x == "null" ? null : (int?)int.Parse(x));
            }
            else
            {
                if (queryPart.ComparisonValue == "null")
                    intValue = null;

                else if (int.TryParse(queryPart.ComparisonValue, out int intValueNotNull))
                {
                    intValue = intValueNotNull;
                }
                else
                {
                    throw new InvalidAutoQueryArgument($"{queryPart.ComparisonValue } is not an integer");
                }

            }

            return queryPart.Operation switch
            {
                Op.EqualTo => x => x == intValue,

                Op.GreaterThan => x => x > intValue,
                Op.GreaterThanOrEqual => x => x >= intValue,

                Op.LessThan => x => x < intValue,
                Op.LessThanOrEqual => x => x <= intValue,

                Op.In => x => ints.Contains(x),

                _ => throw new InvalidAutoQueryArgument($"operation {queryPart.Operation} not supported for integer")
            };

        }

        private Expression<Func<DateTime?, bool>> MakeDateTimePredicate(QueryPart queryPart)
        {
            DateTime? value;
            if (queryPart.ComparisonValue == "null")
            {
                value = null;
            }
            else if (DateTime.TryParse(queryPart.ComparisonValue, out DateTime parsed))
            {
                value = parsed;
            }
            else
            {
                throw new InvalidAutoQueryArgument($"Could not parse {queryPart.ComparisonValue} to DateTime");
            }

            return queryPart.Operation switch
            {
                Op.EqualTo => x => x == value,
                Op.GreaterThan => x => x > value,
                Op.GreaterThanOrEqual => x => x >= value,

                Op.LessThan => x => x < value,
                Op.LessThanOrEqual => x => x <= value,

                _ => throw new InvalidAutoQueryArgument($"operation {queryPart.Operation} not supported for datetimes")
            };
        }

        private static Expression<Func<decimal?, bool>> MakeDecimalPredicate(QueryPart queryPart)
        {
            decimal? decimalValue = null;
            IEnumerable<decimal?> decimals = null;
            if (queryPart.Operation == Op.In)
            {
                decimals = queryPart.ArrayValueItems.Select(x => x == "null" ? null : (decimal?)decimal.Parse(x));
            }
            else
            {
                if (queryPart.ComparisonValue == "null")
                    decimalValue = null;

                else if (int.TryParse(queryPart.ComparisonValue, out int decimalValueNotNull))
                {
                    decimalValue = decimalValueNotNull;
                }
                else
                {
                    throw new InvalidAutoQueryArgument($"{queryPart.ComparisonValue } is not a decimal");
                }

            }

            return queryPart.Operation switch
            {
                Op.EqualTo => x => x == decimalValue,

                Op.GreaterThan => x => x > decimalValue,
                Op.GreaterThanOrEqual => x => x >= decimalValue,

                Op.LessThan => x => x < decimalValue,
                Op.LessThanOrEqual => x => x <= decimalValue,

                Op.In => x => decimals.Contains(x),

                _ => throw new InvalidAutoQueryArgument($"operation {queryPart.Operation} not supported for decimal")
            };

        }
    }
}
