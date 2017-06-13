using System.Windows;
using System.Windows.Interop;

namespace EveLocalChatAnalyser.Utilities
{
    public static class RenderingUtil
    {
        public static void ActivateSoftwareRendering(this Window newsWindow)
        {
            var hwndSource = (HwndSource)PresentationSource.FromVisual(newsWindow);
            if (hwndSource != null)
            {
                var hwndTarget = hwndSource.CompositionTarget;
                if (hwndTarget != null) hwndTarget.RenderMode = RenderMode.SoftwareOnly;
            }
        }
    }
}
