namespace MirkaApi.Lab.AutoQuery.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// If IQueryable.ToListAsync is called after the EF Core query is already executed, EF Core will throw an exception. 
    /// This extension method will call ToListAsync if possible, else ToList.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <returns></returns>
    public static async Task<List<T>> ToListAsyncIfPossible<T>(this IQueryable<T> queryable)
    {
        return queryable is IAsyncEnumerable<T>
           ? await queryable.ToListAsync()
           : queryable.ToList();
    }

    /// <summary>
    /// If IQueryable.CountAsync is called after the EF Core query is already executed, EF Core will throw an exception. 
    /// This extension method will call CountAsync if possible, else Count.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <returns></returns>
    public static async Task<int> CountAsyncIfPossible<T>(this IQueryable<T> queryable)
    {
        return queryable is IAsyncEnumerable<T>
           ? await queryable.CountAsync()
           : queryable.Count();
    }
}
