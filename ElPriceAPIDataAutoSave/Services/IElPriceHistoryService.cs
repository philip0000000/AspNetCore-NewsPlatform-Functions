
namespace ElPriceAPIDataAutoSave.Services
{
    public interface IElPriceHistoryService
    {
        Task EnsureTableAsync();
        Task AddSnapshotAsync(ElPriceViewModel dto);
        Task<IEnumerable<ElPriceViewModel>> GetLatestSnapshotsAsync(int count = 10);
        Task<IEnumerable<ElPriceViewModel>> GetSnapshotsByDateAsync(string date);
    }
}
