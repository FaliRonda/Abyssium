using System;
using System.Collections.Generic;
using DG.Tweening;
using Ju.Services;
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

    public void DoRewind(Transform pjTransform, Transform moonTransform, Action callback)
    {
        Sequence rewindSequence = DOTween.Sequence();

        int numberOfRecordedPositions = pjPositionsList.Count;
        float duration = rewindConfig.initialDuration;
        float accumulatedDuration = 0;
        
        for (int i = numberOfRecordedPositions - 1; i >= 0; i--)
        {
            Vector3 pjPosition = pjPositionsList[i];
            rewindSequence.Append(pjTransform.DOMove(pjPosition, duration));
            
            Quaternion moonRotation = moonRotationsList[i];
            rewindSequence.Append(moonTransform.DORotateQuaternion(moonRotation, duration));

            accumulatedDuration += duration;
            if (accumulatedDuration >= rewindConfig.minimumSoundPeriod)
            {
                accumulatedDuration = 0;
                float pitch = 1.5f + (1f - duration * 4);
                rewindSequence.AppendCallback(() => Core.Audio.Play(SOUND_TYPE.ClockTikTak, 0, 0.05f, pitch));
            }

            float rewindFactor = 1;
            
            if (i >= numberOfRecordedPositions - (numberOfRecordedPositions * rewindConfig.initialPorcentage))
            {
                rewindFactor = duration >= rewindConfig.minimumRewindStepDuration ? rewindConfig.accelerationFactor : 1;
            } else if (i <= numberOfRecordedPositions * rewindConfig.finalPorcentage)
            {
                rewindFactor = duration <= rewindConfig.initialDuration ? rewindConfig.decelerationFactor : 1;
            }

            duration *= rewindFactor;
        }

        rewindSequence.OnComplete(callback.Invoke);
        
        rewindSequence.Play();
    }
}