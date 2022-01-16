using System;
using System.Windows;
using System.Windows.Controls;

namespace GraphicsLibrary
{
    public class Image2D : IShape
    {
        public Image image = new Image();
        public override UIElement Draw()
        {
            return image;
        }

        public override void HandleEnd(Point2D point)
        {
        }

        public override void HandleShiftMode()
        {
        }

        public override void HandleStart(Point2D point)
        {
        }
    }
}
