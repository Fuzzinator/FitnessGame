using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace BeatSaverSharp.Http
{
    public interface IHttpResponse
    {
        /// <summary>
        /// The HTTP status code of the response.
        /// </summary>
        int Code { get; }

        /// <summary>
        /// Whether or not the request was successful or not.
        /// </summary>
        bool Successful { get; }

        /// <summary>
        /// Read the body as a string.
        /// </summary>
        /// <returns>The body represented as a string.</returns>
        UniTask<string> ReadAsStringAsync();

        /// <summary>
        /// Read the body as a byte array.
        /// </summary>
        /// <returns>The body represented as an array of bytes.</returns>
        UniTask<byte[]> ReadAsByteArrayAsync();

        /// <summary>
        /// Reads the body and deserializes it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        UniTask<T> ReadAsObjectAsync<T>() where T : class;
    }
}