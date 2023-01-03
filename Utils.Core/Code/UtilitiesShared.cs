using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NodaTime.Extensions;

using Utils.Core.Classes;
using Utils.Core.StaticData;

namespace Utils.Core.Code
{
    public static class UtilitiesShared
    {
        #region serialization
        public static T FromJson<T>(this string InputJson)/* where T : class*/
        {
            T res = default(T);

            if (!string.IsNullOrWhiteSpace(InputJson))
            {
                try
                {
                    res = JsonConvert.DeserializeObject<T>(InputJson);
                }
                catch { }
            }
            return res;
        }

        public static string ToJson(this object InputObject)
        {
            if (InputObject == null)
            {
                return "";
            }

            return JsonConvert.SerializeObject(InputObject);
        }

        public static T GetProperty<T>(this string InputJson, string PropertyName)
        {
            T res = default(T);

            var jObject = JObject.Parse(InputJson);

            if (jObject.HasValues)
            {
                res = jObject[PropertyName].ToString().FromJson<T>();
            }

            return res;
        }

        public static string GetPropertyJson(this string InputJson, string PropertyName)
        {
            string res = string.Empty;

            var jObject = JObject.Parse(InputJson);

            if (jObject.HasValues)
            {
                res = jObject.GetValue(PropertyName).ToString();
            }

            return res;
        }
        #endregion


        #region IO
        public static string GetFileTempPath(string FileExtention)
        {
            if (!FileExtention.StartsWith('.'))
            {
                FileExtention = $".{FileExtention}";
            }

            string filename = "Temp_" + DateTime.Now.ToShortDateString() +
                                      DateTime.Now.ToLongTimeString() + FileExtention;
            filename = Path.GetInvalidFileNameChars().
                Aggregate(filename, (current, c) => current.Replace(c.ToString(), "_"));
            string filepath = Path.Combine(Path.GetTempPath(), filename);

            return filepath;
        }

        public static string GetFileRandomPath(string FileExtention)
        {
            if (!FileExtention.StartsWith('.'))
            {
                FileExtention = $".{FileExtention}";
            }

            string filename = $"Temp_{Path.GetRandomFileName()}{FileExtention}";

            filename = Path.GetInvalidFileNameChars().
                Aggregate(filename, (current, c) => current.Replace(c.ToString(), "_"));
            string filepath = Path.Combine(Path.GetTempPath(), filename);

            return filepath;
        }
        #endregion


        #region Converters
        public static T ConvertTo<T>(this object InputObject)
        {
            try
            {
                Type resultType = typeof(T);
                Type unerlyingType = Nullable.GetUnderlyingType(resultType);

                if (unerlyingType?.BaseType == typeof(Enum) || resultType.BaseType == typeof(Enum))
                {
                    return (T)Enum.Parse(unerlyingType ?? resultType, InputObject?.ToString());
                }

                if (InputObject == null)
                    return default(T);
                else
                {
                    return unerlyingType != null ? (T)Convert.ChangeType(InputObject, unerlyingType) : (T)Convert.ChangeType(InputObject, resultType);
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                return default(T);
            }
        }

        public static T CloneObjectJson<T>(this object InputObject) where T : class
        {
            T res = default(T);

            if (InputObject != null)
                res = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(InputObject));

            return res;
        }

