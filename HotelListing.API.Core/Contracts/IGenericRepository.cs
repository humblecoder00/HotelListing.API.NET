using HotelListing.API.Core.Models;

namespace HotelListing.API.Core.Contracts
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetAsync(int? id); // T -> represents entity type
        Task<TResult?> GetAsync<TResult>(int? id); // TResult -> represents the "whataever" the data type of the result is (or can be called TEntity too)

        Task<List<T>> GetAllAsync();

        Task<List<TResult>> GetAllAsync<TResult>();

        Task<PagedResult<TResult>> GetAllAsync<TResult>(QueryParameters queryParameters);
        Task<T> AddAsync(T entity);
        Task<TResult> AddAsync<TSource, TResult>(TSource source); // Uses 2 generics, TSource params type, TResult return type

        Task DeleteAsync(int id);
        Task UpdateAsync(T entity);
        Task UpdateAsync<TSource>(int id, TSource source);
        Task<bool> Exists(int id);
    }
}
