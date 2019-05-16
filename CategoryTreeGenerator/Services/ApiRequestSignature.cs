using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CategoryTreeGenerator.Services
{
    public class ApiRequestSignature
    {
        public string AppId { get; set; }
        public string Hash { get; set; }
        public DateTime Timestamp { get; private set; }
        public string TimestampString { get; private set; }

        public ApiRequestSignature()
        {
            Timestamp = DateTime.UtcNow;
            TimestampString = Timestamp.ToString("o", CultureInfo.InvariantCulture);
        }

        public static bool TryParse(string input, out ApiRequestSignature parsedValue)
        {
            parsedValue = null;
            bool success = false;

            if (input != null)
            {
                string[] parts = input.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 3)
                {
                    if (parts[2].Length == 64)
                    {
                        if (DateTime.TryParseExact(
                            parts[1],
                            "o",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AdjustToUniversal,
                            out DateTime timestamp))
                        {
                            parsedValue = new ApiRequestSignature
                            {
                                AppId = parts[0],
                                TimestampString = parts[1],
                                Hash = parts[2],
                                Timestamp = timestamp,
                            };
                            success = true;
                        }
                    }
                }
            }

            return success;
        }

        public override string ToString()
        {
            return string.Join(";", AppId, TimestampString, Hash);
        }
    }

    public class NameValuePair
    {
        public string Name { get; private set; }
        public string Value { get; private set; }

        public NameValuePair(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            builder.Append(Name);
            builder.Append(", ");
            builder.Append(Value);
            builder.Append("]");
            return builder.ToString();
        }
    }

    public static class HmacUtility
    {
        public const string NameValueSeparator = "=";
        public const string ParameterSeparator = "&";

        public static string ComputeHash(Func<byte[], HMAC> hmacFactory, string secretKey, string data)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] keyBytes = ConvertHexStringToBytes(secretKey);
            return ComputeHash(hmacFactory, keyBytes, dataBytes);
        }

        public static string ComputeHash(Func<byte[], HMAC> hmacFactory, byte[] secretKey, byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            using (HMAC hmac = hmacFactory(secretKey))
            {
                byte[] hash = hmac.ComputeHash(data, 0, data.Length);
                return ConvertBytesToHexString(hash);
            }
        }

        public static string GetHashString(Func<byte[], HMAC> hmacFactory, string secretKey, NameValuePair[] parameters)
        {
            string data = BuildDataString(parameters);
            return ComputeHash(hmacFactory, secretKey, data);
        }


        private static string BuildDataString(NameValuePair[] parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            StringBuilder builder = new StringBuilder();

            System.Collections.Generic.List<NameValuePair> orderedParameters =
                parameters.Where(p => string.IsNullOrEmpty(p.Name) && !string.IsNullOrEmpty(p.Value))
                    .Union(
                        parameters.Where(p => !string.IsNullOrEmpty(p.Name) && !string.IsNullOrEmpty(p.Value))
                            .OrderBy(i => i.Name))
                    .ToList();

            foreach (NameValuePair parameter in orderedParameters)
            {
                if (builder.Length > 0)
                {
                    builder.Append(ParameterSeparator);
                }

                if (!string.IsNullOrEmpty(parameter.Name))
                {
                    builder.Append(parameter.Name);
                    builder.Append(NameValueSeparator);
                }

                builder.Append(parameter.Value);
            }

            string data = builder.ToString();
            return data;
        }

        private static string ConvertBytesToHexString(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();

            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private static byte[] ConvertHexStringToBytes(string hexString)
        {
            return
                Enumerable.Range(0, hexString.Length)
                    .Where(i => i % 2 == 0)
                    .Select(i => Convert.ToByte(hexString.Substring(i, 2), 16))
                    .ToArray();
        }
    }
}