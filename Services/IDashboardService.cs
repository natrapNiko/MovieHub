using MovieHub.ViewModels;

namespace MovieHub.Services;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync();
}
