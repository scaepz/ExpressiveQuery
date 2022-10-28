using MirkaApi.Lab.AutoQuery.Exceptions;
using Op = MirkaApi.Lab.AutoQuery.Where.WhereOperations;

namespace MirkaApi.Lab.AutoQuery.Where
{
    public class WhereQueryParser
    {

        public const char Quote = '\'';
        public const char Space = ' ';
        public const char OpenParenthesis = '(';
        public const char CloseParenthesis = ')';
        public const string OpenArray = "[";
        public const string CloseArray = "]";

        public string Query { get; private set; }

        public bool HasMore => Query.Length > 0;

        public WhereQueryParser(string query)
        {
            Query = query;
        }

        public string GetNextBooleanOperator()
        {
            var trimmedQuery = Query.TrimStart();
            const string and = $"{Op.And} ";
            const string or = $"{Op.Or} ";
            const string not = $"{Op.Not} ";

            string booleanOperator;

            if (trimmedQuery.StartsWith(and))
                booleanOperator = Op.And;
            else if (trimmedQuery.StartsWith(or))
                booleanOperator = Op.Or;
            else if (trimmedQuery.StartsWith(not))
                booleanOperator = Op.Not;
            else
                return null;


            Query = trimmedQuery[booleanOperator.Length..];

            return booleanOperator;
        }

        public ParenthesisPart GetNextParenthesisPart()
        {
            var trimmedQuery = Query.Trim();

            if (!trimmedQuery.StartsWith("("))
            {
                return null;
            }

            var chars = trimmedQuery.ToCharArray();
            int currentDepth = 1;
            bool currentlyInsideQuotes = false;

            for (int i = 1; i < chars.Length; i++)
            {
                if (chars[i] == OpenParenthesis && !currentlyInsideQuotes)
                {
                    currentDepth++;
                }
                else if (chars[i] == Quote)
                {
                    if (!currentlyInsideQuotes)
                    {
                        bool previousCharWasSpace = chars[i - 1] == Space;
                        currentlyInsideQuotes = previousCharWasSpace;
                    }
                    else
                    {
                        if (chars.Length == i - 1 ||
                            chars[i + 1] == Space ||
                            chars[i + 1] == CloseParenthesis)
                            currentlyInsideQuotes = false;
                    }

                }
                else if (chars[i] == CloseParenthesis && !currentlyInsideQuotes)
                {
                    currentDepth--;
                    if (currentDepth == 0)
                    {
                        string parenthesisContent = trimmedQuery[1..i];
                        Query = trimmedQuery[(i + 1)..];
                        return new ParenthesisPart(parenthesisContent);
                    }
                }
            }

            throw new ArgumentException($"Unclosed parenthesis: {Query}");
        }

        public QueryPart GetNextQueryPart()
        {
            var parts = Query
                  .Trim()
                  .Split(Space);

            var property = parts[0];
            var operation = parts[1];

            Query = string.Join(Space, parts.Skip(2));

            if (ExpectChildQuery(operation))
            {
                var childQuery = GetNextChildQuery();
                return new QueryPart(property, operation, childQuery);
            }
            if (ExpectArrayValue(operation))
            {
                var arrayValue = GetNextArrayValue();
                return new QueryPart(property, operation, null, null, arrayValue);
            }
            else if (IsNextQuotedValue())
            {
                var value = GetNextQuotedValue();
                return new QueryPart(property, operation, value);
            }
            else
            {
                var value = GetNextSimpleValue();
                return new QueryPart(property, operation, value);
            }
        }

        private string GetNextChildQuery()
        {
            var parenthesis = GetNextParenthesisPart();
            if (parenthesis == null)
                throw new InvalidAutoQueryArgument($"Expected parenthesis here: {Query}");

            return parenthesis.ContainedQuery;
        }

        private bool ExpectArrayValue(string operation)
        {
            return operation == Op.In;
        }

        private List<string> GetNextArrayValue()
        {
            if (!Query.TrimStart().StartsWith(OpenArray))
                throw new InvalidAutoQueryArgument($"Expected array value starting here: {Query}");

            var closeIndex = Query.IndexOf(CloseArray);

            if (closeIndex < 0)
                throw new InvalidAutoQueryArgument($"This array did not close: {Query}");

            var commaDelimitedArray = Query[1..Query.IndexOf(CloseArray)];
            Query = Query[(closeIndex + 1)..];
            return commaDelimitedArray.Split(",").Select(x => x.Trim()).ToList();
        }

        private bool IsNextQuotedValue()
        {
            if (!Query.Trim().StartsWith(Quote))
                return false;

            return Query
                .Split(Space)
                .Any(WordEndsQuote);
        }

        private string GetNextSimpleValue()
        {
            var parts = Query.Split(Space);
            Query = string.Join(Space, parts.Skip(1));

            return parts.First();
        }

        private string GetNextSimpleArrayValue()
        {
            var parts = Query.Split(',');
            Query = string.Join(",", parts.Skip(1));

            return parts.First().TrimEnd(']');
        }

        private bool ExpectChildQuery(string operation)
        {
            return operation == Op.Any
                || operation == Op.All;
        }

        private bool WordEndsQuote(string word)
        {
            return word.EndsWith(Quote)
                || word.EndsWith($"{Quote}{CloseParenthesis}");
        }

        private string GetNextQuotedValue()
        {
            var parts = Query.Split(Space);
            var quoteEndIndex = parts.ToList().FindIndex(WordEndsQuote);

            var first = parts[0];
            var last = parts[quoteEndIndex];

            string fullString;
            if (first == last)
            {
                fullString = first[1..^1];
            }
            else
            {
                var firstTrimmed = first[1..];
                var lastTrimmed = last[..^1];
                var middleParts = parts.Skip(1).Take(quoteEndIndex - 1).ToList();

                middleParts.Insert(0, firstTrimmed);
                middleParts.Add(lastTrimmed);

                fullString = string.Join(' ', middleParts);
            }

            Query = string.Join(Space, parts.Skip(quoteEndIndex + 1));
            return fullString;
        }

        private class QueryParts
        {
            public string Property { get; }
            public string Operation { get; }
            public IEnumerable<string> Rest { get; }
            public QueryParts(string query)
            {
                var parts = query
                    .Trim()
                    .Split(Space)
                    .ToList();

                Property = parts[0];
                Operation = parts[1];
                Rest = parts.Skip(2);
            }
        }
    }

}
