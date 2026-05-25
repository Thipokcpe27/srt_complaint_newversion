namespace SRT.Complaint.Services;

public interface IMaskingService
{
    string MaskName(string fullName);
    string MaskPhone(string phone);
    string? MaskEmail(string? email);
    byte[] Encrypt(string plaintext);
    string Decrypt(byte[] ciphertext);
}
