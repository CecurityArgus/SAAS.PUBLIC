using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global

namespace Argus.MessageQueue.Service.Receive
{
    public static class Hasher
    {
        public static string Md5(string text)
        {
            using (var algo = new MD5CryptoServiceProvider())
            {
                return GenerateHashString(algo, text);
            }
        }

        public static string Sha1(string text)
        {
            using (var algo = new SHA1Managed())
            {
                return GenerateHashString(algo, text);
            }
        }

        public static string Sha256(byte[] bytes)
        {
            using (var algo = new SHA256Managed())
            {
                return GenerateHashString(algo, bytes: bytes);
            }
        }

        public static string Sha384(string text)
        {
            using (var algo = new SHA384Managed())
            {
                return GenerateHashString(algo, text);
            }
        }

        public static string Sha512(string text)
        {
            using (var algo = new SHA512Managed())
            {
                return GenerateHashString(algo, text);
            }
        }

        private static string GenerateHashString(HashAlgorithm algo, string text = null, byte[] bytes = null)
        {
            // Compute hash from text parameter
            if (bytes != null)
            {
                algo.ComputeHash(bytes);
            }
            else if (text != null)
            {
                algo.ComputeHash(Encoding.UTF8.GetBytes(text));
            }
            else
            {
                throw new Exception("no valid text or bytes");
            }

            // Get has value in array of bytes
            var result = algo.Hash;

            // Return as hexadecimal string
            return string.Join(string.Empty, result.Select(x => x.ToString("x2")));
        }
    }
}