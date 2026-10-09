using TMS_SharedLibrary.Models;

namespace TMS_API.DAL;

public interface INotificationRepo
{
    IEnumerable<Notification> GetAll();
    Notification? FindById(int id);
    IEnumerable<Notification> GetByRecipientId(int recipientId);
    Notification Create(Notification notification);
    void Update(Notification notification);
    void Delete(int id);

    Task<List<Notification>> GetAllAsync();
    Task<Notification?> FindByIdAsync(int id);
    Task<List<Notification>> GetByRecipientIdAsync(int recipientId);
    Task<Notification> CreateAsync(Notification notification);
    Task UpdateAsync(Notification notification);
    Task DeleteAsync(int id);
}
