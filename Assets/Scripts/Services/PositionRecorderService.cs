﻿using System.Collections.Generic;
using DG.Tweening;
using Ju.Services;
using UnityEditor.Searcher;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class PositionRecorderService : IService
{
    private List<Vector3> pjPositionsList = new List<Vector3>();
    private List<Quaternion> moonRotationsList = new List<Quaternion>();
    
    private Sequence pjPositionSequence;
    private Sequence moonPositionSequence;
    
    private RewindConfigSO rewindConfig;

    public void StartRecording(Transform pjTransform, Transform moonTransform)
    {
        rewindConfig = Resources.Load<RewindConfigSO>("Conf/RewindConfig");
        
        pjPositionSequence = DOTween.Sequence()
            .AppendCallback(() => { pjPositionsList.Add(pjTransform.position); })
            .AppendInterval(0.1f)
            .SetLoops(-1)
            .OnKill(() => { });
        
        moonPositionSequence = DOTween.Sequence()
            .AppendCallback(() => { moonRotationsList.Add(moonTransform.rotation); })
            .AppendInterval(0.1f)
            .SetLoops(-1)
            .OnKill(() => { });
    }
    
    public void StopRecording()
    {
        pjPositionSequence.Kill();
        moonPositionSequence.Kill();
    }

    public void DoRewind(Transform pjTransform, Transform moonTransform)
    {
        Sequence rewindSequence = DOTween.Sequence();

        int numberOfRecordedPositions = pjPositionsList.Count;
        float initialDuration = 0.3f;
        float minDuration = 0.025f;
        float duration = initialDuration;
        
        for (int i = numberOfRecordedPositions - 1; i >= 0; i--)
        {
            Vector3 pjPosition = pjPositionsList[i];
            rewindSequence.Append(pjTransform.DOMove(pjPosition, duration));
            Quaternion moonRotation = moonRotationsList[i];
            rewindSequence.Append(moonTransform.DORotateQuaternion(moonRotation, duration));

           

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