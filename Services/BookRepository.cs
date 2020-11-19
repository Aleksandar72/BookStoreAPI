using BookStoreAPI.Contract;
using BookStoreAPI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreAPI.Services
{
    public class BookRepository : IBookRepository
    {
        private readonly ApplicationDbContext _db;

        public BookRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<bool> Create(Book entity)
        {
            await _db.Books.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(Book entity)
        {
            _db.Books.Remove(entity);
            return await Save();
        }

        public async Task<IList<Book>> FindAll()
        {

            return await _db.Books.Include(q => q.Author).ToListAsync();
        }

        public async Task<Book> FindById(int Id)
        {
            return await _db.Books.Include(q => q.Author).FirstOrDefaultAsync(x => x.Id == Id);
        }

        public async Task<bool> IsExist(int id)
        {
            return await _db.Books.AnyAsync(q => q.Id == id);
        }

        public async Task<bool> Save()
        {
            var changes = await _db.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> Update(Book entity)
        {
            var update = _db.Books.Update(entity);
            return await Save();
        }
    }
}
