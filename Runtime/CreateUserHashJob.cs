using System.Security.Cryptography;
using System.Text;
using Unity.Collections;
using Unity.Jobs;

namespace TelemetryClient
{
    internal struct CreateUserHashJob : IJob
    {
        public const int UserHashStringLength = 64;

        [ReadOnly]
        public NativeArray<char> userIdentifier;

        public NativeArray<char> userHash;

        public void Execute()
        {
            var array = ComputeSha256Hash(userIdentifier.ToArray(), Encoding.Unicode).ToCharArray();
            userHash.CopyFrom(array);
        }

        /// <summary>
        /// Computes SHA256 Hash using .NET Cryptography library.
        /// </summary>
        /// <param name="rawData">Input data to be hashed.</param>
        /// <returns></returns>
        static string ComputeSha256Hash(char[] rawData, Encoding encoding)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(encoding.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    // x2 means two-digit hexadecimal string
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}