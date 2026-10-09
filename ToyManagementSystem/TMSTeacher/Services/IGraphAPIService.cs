namespace TMSTeacher.Services
{
    public interface IGraphAPIService
    {
        Task<StudentGraphInfo?> GetStudentInfoByOidAsync(string oid);
    }
}