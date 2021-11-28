using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace vrhero.VirtualHere
{
    internal class NonWindowsClient : IClient
    {
        public int Timeout { get; set; }

        public NonWindowsClient(int timeout = Constants.DefaultTimeout)
        {
            Timeout = timeout;
        }

        public async Task<string> QueryAsync(string command)
        {
            var start = System.Diagnostics.Stopwatch.StartNew();
            var cacheKey = $"NonWindowsClient:QueryAsync";
            if (!MemoryCacheManager.Default.TryGet(cacheKey, out string result))
            {
                MemoryCacheManager.Default.SetNotExpire(cacheKey, string.Empty);
                try
                {
                    using (var inputStream = File.Open(Constants.DefaultInputStreamFile, FileMode.Truncate))
                    {
                        using (var outputStream = File.Open(Constants.DefaultOutputStreamFile, FileMode.Truncate))
                        {
                            using (var writer = new StreamWriter(inputStream))
                            {
                                using (var reader = new StreamReader(outputStream))
                                {
                                    await writer.WriteLineAsync(command.ToCharArray());
                                    await writer.FlushAsync();
                                    var builder = new StringBuilder();
                                    var cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(Timeout));
#if NET5_0_OR_GREATER
                                    var buffer = new Memory<char>(new char[1], 0, 1);
#else
                                    var buffer = new char[1024];
#endif
                                    while (reader.Peek() > 1)
                                    {
                                        try
                                        {
#if NET5_0_OR_GREATER
                                            var readCount = await reader.ReadAsync(buffer, cancellationToken.Token);
                                            builder.Append(buffer.ToString());
#else
                                            var readCount = await reader.ReadAsync(buffer, 0, buffer.Length);
                                            builder.Append(buffer.Take(readCount).ToString());
#endif
                                            if (start.ElapsedMilliseconds > Timeout)
                                            {
                                                break;
                                            }
                                        }
                                        catch (TaskCanceledException ex)
                                        {
                                            break;
                                        }
                                    }
                                    writer.Close();
                                    reader.Close();
                                    inputStream.Close();
                                    outputStream.Close();
                                    result = builder.ToString();
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                }
                finally
                {
                    MemoryCacheManager.Default.Remove(cacheKey);
                }
            }
            return result;
        }

        public void Dispose()
        {
        }
    }
}
