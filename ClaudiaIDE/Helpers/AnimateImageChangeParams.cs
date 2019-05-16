using System;

namespace ClaudiaIDE.Helpers
{
    class AnimateImageChangeParams
    {
        public double TargetOpacity { get; set; } = 0.35;
        public TimeSpan FadeTime { get; set; } = TimeSpan.FromSeconds(5);
    }
}