        public static string BytesToString(this byte[] InputBytes)
        {
            using (var stream = new MemoryStream(InputBytes))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        public static double? ToUnixMilliseconds(this DateTime? InputDate)
        {
            if (InputDate.HasValue)
            {
                var localDate = DateTime.SpecifyKind(InputDate.Value, DateTimeKind.Local);

                return (InputDate.Value - StaticDataShared.Jan1st1970).TotalMilliseconds;
            }
            else
            {
                return null;
            }
        }

        public static double? ToUnixMillisecondsUTC(this DateTime? InputDate)
        {
            if (InputDate.HasValue)
            {
                var localDate = DateTime.SpecifyKind(InputDate.Value, DateTimeKind.Local).ToLocalDateTime();
                var utcDate = localDate.InZoneLeniently(StaticDataShared.TbilisiDateTimeZone).ToDateTimeUtc();

                return (utcDate - StaticDataShared.Jan1st1970).TotalMilliseconds;
            }
            else
            {
                return null;
            }
        }

        public static DateTime? FromUnixMilliseconds(double? UnixMilliseconds)
        {
            if (UnixMilliseconds.HasValue)
            {
                var localDate = StaticDataShared.Jan1st1970.AddMilliseconds(UnixMilliseconds.Value);
                return DateTime.SpecifyKind(localDate, DateTimeKind.Local);
            }
            else
            {
                return null;
            }
        }

        public static DateTime? FromUnixMillisecondsUTC(double? UnixMillisecondsUTC)
        {
            if (UnixMillisecondsUTC.HasValue)
            {
                var utcDate = StaticDataShared.Jan1st1970.AddMilliseconds(UnixMillisecondsUTC.Value);
                var localDate = utcDate.ToInstant().InZone(StaticDataShared.TbilisiDateTimeZone).ToDateTimeUnspecified();

                return DateTime.SpecifyKind(localDate, DateTimeKind.Local);
            }
            else
            {
                return null;
            }
        }

        public static string ToHttpCodeString(this HttpStatusCode Code)
        {
            return ((int)Code).ToString();
        }

        public static int ToInt(this Enum EnumValue)
        {
            return EnumValue.ConvertTo<int>();
        }

        public static Dictionary<string, string> ToKeyValuePair(this object InputObject, List<string> ExcludeFields = null)
        {
            var objectParams = new Dictionary<string, string>();
            string key = string.Empty, value = string.Empty;

            foreach (var property in InputObject.GetType().GetProperties())
            {
                if (ExcludeFields == null || !ExcludeFields.Contains(property.Name))
                {
                    key = property.Name;

                    if (property.PropertyType.IsGenericType)
                    {
                        var list = property.GetValue(InputObject) as List<string>;

                        if (list != null)
                        {
                            value = string.Join(",", list);
                        }
                        else
                        {
                            value = string.Empty;
                        }
                    }
                    else
                    {
                        value = property.GetValue(InputObject)?.ToString();
                    }

                    if (!value.IsEmpty())
                    {
                        objectParams.Add(key, value);
                    }
                }
            }

            return objectParams;
        }

        public static bool ValidateTemplateParameters(List<string> TemplateParams, Dictionary<string, string> ParamsValues)
        {
            foreach (var Param in TemplateParams)
            {
                if (!ParamsValues.ContainsKey(Param))
                {
                    return false;
                }
            }

            return true;
        }

        public static string ToMD5(this string InputString)
        {
            StringBuilder sBuilder = new StringBuilder();
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] hashData = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(InputString));

                for (int i = 0; i < hashData.Length; i++)
                {
                    sBuilder.Append(hashData[i].ToString("x2"));
                }
            }

            var hash = sBuilder.ToString();

