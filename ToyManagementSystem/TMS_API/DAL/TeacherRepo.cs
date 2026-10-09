using Microsoft.EntityFrameworkCore;
using TMS_SharedLibrary.Models;

namespace TMS_API.DAL
{
    public class TeacherRepo : ITeacherRepo
    {
        private readonly K40TmsDdContext _context;
        public TeacherRepo(K40TmsDdContext context)
        {
            _context = context;
        }

        public Task<Teacher?> FindByIdAsync(int id)
        {
            return _context.Teachers.FirstOrDefaultAsync(t => t.TeacherId == id);
        }
        public Teacher? FindById(int id)
        {
          return _context.Teachers.FirstOrDefault(t => t.TeacherId == id);
        }

        public Task<List<Teacher>> GetAllAsync()
        {
            return _context.Teachers
                .Include(t => t.User)
                .ToListAsync();
        }

        public List<Teacher> GetAll()
        {
            return _context.Teachers
                .Include(t => t.User)
                .ToList();
        }

        public async Task<Teacher> CreateAsync(Teacher teacher)
        {
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();
            return teacher;
        }

        public Teacher Create(Teacher teacher)
        {
            _context.Teachers.Add(teacher);
            _context.SaveChanges();
            return teacher;
        }
    }
}
