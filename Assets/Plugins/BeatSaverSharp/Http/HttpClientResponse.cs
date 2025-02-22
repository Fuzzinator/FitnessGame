﻿#if !RELEASE_UNITY
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace BeatSaverSharp.Http
{
    internal class HttpClientResponse : IHttpResponse
    {
        public int Code => (int)_httpResponseMessage.StatusCode;
        public bool Successful => _httpResponseMessage.IsSuccessStatusCode;

        private byte[]? _bytes;
        private string? _bodyAsString;
        private readonly HttpResponseMessage _httpResponseMessage;
        private static readonly JsonSerializer _jsonSerializer = new JsonSerializer();

        public HttpClientResponse(HttpResponseMessage httpResponseMessage)
        {
            _httpResponseMessage = httpResponseMessage;
        }

        public HttpClientResponse(byte[] bytes, HttpResponseMessage httpResponseMessage)
        {
            _bytes = bytes;
            _httpResponseMessage = httpResponseMessage;
        }

        public async UniTask<byte[]> ReadAsByteArrayAsync()
        {
            if (_bytes is null)
                _bytes = await _httpResponseMessage.Content.ReadAsByteArrayAsync().AsUniTask();//.ConfigureAwait(false);
            return _bytes;
        }

        public async UniTask<string> ReadAsStringAsync()
        {
            if (_bodyAsString is null)
            {
                if (_bytes is null)
                    _bytes = await ReadAsByteArrayAsync();//.ConfigureAwait(false);
                _bodyAsString = Encoding.UTF8.GetString(_bytes);
            }

            /*while (_bodyAsString.Contains("\"curator\": {"))
            {
                var index = _bodyAsString.IndexOf("curator");
                var endIndex = _bodyAsString.IndexOf('}', index);
                _bodyAsString = _bodyAsString.Substring(0, index) +
                                _bodyAsString.Substring(endIndex, _bodyAsString.Length);
            }*/
            return _bodyAsString;
        }

        public async UniTask<T> ReadAsObjectAsync<T>() where T : class
        {
            using StringReader reader = new StringReader(await ReadAsStringAsync());//.ConfigureAwait(false));
            using JsonTextReader jsonTextReader = new JsonTextReader(reader);
            return _jsonSerializer.Deserialize<T>(jsonTextReader)!;
        }
    }
}
#endif