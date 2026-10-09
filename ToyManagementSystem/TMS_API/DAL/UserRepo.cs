using Microsoft.EntityFrameworkCore;
using TMS_SharedLibrary.Models;

namespace TMS_API.DAL
{
    public class UserRepo : IUserRepo
    {
        private readonly K40TmsDdContext _context;

        public UserRepo(K40TmsDdContext context)
        {
            _context = context;
        }

        public User? FindById(int id)
        {
            return _context.Users
                .Include(u => u.Student)
                .Include(u => u.Teacher)
                .FirstOrDefault(u => u.UserId == id);
        }

        public Task<User?> FindByIdAsync(int id)
        {
            return _context.Users
                .Include(u => u.Student)
                .Include(u => u.Teacher)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public List<User> GetAll()
        {
            return _context.Users
                .Include(u => u.Student)
                .Include(u => u.Teacher)
                .OrderBy(u => u.UserId)
                .ToList();
        }

        public Task<List<User>> GetAllAsync()
        {
            return _context.Users
                .Include(u => u.Student)
                .Include(u => u.Teacher)
                .OrderBy(u => u.UserId)
                .ToListAsync();
        }

        public User? FindByOid(string oid)
        {
            return _context.Users
                .Include(u => u.Student)
                .Include(u => u.Teacher)
                .FirstOrDefault(u => u.Oid == oid);
        }

        public Task<User?> FindByOidAsync(string oid)
        {
            return _context.Users
                .Include(u => u.Student)
                .Include(u => u.Teacher)
                .FirstOrDefaultAsync(u => u.Oid == oid);
        }

        public User Create(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
