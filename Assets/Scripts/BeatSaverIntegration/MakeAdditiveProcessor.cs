using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.IO;
using System.Threading.Tasks;

public static class MakeAdditiveProcessor
{
    // Job to process notes
    [BurstCompile]
    struct ProcessChoreographyJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ChoreographyNote> prevNotes;
        [ReadOnly] public NativeArray<ChoreographyNote> currentNotes;
        public NativeList<ChoreographyNote>.ParallelWriter toAddNotes;
        public float lastCurrentNoteTime;
        public int finalSize;

        public void Execute(int index)
        {
            var note = prevNotes[index];
            bool noteExists = false;

            // Replace LINQ Any with manual search
            for (int i = 0; i < currentNotes.Length; i++)
            {
                if (currentNotes[i].Time == note.Time)
                {
                    noteExists = true;
                    break;
                }
            }

            if (!noteExists && note.Time <= lastCurrentNoteTime)
            {
                finalSize++;
                // Then, in your job, write data using the writer
                toAddNotes.AddNoResize(note);
            }
        }
    }

    public static async UniTask MakeAdditive(SongInfo songInfo)
    {
        // Allocate pooled NativeArrays to avoid frequent allocation/disposal
        NativeArray<ChoreographyNote> currentNotes = default;
        NativeArray<ChoreographyNote> prevNotes = default;
        NativeList<ChoreographyNote> toAddNotes = default;

        for (var i = 0; i < songInfo.DifficultySets.Length; i++)
        {
            var set = songInfo.DifficultySets[i];

            for (var j = 1; j < set.DifficultyInfos.Length; j++)
            {
                var currentDifInfo = set.DifficultyInfos[j];
                var prevDifInfo = set.DifficultyInfos[j - 1];

                var currentChoreography = Choreography.LoadFromSongInfo(songInfo, currentDifInfo);
                var prevChoreography = Choreography.LoadFromSongInfo(songInfo, prevDifInfo);

                if (currentChoreography != null && prevChoreography != null)
                {
                    // Reuse NativeArrays for each difficulty set
                    if (!currentNotes.IsCreated || currentNotes.Length != currentChoreography.Notes.Length)
                        currentNotes = new NativeArray<ChoreographyNote>(currentChoreography.Notes, Allocator.Persistent);
                    else
                        NativeArray<ChoreographyNote>.Copy(currentChoreography.Notes, currentNotes);

                    if (!prevNotes.IsCreated || prevNotes.Length != prevChoreography.Notes.Length)
                        prevNotes = new NativeArray<ChoreographyNote>(prevChoreography.Notes, Allocator.Persistent);
                    else
                        NativeArray<ChoreographyNote>.Copy(prevChoreography.Notes, prevNotes);

                    if (!toAddNotes.IsCreated)
                        toAddNotes = new NativeList<ChoreographyNote>(currentNotes.Length + prevNotes.Length, Allocator.Persistent);
                    else
                        toAddNotes.Clear();

                    // Schedule the job
                    var processJob = new ProcessChoreographyJob
                    {
                        prevNotes = prevNotes,
                        currentNotes = currentNotes,
                        toAddNotes = toAddNotes.AsParallelWriter(),
                        lastCurrentNoteTime = currentNotes[currentNotes.Length - 1].Time // Cache last element time
                    };

                    var jobHandle = processJob.Schedule(prevNotes.Length, 64);
                    jobHandle.Complete();

                    toAddNotes.Resize(processJob.finalSize, NativeArrayOptions.ClearMemory);

                    // Merge the results and sort (can further optimize by using Jobs for sorting)
                    var allNotes = new NativeList<ChoreographyNote>(Allocator.Persistent);
                    allNotes.AddRange(currentNotes);
                    allNotes.AddRange(toAddNotes);

                    var handle = allNotes.AsArray().Sort();
                    await handle;
                    handle.Complete();

                    // Convert to array and set notes
                    currentChoreography.SetNotes(allNotes.ToArray());

                    // Dispose NativeList for current iteration
                    allNotes.Dispose();
                }

                // Use async batch file writing for performance
                WriteSongToFileAsync(songInfo.fileLocation, currentDifInfo.FileName, currentChoreography).Forget();
            }
        }

        // Dispose pooled NativeArrays when done
        if (currentNotes.IsCreated) currentNotes.Dispose();
        if (prevNotes.IsCreated) prevNotes.Dispose();
        if (toAddNotes.IsCreated) toAddNotes.Dispose();
    }

    // Batch file writing using UniTask
    public static async UniTask WriteSongToFileAsync(string fileLocation, string fileName, Choreography choreography)
    {
        // Simulate file writing, batching can also be applied here
        var path = $"{AssetManager.SongsPath}{fileLocation}/{fileName}";
        using (StreamWriter writer = new StreamWriter(path))
        {
            await writer.WriteAsync(choreography.ToString());
        }
    }
}
