using AllocationSystem.WebApi.Models;

namespace AllocationSystem.WebApi.Business
{
    public interface IAutoAllocation
    {
        Task<AllocationHistory> RunAllocationProcess(long UserID);
    }
}
