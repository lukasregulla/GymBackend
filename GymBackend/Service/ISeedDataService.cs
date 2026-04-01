namespace GymBackend.Service
{
    public interface ISeedDataService
    {
        Task SeedForUserAsync(int userId);
    }
}
