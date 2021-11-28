using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace vrhero.VirtualHere
{
    internal class WindowsClient : IClient
    {
        private readonly NamedPipeClientStream pipeStream;
        private StreamWriter streamWriter;
        private StreamReader streamReader;

        public int Timeout { get; set; }

        public WindowsClient(int timeout = Constants.DefaultTimeout)
        {
            pipeStream = new NamedPipeClientStream(Constants.DefaultPipeServer, Constants.DefaultPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            Timeout = timeout;
        }

        public async Task<string> QueryAsync(string command)
        {
            var start = System.Diagnostics.Stopwatch.StartNew();
            var cacheKey = $"WindowsClient:QueryAsync";
            while (MemoryCacheManager.Default.TryGet(cacheKey, out string lastCommand))
            {
                System.Threading.Thread.Sleep(100);
                if (command == lastCommand)
                {
                    return string.Empty;
                }
            }
            var buffer = new StringBuilder();
            try
            {
                MemoryCacheManager.Default.SetNotExpire(cacheKey, command);
                if (!pipeStream.IsConnected)
                {
                    await pipeStream.ConnectAsync(Timeout);
                }
                streamWriter = new StreamWriter(pipeStream);
                streamWriter.AutoFlush = true;
                streamReader = new StreamReader(pipeStream);
                streamWriter.WriteLine(command);
                while (streamReader.Peek() > 1)
                {
                    try
                    {
#if NET5_0_OR_GREATER
                        var cancellationToken = new System.Threading.CancellationTokenSource(TimeSpan.FromMilliseconds(Timeout));
                        var bytes = new Memory<char>(new char[1], 0, 1);
                        await streamReader.ReadAsync(bytes, cancellationToken.Token);
                        buffer.Append(bytes.ToString());
#else
                        var bytes = new char[1024];
                        var readCount = await streamReader.ReadAsync(bytes, 0, bytes.Length);
                        buffer.Append(bytes, 0, readCount);
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
            }
            catch(Exception ex)
            {
            }
            finally
            {
                MemoryCacheManager.Default.Remove(cacheKey);
            }
            return buffer.ToString();
        }

        public void Dispose()
        {
            streamReader?.Close();
            streamReader?.Dispose();
            pipeStream?.Close();
            pipeStream?.Dispose();
        }
    }
}
