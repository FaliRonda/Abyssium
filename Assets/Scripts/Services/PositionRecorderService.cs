using System.Collections.Generic;
using DG.Tweening;
using Ju.Services;
using UnityEditor.Searcher;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class PositionRecorderService : IService
{
    private List<Vector3> positionsList = new List<Vector3>();
    private Sequence positionSequence;
    private RewindConfigSO rewindConfig;

    public void StartRecording(Transform transformToRecord)
    {
        rewindConfig = Resources.Load<RewindConfigSO>("Conf/RewindConfig");
        
        positionSequence = DOTween.Sequence()
            .AppendCallback(() => { positionsList.Add(transformToRecord.position); })
            .AppendInterval(0.1f)
            .SetLoops(-1)
            .OnKill(() => { });
    }
    
    public void StopRecording()
    {
        positionSequence.Kill();
    }

    public void DoRewind(Transform transformToRewind)
    {
        Sequence rewindSequence = DOTween.Sequence();

        int numberOfRecordedPositions = positionsList.Count;
        float initialDuration = 0.3f;
        float minDuration = 0.025f;
        float duration = initialDuration;
        
        for (int i = numberOfRecordedPositions - 1; i >= 0; i--)
        {
            Vector3 position = positionsList[i];
            rewindSequence.Append(transformToRewind.DOMove(position, duration));

            float rewindFactor = 1;
            
            if (i >= numberOfRecordedPositions - (numberOfRecordedPositions * rewindConfig.initialPorcentage))
            {
                rewindFactor = duration >= minDuration ? rewindConfig.accelerationFactor : 1;
            } else if (i <= numberOfRecordedPositions * rewindConfig.finalPorcentage)
            {
                rewindFactor = duration <= initialDuration ? rewindConfig.decelerationFactor : 1;
            }
            
            duration *= rewindFactor;
        }

        rewindSequence.OnComplete(() => {});
        
        rewindSequence.Play();
    }
}