/*
UniBpmAnalyzer
Copyright (c) 2016 WestHillApps (Hironari Nishioka)
This software is released under the MIT License.
http://opensource.org/licenses/mit-license.php
*/

using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

public class UniBpmAnalyzer
{
    #region CONST

    // BPM search range
    private const int MIN_BPM = 60;
    private const int MAX_BPM = 250;
    // Base frequency (44.1kbps)
    private const int BASE_FREQUENCY = 44100;
    // Base channels (2ch)
    private const int BASE_CHANNELS = 2;
    // Base split size of sample data (case of 44.1kbps & 2ch)
    private const int BASE_SPLIT_SAMPLE_SIZE = 2205;

    #endregion

    public struct BpmMatchData
    {
        public int bpm;
        public float match;
    }

    private static BpmMatchData[] bpmMatchDatas = new BpmMatchData[MAX_BPM - MIN_BPM + 1];

    public static async UniTask<int> TryAnalyzeBpmWithJobs(AudioClip clip)
    {
        for (int i = 0; i < bpmMatchDatas.Length; i++)
        {
            bpmMatchDatas[i].match = 0f;
        }

        if (clip == null)
        {
            await UniTask.DelayFrame(1);
            return -1;
        }

        int frequency = clip.frequency;
        int channels = clip.channels;
        int splitFrameSize = Mathf.FloorToInt((frequency / (float)BASE_FREQUENCY) * (channels / (float)BASE_CHANNELS) * BASE_SPLIT_SAMPLE_SIZE);

        var targetLength = clip.samples * channels;
        float[] tempBuffer = new float[targetLength];
        NativeArray<float> allSamples;

        if (clip.GetData(tempBuffer, 0))
        {
            allSamples = new NativeArray<float>(tempBuffer, Allocator.TempJob);
        }
        else
        {
            Debug.Log("Failed to get data from clip.");
            return -1;
        }


        int volumeArrLength = Mathf.CeilToInt((float)allSamples.Length / splitFrameSize);
        var volumeArr = new NativeArray<float>(volumeArrLength, Allocator.TempJob);

        var volumeArrayJob = new VolumeArrayJob
        {
            allSamples = allSamples,
            volumeArr = volumeArr,
            splitFrameSize = splitFrameSize
        };

        var volumeHandle = volumeArrayJob.Schedule(volumeArr.Length, 64);
        await UniTask.WaitUntil(() => volumeHandle.IsCompleted);
        volumeHandle.Complete();

        var diffList = new NativeArray<float>(volumeArr.Length - 1, Allocator.TempJob);
        for (int i = 1; i < volumeArr.Length; i++)
        {
            diffList[i - 1] = Mathf.Max(volumeArr[i] - volumeArr[i - 1], 0f);
        }

        var bpmMatchDataArray = new NativeArray<BpmMatchData>(bpmMatchDatas.Length, Allocator.TempJob);

        // Using BpmAnalysisBatchJob to process BPM analysis in batches
        var bpmAnalysisBatchJob = new BpmAnalysisBatchJob
        {
            diffList = diffList,
            bpmMatchDatas = bpmMatchDataArray,
            splitFrequency = frequency / (float)splitFrameSize
        };

        // Determine a batch size; adjust based on your system's capacity
        int batchSize = 32;
        var bpmHandle = bpmAnalysisBatchJob.ScheduleBatch(bpmMatchDataArray.Length, batchSize);
        await UniTask.WaitUntil(() => bpmHandle.IsCompleted);
        bpmHandle.Complete();

        // Find the BPM with the highest match
        int bpm = -1;
        float maxMatch = 0;

        for (int i = 0; i < bpmMatchDataArray.Length; i++)
        {
            if (bpmMatchDataArray[i].match > maxMatch)
            {
                maxMatch = bpmMatchDataArray[i].match;
                bpm = bpmMatchDataArray[i].bpm;
            }
        }

        // Dispose NativeArrays
        allSamples.Dispose();
        volumeArr.Dispose();
        diffList.Dispose();
        bpmMatchDataArray.Dispose();

        return bpm;
    }

    /// <summary>
    /// Analyze BPM from an audio clip
    /// </summary>
    /// <param name="clip">target audio clip</param>
    /// <returns>bpm</returns>
    public static async UniTask<int> TryAnalyzeBpm(AudioClip clip)
    {
        for (int i = 0; i < bpmMatchDatas.Length; i++)
        {
            bpmMatchDatas[i].match = 0f;
        }

        var bpm = -1;

        if (clip == null)
        {
            await UniTask.DelayFrame(1);
            return bpm;
        }

        int frequency = clip.frequency;
        int channels = clip.channels;

        int splitFrameSize = Mathf.FloorToInt((frequency / (float)BASE_FREQUENCY) * ((float)channels / (float)BASE_CHANNELS) * (float)BASE_SPLIT_SAMPLE_SIZE);

        // Get all sample data from audioclip
        var allSamples = new float[clip.samples * channels];
        clip.GetData(allSamples, 0);

        // Create volume array from all sample data
        var volumeArr = await UniTask.RunOnThreadPool(() => CreateVolumeArray(allSamples, splitFrameSize));

        // Search bpm from volume array
        bpm = await UniTask.RunOnThreadPool(() => SearchBpm(volumeArr, frequency, splitFrameSize));

        return bpm;
    }

