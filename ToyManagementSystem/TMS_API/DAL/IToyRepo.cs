using TMS_SharedLibrary.Models;

namespace TMS_API.DAL
{
    public interface IToyRepo
    {
        List<Toy> GetAll();
        List<Toy> GetFiltered(IEnumerable<string>? materials, IEnumerable<string>? categories);
        Toy? FindById(int id);
        void Add(Toy toy);
        void Delete(int id);
        void Update(int id, Toy toy);

        Task<List<Toy>> GetAllAsync();
        Task<List<Toy>> GetFilteredAsync(IEnumerable<string>? materials, IEnumerable<string>? categories);
        Task<Toy?> FindByIdAsync(int id);
        Task AddAsync(Toy toy);
        Task DeleteAsync(int id);
        Task UpdateAsync(int id, Toy toy);
    }
}
