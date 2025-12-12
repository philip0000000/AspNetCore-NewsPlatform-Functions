using Azure.Data.Tables;

namespace AFHPNewsFunctions.Services
{
    public interface ITableStorageService
    {
        Task<bool> Add(List<ITableEntity> entities);

        Task<bool> AddBatch(List<ITableEntity> entities);
    }
}
