using AllocationSystem.WebApi.Models;

namespace AllocationSystem.WebApi.Business
{
    public interface IBusiness
    {
        Task<IEnumerable<SupervisorTopicDto>> GetSelectedList(long id, long UserID);
        Task<IEnumerable<long>> GetSelectedStudentPrefList(long id);

    }
}
