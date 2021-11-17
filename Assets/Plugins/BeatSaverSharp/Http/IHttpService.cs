using System.Threading;
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace BeatSaverSharp.Http
{
    public interface IHttpService
    {
        UniTask<IHttpResponse> GetAsync(string url, CancellationToken token = default, IProgress<double>? progress = null);
        UniTask<IHttpResponse> PostAsync(string url, object? body = null, CancellationToken token = default);
    }
}