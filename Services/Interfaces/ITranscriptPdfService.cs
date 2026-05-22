namespace UM_Project.Services.Interfaces
{
    public interface ITranscriptPdfService
    {
        Task<byte[]> GenerateTranscriptAsync(int studentId);
    }
}