    /// <summary>
    /// Create volume array from all sample data
    /// </summary>
    private static float[] CreateVolumeArray(float[] allSamples, int splitFrameSize)
    {
        // Pre-calculate array length
        int volumeArrLength = Mathf.CeilToInt((float)allSamples.Length / splitFrameSize);
        var volumeArr = new float[volumeArrLength];

        // For tracking the max volume during calculation
        float maxVolume = 0f;

        // Use Parallel.For for multithreading the sample analysis
        Parallel.For(0, volumeArrLength, powerIndex =>
        {
            int sampleIndex = powerIndex * splitFrameSize;
            float sum = 0f;

            for (int frameIndex = sampleIndex; frameIndex < sampleIndex + splitFrameSize && frameIndex < allSamples.Length; frameIndex++)
            {
                float absValue = Mathf.Abs(allSamples[frameIndex]);
                if (absValue > 1f) continue;
                sum += (absValue * absValue);
            }

            float volumeValue = Mathf.Sqrt(sum / splitFrameSize);
            volumeArr[powerIndex] = volumeValue;

            // Track max volume
            lock (volumeArr)
            {
                if (volumeValue > maxVolume)
                {
                    maxVolume = volumeValue;
                }
            }
        });

        // Normalize volumes
        if (maxVolume > 0)
        {
            Parallel.For(0, volumeArrLength, i =>
            {
                volumeArr[i] /= maxVolume;
            });
        }

        return volumeArr;
    }

    /// <summary>
    /// Search bpm from volume array
    /// </summary>
    private static int SearchBpm(float[] volumeArr, int frequency, int splitFrameSize)
    {
        // Pre-allocate diffList size
        var diffList = ListPool<float>.Get();
        diffList.Capacity = volumeArr.Length - 1;

        // Calculate the differences in volume and add to diffList
        for (int i = 1; i < volumeArr.Length; i++)
        {
            diffList.Add(Mathf.Max(volumeArr[i] - volumeArr[i - 1], 0f));
        }

        float splitFrequency = frequency / (float)splitFrameSize;

        // Parallelize the BPM matching process
        Parallel.For(0, MAX_BPM - MIN_BPM + 1, index =>
        {
            int bpm = MIN_BPM + index;
            float bps = bpm / 60f;
            float sinMatch = 0f, cosMatch = 0f;

            // Pre-calculate the angular frequency factor
            float angularFactor = 2f * Mathf.PI * bps / splitFrequency;

            // Use a more efficient loop for sin and cos match calculations
            for (int i = 0; i < diffList.Count; i++)
            {
                float angle = i * angularFactor;
                sinMatch += (diffList[i] * Mathf.Cos(angle));
                cosMatch += (diffList[i] * Mathf.Sin(angle));
            }

            sinMatch /= diffList.Count;
            cosMatch /= diffList.Count;

            float match = Mathf.Sqrt((sinMatch * sinMatch) + (cosMatch * cosMatch));

            bpmMatchDatas[index].bpm = bpm;
            bpmMatchDatas[index].match = match;
        });

        // Find the BPM with the highest match
        int matchIndex = Array.FindIndex(bpmMatchDatas, x => x.match == bpmMatchDatas.Max(y => y.match));

        // Release memory from diffList
        ListPool<float>.Release(diffList);

        return bpmMatchDatas[matchIndex].bpm;
    }



    [BurstCompile]
    public struct VolumeArrayJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> allSamples;
        public NativeArray<float> volumeArr;
        public int splitFrameSize;

        public void Execute(int powerIndex)
        {
            int sampleIndex = powerIndex * splitFrameSize;
            float sum = 0f;

            for (int frameIndex = sampleIndex; frameIndex < sampleIndex + splitFrameSize && frameIndex < allSamples.Length; frameIndex++)
            {
                float absValue = math.abs(allSamples[frameIndex]);
                if (absValue > 1f) continue;
                sum += (absValue * absValue);
            }

            var volumeValue = math.sqrt(sum / splitFrameSize);
            volumeArr[powerIndex] = volumeValue;
        }
    }

    [BurstCompile]
    public struct MaxVolumeGetterJob : IJobParallelFor
    {
        public NativeArray<float> volumeArr;
        public float maxVolume;

        public void Execute(int index)
        {
            volumeArr[index] /= maxVolume;
        }
    }

    [BurstCompile]
    public struct BpmAnalysisBatchJob : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<float> diffList;
        public NativeArray<BpmMatchData> bpmMatchDatas;
        public float splitFrequency;

        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; i++)
            {
                int bpm = MIN_BPM + i;
                float bps = bpm / 60f;
                float sinMatch = 0f, cosMatch = 0f;

                float angularFactor = 2f * Mathf.PI * bps / splitFrequency;

                for (int j = 0; j < diffList.Length; j++)
                {
                    float angle = j * angularFactor;
                    sinMatch += (diffList[j] * math.cos(angle));
                    cosMatch += (diffList[j] * math.sin(angle));
                }

                sinMatch /= (float)diffList.Length;
                cosMatch /= (float)diffList.Length;

                float match = math.sqrt((sinMatch * sinMatch) + (cosMatch * cosMatch));

                bpmMatchDatas[i] = new BpmMatchData { bpm = bpm, match = match };
            }
        }
    }
}
