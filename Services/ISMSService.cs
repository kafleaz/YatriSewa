namespace YatriSewa.Services
{
    public interface ISMSService
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message);
    }
}
