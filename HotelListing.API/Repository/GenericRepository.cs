using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly HotelListingDbContext _context;
        private readonly IMapper _mapper;

        public GenericRepository(HotelListingDbContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }

        public async Task<T> AddAsync(T entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetAsync(id);

            // remove is not async, so we don't use await here:
            _context.Set<T>().Remove(entity);

            // save changes is async
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Exists(int id)
        {
            var entity = await GetAsync(id);
            return entity != null;
        }

        public async Task<List<T>> GetAllAsync()
        {
            // here, we say go to the DB, get the table associated with "T" (can be any table in this case)
            // return all as a list.
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<PagedResult<TResult>> GetAllAsync<TResult>(QueryParameters queryParameters)
        {
            var totalSize = await _context.Set<T>().CountAsync(); // go to DB and count the amount of records in the table
            var items = await _context.Set<T>()
                .Skip(queryParameters.StartIndex) // skip the first n records
                .Take(queryParameters.PageSize) // take the next n records
                .ProjectTo<TResult>(_mapper.ConfigurationProvider) // map the records to the desired type (exact columns it should query)
                .ToListAsync();

            return new PagedResult<TResult>
            {
                Items = items,
                PageNumber = queryParameters.PageNumber,
                RecordNumber = queryParameters.PageSize,
                TotalCount = totalSize
            };
        }

        public async Task<T> GetAsync(int? id)
        {
            // "id is null" is equivelant of "id == null"
            // just a newer syntax for Csharp
            if (id is null)
            {
                return null;
            }

            return await _context.Set<T>().FindAsync(id);
        }

        public async Task UpdateAsync(T entity)
        {
            // update is not async, so we don't use await here:
            _context.Update(entity);

            // save changes is async
            await _context.SaveChangesAsync();
        }
    }
}
