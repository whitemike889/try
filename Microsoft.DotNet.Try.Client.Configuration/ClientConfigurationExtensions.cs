using System;
using System.Security.Cryptography;
using System.Text;
using Recipes;

namespace Microsoft.DotNet.Try.Client.Configuration
{
    public static class ClientConfigurationExtensions
    {
        private static string ToSha256(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var inputBytes = Encoding.UTF8.GetBytes(value);

            byte[] hash;
            using (var sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(inputBytes);
            }

            return Convert.ToBase64String(hash);
        }
        public static string ComputeHash(this RequestDescriptors links)
        {
            return ToSha256(links.ToJson());
        }
    }
}