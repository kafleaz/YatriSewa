using Newtonsoft.Json;
using System.Text;
using YatriSewa.Models;

namespace YatriSewa.Services
{
    public class TokenService
    {
        public string GenerateToken(TokenData data)
        {
            string jsonData = JsonConvert.SerializeObject(data);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonData);
            return Convert.ToBase64String(bytes);
        }

        public TokenData DecodeToken(string token)
        {
            byte[] bytes = Convert.FromBase64String(token);
            string jsonData = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<TokenData>(jsonData);
        }

    }
}
