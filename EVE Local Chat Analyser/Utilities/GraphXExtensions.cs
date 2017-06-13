using System.Windows;
using GraphX.Controls;

namespace EveLocalChatAnalyser.Utilities
{
    public static class GraphXExtensions
    {
        public static void CenterOnVertexControl(this ZoomControl zoomControl, VertexControl vertex)
        {
            var vPos = vertex.GetPosition();
            var width = zoomControl.ActualWidth/zoomControl.Zoom;
            var height = zoomControl.ActualHeight/zoomControl.Zoom;
            var x = vPos.X - width/2 +  vertex.RenderSize.Width / 2;;
            var y = vPos.Y - height/2 + vertex.RenderSize.Height / 2;

            zoomControl.ZoomToContent(new Rect(x,y,width, height));
        }
    }
}
