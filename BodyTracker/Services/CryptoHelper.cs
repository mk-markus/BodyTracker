using System;
using System.Security.Cryptography;
using System.Text;


namespace BodyTracker.Services
{
    /// <summary>
    /// Provides static utility methods for symmetric encryption and decryption using the AES algorithm.
    /// Handles the conversion between byte arrays and Base64-encoded strings for easy storage.
    /// </summary>
    public static class CryptoHelper
    {

        /// <summary>
        /// Encrypts a plain-text string using AES symmetric encryption.
        /// </summary>
        /// <param name="plainText">The sensitive string to be encrypted (e.g., a password).</param>
        /// <param name="keyBase64">The Base64-encoded encryption key.</param>
        /// <param name="ivBase64">The Base64-encoded initialization vector (IV).</param>
        /// <returns>A Base64-encoded string representing the encrypted cipher text.</returns>
        /// <remarks>
        /// Uses UTF-8 encoding for string-to-byte conversion before encryption.
        /// </remarks>
        public static string Encrypt(string plainText, string keyBase64, string ivBase64)
        {
            var key = Convert.FromBase64String(keyBase64);
            var iv = Convert.FromBase64String(ivBase64);
            using var aes = Aes.Create();
            aes.Key = key; aes.IV = iv;
            using var encryptor = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var cipher = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToBase64String(cipher);
        }

        /// <summary>
        /// Decrypts an AES-encrypted Base64 string back into its original plain-text representation.
        /// </summary>
        /// <param name="cipherText">The Base64-encoded cipher text to decrypt.</param>
        /// <param name="keyBase64">The Base64-encoded encryption key used during encryption.</param>
        /// <param name="ivBase64">The Base64-encoded initialization vector (IV) used during encryption.</param>
        /// <returns>The decrypted plain-text string.</returns>
        /// <exception cref="CryptographicException">Thrown if the key or IV is incorrect.</exception>
        public static string Decrypt(string cipherText, string keyBase64, string ivBase64)
        {
            var key = Convert.FromBase64String(keyBase64);
            var iv = Convert.FromBase64String(ivBase64);
            using var aes = Aes.Create();
            aes.Key = key; aes.IV = iv;
            using var decryptor = aes.CreateDecryptor();
            var bytes = Convert.FromBase64String(cipherText);
            var plain = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Encoding.UTF8.GetString(plain);
        }

        /// <summary>
        /// Generates a new, cryptographically strong random Key and Initialization Vector (IV).
        /// </summary>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><description><c>key</c>: A Base64-encoded 256-bit key (default for AES).</description></item>
        /// <item><description><c>iv</c>: A Base64-encoded 128-bit initialization vector.</description></item>
        /// </list>
        /// </returns>
        public static (string key, string iv) GenerateKeyIv()
        {
            using var aes = Aes.Create();
            aes.GenerateKey(); aes.GenerateIV();
            return (Convert.ToBase64String(aes.Key), Convert.ToBase64String(aes.IV));
        }
    }
}
