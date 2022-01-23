using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace BeatSaverSharp.Models.Pages
{
    internal sealed class UploaderPage : Page
    {
        private readonly int _uploader;
        private readonly int _pageNumber;

        public UploaderPage(int pageNumber, int uploader, IReadOnlyList<Beatmap> beatmaps) : base(beatmaps)
        {
            _uploader = uploader;
            _pageNumber = pageNumber;
        }

        public override UniTask<Page?> Next(CancellationToken token = default)
        {
            return Client.UploaderBeatmaps(_uploader, _pageNumber + 1, token);
        }

        public override UniTask<Page?> Previous(CancellationToken token = default)
        {
            if (_pageNumber == 0)
                return UniTask.FromResult<Page?>(null);
            return Client.UploaderBeatmaps(_uploader, _pageNumber - 1, token);
        }
    }
}