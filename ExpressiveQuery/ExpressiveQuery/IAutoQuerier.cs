namespace MirkaApi.Lab.AutoQuery
{
    public interface IAutoQuerier<T>
    {
        IQueryable<T> ApplyAll(IQueryable<T> queryable, IAutoQueryParameters parameters);
        IQueryable<T> ApplyAll(IQueryable<T> queryable, IDictionary<string, string> queryStringDictionary);
        IQueryable<T> ApplyOrderBy(IQueryable<T> queryable, string orderBy);
        IQueryable<T> ApplySkip(IQueryable<T> queryable, int? skip);
        IQueryable<T> ApplyTake(IQueryable<T> queryable, int? take);
        IQueryable<T> ApplyWhere(IQueryable<T> queryable, string where);
    }
}