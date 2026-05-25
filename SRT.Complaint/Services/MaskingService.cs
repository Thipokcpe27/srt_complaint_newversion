using System.Security.Cryptography;
using System.Text;

namespace SRT.Complaint.Services;

public class MaskingService(IConfiguration config) : IMaskingService
{
    private readonly byte[] _key = Convert.FromBase64String(
        config["Encryption:Key"] ?? throw new InvalidOperationException("Encryption:Key is not configured"));

    public string MaskName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return fullName;
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", parts.Select(p =>
            p.Length <= 1 ? p : $"{p[0]}{new string('*', p.Length - 1)}"));
    }

    public string MaskPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 4) return phone;
        var digits = phone.Replace("-", "").Replace(" ", "");
        return digits.Length <= 4
            ? new string('x', digits.Length - 2) + digits[^2..]
            : $"{digits[..3]}-xxx-x{digits[^3..]}";
    }

    public string? MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return email;
        var atIndex = email.IndexOf('@');
        if (atIndex <= 1) return email;
        return $"{email[0]}{new string('*', atIndex - 1)}{email[atIndex..]}";
    }

    public byte[] Encrypt(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
        // Prepend IV to ciphertext
        var result = new byte[aes.IV.Length + ciphertext.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(ciphertext, 0, result, aes.IV.Length, ciphertext.Length);
        return result;
    }

    public string Decrypt(byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        var iv = new byte[aes.BlockSize / 8];
        var ciphertext = new byte[data.Length - iv.Length];
        Buffer.BlockCopy(data, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(data, iv.Length, ciphertext, 0, ciphertext.Length);
        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        var plaintext = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
        return Encoding.UTF8.GetString(plaintext);
    }
}
