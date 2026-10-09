using TMS_SharedLibrary.Models;

namespace TMS_API.DAL
{
    public interface IUserRepo
    {
        User? FindById(int id);
        User? FindByOid(string oid);
        List<User> GetAll();
        User Create(User user);

        Task<User?> FindByIdAsync(int id);
        Task<User?> FindByOidAsync(string oid);
        Task<List<User>> GetAllAsync();
        Task<User> CreateAsync(User user);
    }
}
