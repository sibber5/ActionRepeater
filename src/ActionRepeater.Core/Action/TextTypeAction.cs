using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Threading;
using ActionRepeater.Core.Input;
using ActionRepeater.Win32;
using ActionRepeater.Win32.Input;
using ActionRepeater.Win32.Synch.Utilities;

namespace ActionRepeater.Core.Action;

public sealed class TextTypeAction : WaitableInputAction
{
    [JsonIgnore]
    public override string Name => "Type Text";
    [JsonIgnore]
    public override string Description
    {
        get
        {
            var text = Text.Length <= 100 ? Text : string.Concat(Text.AsSpan(0, 97), "...");
            return WPM > 0
                ? $"{text} ({WPM} WPM)"
                : text;
        }
    }

    private string _text;
    public string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;

            _text = value;
            OnDescriptionChanged();
        }
    }

    private int _wpm;
    public int WPM
    {
        get => _wpm;
        set
        {
            if (_wpm == value) return;

            Debug.Assert(value >= 0);
            _wpm = value;
            OnDescriptionChanged();
        }
    }

    public TextTypeAction(string text, int wpm = 0)
    {
        _text = text;
        _wpm = wpm;
    }

    public override void PlayWait(HighResolutionWaiter waiter, CancellationToken cancellationToken)
    {
        int charsPerSecond = WPM * 5 / 60;
        int delayMS = charsPerSecond == 0 ? 0 : 1000 / charsPerSecond;

        using var registration = cancellationToken.Register(static (waiter) => ((HighResolutionWaiter)waiter!).Cancel(), waiter);

        foreach (char c in Text.AsSpan())
        {
            if (cancellationToken.IsCancellationRequested) return;

            var (shift, ctrl, alt, key) = GetVirtualKeys(c);

            if (shift) InputSimulator.SendKeyDown(VirtualKey.SHIFT);
            if (ctrl) InputSimulator.SendKeyDown(VirtualKey.CONTROL);
            if (alt) InputSimulator.SendKeyDown(VirtualKey.MENU);

            InputSimulator.SendKeyPress(key);

            if (shift) InputSimulator.SendKeyUp(VirtualKey.SHIFT);
            if (ctrl) InputSimulator.SendKeyUp(VirtualKey.CONTROL);
            if (alt) InputSimulator.SendKeyUp(VirtualKey.MENU);

            waiter.Wait(delayMS);
        }
    }

    private static (bool shift, bool ctrl, bool alt, VirtualKey key) GetVirtualKeys(char c)
    {
        short vk = PInvoke.VkKeyScan(c);

        sbyte low = (sbyte)(vk & 0xFF);
        sbyte high = (sbyte)((vk >> 8) & 0xFF);

        if (low == -1 && high == -1) throw new InvalidOperationException("Character could not be translated.");

        VirtualKey key = (VirtualKey)(((ushort)low) & 0xFF);

        bool shift = (high & 1) != 0;
        bool ctrl = (high & 2) != 0;
        bool alt = (high & 4) != 0;
        bool hankaku = (high & 8) != 0;

        if (hankaku) throw new NotSupportedException("Hankaku is not supported.");

        return (shift, ctrl, alt, key);
    }
}
