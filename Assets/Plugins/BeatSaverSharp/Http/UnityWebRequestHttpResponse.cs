#if RELEASE_UNITY
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace BeatSaverSharp.Http
{
    internal class UnityWebRequestHttpResponse : IHttpResponse
    {
        public int Code { get; }
        public byte[] Bytes { get; }
        public bool Successful { get; }

        public UnityWebRequestHttpResponse(UnityWebRequest unityWebRequest, bool successful)
        {
            Successful = successful;
            Code = (int)unityWebRequest.responseCode;
            Bytes = unityWebRequest.downloadHandler.data;
        }

        public UniTask<byte[]> ReadAsByteArrayAsync()
        {
            return UniTask.FromResult(Bytes);
        }

        public UniTask<string> ReadAsStringAsync()
        {
            return UniTask.FromResult(Encoding.UTF8.GetString(Bytes));
        }

        public async UniTask<T> ReadAsObjectAsync<T>() where T : class
        {
            var str = await ReadAsStringAsync();//.ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(str);
        }
    }
}
#endif