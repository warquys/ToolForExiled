using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using MEC;
using Unity.Collections.LowLevel.Unsafe;

namespace ToolForExiled;

public class CoolDown : IEnumerator<float>
{
    internal float _coolDownTime;

    public float Duration { get; }
    public bool IsActive => CurrentCooldown > 0;
    public float CurrentCooldown => _coolDownTime - Timing.LocalTime;

    float IEnumerator<float>.Current => _coolDownTime;
    object IEnumerator.Current => _coolDownTime;

    public CoolDown(float duration)
    {
        Duration = duration;
    }

    /// <param name="message"><see cref="String.Empty"/> when <see langword="false"/></param>
    public bool NotAllow(out string message)
    {
        if (!IsActive)
        {
            message = string.Empty;
            return false;
        }

        message = ToolForExiledPlugin.Instance.Translation.CoolDown;
        ToolForExiledPlugin.Instance.IgnoreCaseReplaces(ref message, ToolForExiledTranslation.time_Pattern, CurrentCooldown.ToString());
        return true;
    }

    /// <summary>
    /// Use as <see langword="yield"/> <see langword="return"/> value for coroutine.
    /// </summary>
    public float WaitEnd() => _coolDownTime;

    public void Start()
    {
        _coolDownTime = Timing.WaitForSeconds(_coolDownTime);
    }

    public void Reset()
    {
        _coolDownTime = 0;
    }

    void IDisposable.Dispose() { }

    bool IEnumerator.MoveNext() => IsActive;
}
