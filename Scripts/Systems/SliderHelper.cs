using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderHelper : MonoBehaviour, IPointerUpHandler
{
    private Slider slider;

    protected Subject<float> changeValueSubject = new Subject<float>();

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public IObservable<float> OnChangeValue
    {
        get { return changeValueSubject; }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        changeValueSubject.OnNext(slider.value);
    }
}