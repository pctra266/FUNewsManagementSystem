
namespace BussinessLogic.Services
{
    public interface IAIService
    {
        Task<Dictionary<string, double>> SuggestTagsAsync(string content, int? userId = null);
    }
}
