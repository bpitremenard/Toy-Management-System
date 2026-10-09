using System.Threading.Tasks;
using TMS_SharedLibrary.Models;
using TMSTeacher.Services;

namespace TMSTeacher.Models
{
    public class ToyWithStudentViewModel
    {
        public Toy Toy { get; set; } = null!;
        public Student? ReservedByStudent { get; set; }
        public string? StudentName { get; private set; }

        public async Task LoadStudentNameAsync(IGraphAPIService graphAPIService)
        {
            if (ReservedByStudent?.User?.Oid != null)
            {
                var info = await graphAPIService.GetStudentInfoByOidAsync(ReservedByStudent.User.Oid);

                if (info != null && (!string.IsNullOrEmpty(info.FirstName) || !string.IsNullOrEmpty(info.LastName)))
                {
                    StudentName = $"{info.FirstName} {info.LastName}".Trim();
                }
                else
                {
                    StudentName = $"Student (OID: {ReservedByStudent.User.Oid.Substring(0, Math.Min(8, ReservedByStudent.User.Oid.Length))}...)";
                }
            }
        }
    }
}