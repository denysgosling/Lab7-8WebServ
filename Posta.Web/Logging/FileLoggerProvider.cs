using System.Text;
using System.Text.Json;
namespace Posta.Web.Logging;
public sealed class FileLoggerProvider(string directory) : ILoggerProvider
{
    private readonly object gate = new();
    private readonly string logDirectory = directory;
    public ILogger CreateLogger(string categoryName) => new FileLogger(this, categoryName);
    public void Dispose() { }
    private sealed class FileLogger(FileLoggerProvider owner, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel level) => level >= LogLevel.Information && level != LogLevel.None;
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? error, Func<TState, Exception?, string> formatter) {
            if (!IsEnabled(level)) return;
            var now = DateTimeOffset.Now;
            var entry = JsonSerializer.Serialize(new { timestamp = now, level = level.ToString(), category, message = formatter(state, error), exception = error?.ToString() });
            lock (owner.gate) {
                Directory.CreateDirectory(owner.logDirectory);
                File.AppendAllText(Path.Combine(owner.logDirectory, $"requests-{now:yyyy-MM-dd}.log"), entry + Environment.NewLine, Encoding.UTF8);
            }
        }
    }
}
