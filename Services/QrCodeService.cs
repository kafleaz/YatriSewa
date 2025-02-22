using System.Net.Http;
using System.Threading.Tasks;

public class QrCodeService
{
    private readonly HttpClient _httpClient;

    public QrCodeService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]> GenerateQRCodeAsync(string qrText)
    {
        var url = $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(qrText)}";
        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsByteArrayAsync();
        }

        throw new HttpRequestException("Failed to generate QR code.");
    }
}
