using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace BeatSaverSharp.Models.Pages
{
    internal class SearchPage : Page
    {
        private readonly int _pageNumber;
        private readonly SearchTextFilterOption? _searchTextFilterOptions;

        public SearchPage(int pageNumber, SearchTextFilterOption? searchTextFilterOption, IReadOnlyList<Beatmap> beatmaps) : base(beatmaps)
        {
            _pageNumber = pageNumber;
            _searchTextFilterOptions = searchTextFilterOption;
        }

        public override UniTask<Page?> Next(CancellationToken token = default, IProgress<double> progress = null)
        {
            return Client.SearchBeatmaps(_searchTextFilterOptions, _pageNumber + 1, token, progress);
        }

        public override UniTask<Page?> Previous(CancellationToken token = default, IProgress<double> progress = null)
        {
            if (_pageNumber == 0)
                return UniTask.FromResult<Page?>(null);
            return Client.SearchBeatmaps(_searchTextFilterOptions, _pageNumber - 1, token);
        }
    }
}