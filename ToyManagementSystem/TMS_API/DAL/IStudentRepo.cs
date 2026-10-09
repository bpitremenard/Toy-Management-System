using TMS_SharedLibrary.Models;

namespace TMS_API.DAL
{
    public interface IStudentRepo
    {
        List<Student> GetAll();
        Student? FindById(int id);
        void BookToy(int studentId, int toyId);
        void ReturnToy(int studentId, int toyId);
        bool CheckToyIsBorrowedByStudent(int studentId, int toyId);
        bool HasActiveLoanForToy(int toyId);
        List<ToyLoan> GetBorrowHistory(int studentId);
        int? FindByOid(string oid);

        Task<List<Student>> GetAllAsync();
        Task<Student?> FindByIdAsync(int id);
        Task BookToyAsync(int studentId, int toyId);
        Task ReturnToyAsync(int studentId, int toyId);
        Task<bool> CheckToyIsBorrowedByStudentAsync(int studentId, int toyId);
        Task<List<ToyLoan>> GetBorrowHistoryAsync(int studentId);
        Task<int?> FindByOidAsync(string oid);
    }
}
