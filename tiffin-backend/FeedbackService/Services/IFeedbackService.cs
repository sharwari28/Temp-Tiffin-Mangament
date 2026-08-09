using FeedbackService.DTOs;

namespace FeedbackService.Services
{
    public interface IFeedbackService
    {
        Task<ApiResponse> SubmitFeedbackAsync(string customerEmail, FeedbackRequestDto request);

        Task<ApiResponse> GetAllFeedbackAsync();

        Task<ApiResponse> GetFeedbackByCustomerAsync(string customerEmail);

    }
}