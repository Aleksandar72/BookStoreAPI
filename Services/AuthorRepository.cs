using BookStoreAPI.Contract;
using BookStoreAPI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreAPI.Services
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly ApplicationDbContext _db;

        public AuthorRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<bool> Create(Author entity)
        {
            await _db.Authors.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(Author entity)
        {
             _db.Authors.Remove(entity);
            return await Save();
        }

        public async Task<IList<Author>> FindAll()
        {
           
            return await _db.Authors.Include(q => q.Books).ToListAsync();
        }

        public async Task<Author> FindById(int Id)
        {
            return await _db.Authors.Include(q => q.Books).FirstOrDefaultAsync(q => q.Id == Id);
        }

        public async Task<bool> IsExist(int id)
        {
            return await _db.Authors.AnyAsync(q => q.Id == id);
        }

        public async Task<bool> Save()
        {
            var changes = await _db.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> Update(Author entity)
        {
            var update = _db.Authors.Update(entity);
            return await Save();
        }
    }
}
