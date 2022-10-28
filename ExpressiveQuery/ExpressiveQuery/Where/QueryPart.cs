namespace MirkaApi.Lab.AutoQuery.Where
{
    public record QueryPart(string Prop, string Operation, string ComparisonValue, string EnumerableOperation = null, List<string> ArrayValueItems = null);
}
