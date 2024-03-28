using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TimeMoonUI : MonoBehaviour
{
    public Image sunSymbol;
    public Transform centerPoint;
    public float semiejeX = 100.0f;
    public float semiejeY = 50.0f;

    private void Start()
    {
        float timeProgress = Mathf.Clamp01(GameState.timeLoopDuration / GameState.initialTimeLoopDuration);
        float angle = timeProgress * Mathf.PI;

        float semiejeXResized = semiejeX * ((float)Screen.width / 3840);
        float semiejeYResized = semiejeY * ((float)Screen.height / 2160);

        float x = centerPoint.position.x + semiejeXResized * Mathf.Cos(angle);
        float y = centerPoint.position.y + semiejeYResized * Mathf.Sin(angle);

        sunSymbol.rectTransform.position = new Vector3(x, y, 0);
    }

    private void Update()
    {
        float timeProgress = Mathf.Clamp01(GameState.timeLoopDuration / GameState.initialTimeLoopDuration);
        float angle = timeProgress * Mathf.PI;

        float semiejeXResized = semiejeX * ((float)Screen.width / 3840);
        float semiejeYResized = semiejeY * ((float)Screen.height / 2160);

        float x = centerPoint.position.x + semiejeXResized * Mathf.Cos(angle);
        float y = centerPoint.position.y + semiejeYResized * Mathf.Sin(angle);

        sunSymbol.rectTransform.DOMove(new Vector3(x, y, 0), 0.2f);
    }
}
