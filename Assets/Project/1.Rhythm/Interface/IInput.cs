using System;
using UnityEngine;

namespace Project.Rhythm.Interface
{
    public interface IInputProvider
    {
        event Action<Vector2> OnPointerDown;
        event Action<Vector2> OnSlideAction;
        event Action OnPointerUp;
        bool IsPressing { get; }
    }
}
