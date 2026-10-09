using Microsoft.EntityFrameworkCore;
using TMS_SharedLibrary.Audit;
using TMS_SharedLibrary.Audit.Enums;
using TMS_SharedLibrary.Models;
using TMS_SharedLibrary.Utilities;

namespace TMS_API.DAL
{
    public class StudentRepo : IStudentRepo
    {
        private readonly K40TmsDdContext _context;
        private readonly IAuditLogger _auditLogger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StudentRepo(
            K40TmsDdContext context,
            IAuditLogger auditLogger,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _auditLogger = auditLogger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task BookToyAsync(int studentId, int toyId)
        {
            var toyLoan = new ToyLoan
            {
                StudentId = studentId,
                ToyId = toyId,
                BorrowDate = DateOnly.FromDateTime(DateTime.Now),
                DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(3 * 7)),
                ReturnDate = null
            };
            _context.ToyLoans.Add(toyLoan);

            var toy = await _context.Toys.FirstOrDefaultAsync(t => t.ToyId == toyId);
            if (toy != null)
            {
                toy.IsAvailable = false;
                _context.Toys.Update(toy);
            }

            await _context.SaveChangesAsync();

            AuditHelper.LogAudit(
                _auditLogger,
                _httpContextAccessor.HttpContext,
                AuditAction.Borrow,
                nameof(Toy),
                toyId,
                $"borrowed {toy?.Name}");
        }

        public void BookToy(int studentId, int toyId)
        {
            var toyLoan = new ToyLoan
            {
                StudentId = studentId,
                ToyId = toyId,
                BorrowDate = DateOnly.FromDateTime(DateTime.Now),
                DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(3 * 7)),
                ReturnDate = null
            };
            _context.ToyLoans.Add(toyLoan);

            var toy = _context.Toys.FirstOrDefault(t => t.ToyId == toyId);
            if (toy != null)
            {
                toy.IsAvailable = false;
                _context.Toys.Update(toy);
            }

            _context.SaveChanges();

            // Log the borrow action
            AuditHelper.LogAudit(
                _auditLogger,
                _httpContextAccessor.HttpContext,
                AuditAction.Borrow,
                nameof(Toy),          // entity name
                toyId,                // entityId
                $"borrowed {toy?.Name}"); // details
        }

        public async Task ReturnToyAsync(int studentId, int toyId)
        {
            var toyLoan = await _context.ToyLoans
                .Where(tl => tl.StudentId == studentId && tl.ToyId == toyId && tl.ReturnDate == null)
                .FirstOrDefaultAsync();

            if (toyLoan != null)
            {
                var toy = await _context.Toys.FirstOrDefaultAsync(t => t.ToyId == toyId);
                if (toy != null)
                {
                    toy.IsAvailable = true;
                    _context.Toys.Update(toy);
                    toyLoan.ReturnDate = DateOnly.FromDateTime(DateTime.Now);
                    _context.ToyLoans.Update(toyLoan);

                    await _context.SaveChangesAsync();

                    AuditHelper.LogAudit(
                        _auditLogger,
                        _httpContextAccessor.HttpContext,
                        AuditAction.Return,
                        nameof(Toy),
                        toyId,
                        $"returned {toy?.Name}");
                }
            }
        }


        public void ReturnToy(int studentId, int toyId)
        {
            var toyLoan = _context.ToyLoans
                .Where(tl => tl.StudentId == studentId && tl.ToyId == toyId && tl.ReturnDate == null)
                .FirstOrDefault();

            if (toyLoan != null)
            {
                var toy = _context.Toys.FirstOrDefault(t => t.ToyId == toyId);
                if (toy != null)
                {
                    toy.IsAvailable = true;
                    _context.Toys.Update(toy);
                    toyLoan.ReturnDate = DateOnly.FromDateTime(DateTime.Now);
                    _context.ToyLoans.Update(toyLoan);
                    _context.SaveChanges();

                    // Log the return action
                    AuditHelper.LogAudit(
                        _auditLogger,
                        _httpContextAccessor.HttpContext,
                        AuditAction.Return,
                        nameof(Toy),
                        toyId,
                        $"returned {toy?.Name}");
                }
            }
        }

        public bool CheckToyIsBorrowedByStudent(int studentId, int toyId)
        {
            return _context.ToyLoans.Any(tl => tl.StudentId == studentId && tl.ToyId == toyId && tl.ReturnDate == null);
        }

        public bool HasActiveLoanForToy(int toyId)
        {
            return _context.ToyLoans.Any(tl => tl.ToyId == toyId && tl.ReturnDate == null);
        }

        public Task<Student?> FindByIdAsync(int id)
        {
            return _context.Students
                .Include(s => s.User)
                .Include(s => s.ToyLoans)
                .ThenInclude(tl => tl.Toy)
                .FirstOrDefaultAsync(s => s.StudentId == id);
        }

        public Student? FindById(int id)
        {
            return _context.Students
                .Include(s => s.User)
                .Include(s => s.ToyLoans)
                .ThenInclude(tl => tl.Toy)
                .FirstOrDefault(s => s.StudentId == id);
        }

        public Task<List<Student>> GetAllAsync()
        {
            return _context.Students
                .Include(s => s.User)
                .Include(s => s.ToyLoans)
                .ThenInclude(tl => tl.Toy)
                .OrderBy(s => s.User.UserId)
                .ToListAsync();
        }

        public List<Student> GetAll()
        {
            return _context.Students
                .Include(s => s.User)
                .Include(s => s.ToyLoans)
                .ThenInclude(tl => tl.Toy)
                .OrderBy(s => s.User.UserId)
                .ToList();
        }

        public Task<int?> FindByOidAsync(string oid)
        {
            return _context.Students
                .Where(s => s.User.Oid == oid)
                .Select(s => (int?)s.StudentId)
                .FirstOrDefaultAsync();
        }
        public int? FindByOid(string oid)
        {
            return _context.Students
               .Where(s => s.User.Oid == oid)
               .Select(s => s.StudentId)
               .FirstOrDefault();
        }

        public Task<List<ToyLoan>> GetBorrowHistoryAsync(int studentId)
        {
            return _context.ToyLoans
                .Include(tl => tl.Toy)
                .Where(tl => tl.StudentId == studentId)
                .OrderByDescending(tl => tl.BorrowDate)
                .ToListAsync();
        }

        public List<ToyLoan> GetBorrowHistory(int studentId)
        {
            return _context.ToyLoans
                .Include(tl => tl.Toy)
                .Where(tl => tl.StudentId == studentId)
                .OrderByDescending(tl => tl.BorrowDate)
                .ToList();
        }

        public Task<bool> CheckToyIsBorrowedByStudentAsync(int studentId, int toyId)
        {
            return _context.ToyLoans.AnyAsync(tl => tl.StudentId == studentId && tl.ToyId == toyId && tl.ReturnDate == null);
        }
    }
}