            return hash;
        }

        public static string ToSHA256(this string InputString)
        {
            StringBuilder sBuilder = new StringBuilder();
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] hashData = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(InputString));

                for (int i = 0; i < hashData.Length; i++)
                {
                    sBuilder.Append(hashData[i].ToString("x2"));
                }
            }

            var hash = sBuilder.ToString();

            return hash;
        }

        public static string ToHmacSHA256(this string InputString, string Secret)
        {
            StringBuilder sBuilder = new StringBuilder();

            using (HMACSHA256 hmacSha256Hash = new HMACSHA256((Encoding.UTF8.GetBytes(Secret))))
            {
                byte[] hashData = hmacSha256Hash.ComputeHash(Encoding.UTF8.GetBytes(InputString));

                for (int i = 0; i < hashData.Length; i++)
                {
                    sBuilder.Append(hashData[i].ToString("x2"));
                }
            }

            var hash = sBuilder.ToString();

            return hash;
        }

        #endregion


        #region method tracking
        public static string GetMethodFullName(this MethodBase methodBase, [CallerMemberName] string methodName = null)
        {
            string namespacename = methodBase?.DeclaringType?.DeclaringType?.FullName ?? methodBase?.DeclaringType?.FullName;
            return $"{namespacename}.{methodName}";
        }
        #endregion


        #region async
        public static async Task<TResult> TimeoutAfterAsync<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();

                    return await task;
                }
                else
                {
                    throw new TimeoutException("The operation has timed out.");
                }
            }
        }
        #endregion


        #region value checkers
        public static bool IsEmpty(this string InputText)
        {
            return string.IsNullOrWhiteSpace(InputText);
        }

        public static bool IsNotEmpty(this string InputText)
        {
            return !string.IsNullOrWhiteSpace(InputText);
        }

        public static bool EqualsIgnoreCase(this string string1, string string2)
        {
            if (string1.IsEmpty() || string2.IsEmpty())
            {
                return false;
            }

            return string1.Trim().Equals(string2.Trim(), StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsNotEmpty(this decimal? num)
        {
            return num.HasValue && (num.Value > 0);
        }

        public static bool IsEmpty(this decimal? num)
        {
            return !num.IsNotEmpty();
        }

        public static bool IsNotEmpty(this int? num)
        {
            return num.HasValue && (num.Value > 0);
        }

        public static bool IsEmpty(this int? num)
        {
            return !num.IsNotEmpty();
        }

        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException ex)
            {
                var message = ex.Message;

                return false;
            }
            catch (ArgumentException ex)
            {
                var message = ex.Message;

                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
        #endregion


        #region monitoring

        public static ServerResourceInfo GetServerResourceInfo(List<Func<ModuleStatusInfo>> ModuleCheckFuncs)
        {
            var timeOut = TimeSpan.FromMilliseconds(10000);

            var serverResourceInfo = new ServerResourceInfo
            {
                serverName = Environment.MachineName,
                isServerUnReachable = false,
                moduleStatusInfos = UtilitiesLocal.checkModules(ModuleCheckFuncs, timeOut)
            };

            return serverResourceInfo;
        }

        #endregion


        #region text compression
        public static string CompressString(this string InputString)
        {
            string compressedString = null;

            if (InputString.IsNotEmpty())
            {
                byte[] buffer = Encoding.UTF8.GetBytes(InputString);

                byte[] gzBuffer = buffer.CompressString();

                compressedString = Convert.ToBase64String(gzBuffer);
            }

            return compressedString;
        }


        public static byte[] CompressString(this byte[] InputData)
        {
            byte[] gzBuffer = null;

            byte[] buffer = InputData;

            if (buffer != null)
            {
                using (var ms = new MemoryStream())
                {
                    using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
                    {
                        zip.Write(buffer, 0, buffer.Length);
                    }

                    ms.Position = 0;

                    byte[] compressed = new byte[ms.Length];
                    ms.Read(compressed, 0, compressed.Length);

                    gzBuffer = new byte[compressed.Length + 4];
                    Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
                    Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
                }
            }

            return gzBuffer;
        }


        public static string DecompressString(this string InputCompressedString)
        {
            string decompressedString = null;

            if (InputCompressedString.IsNotEmpty())
            {
                byte[] gzBuffer = Convert.FromBase64String(InputCompressedString);

                byte[] buffer = gzBuffer.DecompressString();

                decompressedString = Encoding.UTF8.GetString(buffer);
            }

            return decompressedString;
        }


        public static byte[] DecompressString(this byte[] InputCompressedData)
        {
            byte[] buffer = null;

            byte[] gzBuffer = InputCompressedData;

            if (gzBuffer != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    int msgLength = BitConverter.ToInt32(gzBuffer, 0);

                    ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                    buffer = new byte[msgLength];

                    ms.Position = 0;

                    using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                    {
                        zip.Read(buffer, 0, buffer.Length);
                    }
                }
            }

            return buffer;
        }
        #endregion
    }
}
