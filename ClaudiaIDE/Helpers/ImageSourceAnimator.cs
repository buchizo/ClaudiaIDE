using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ClaudiaIDE.Helpers
{
    static class ImageSourceAnimator
    {
        public static void AnimateImageSourceChange(this Image image, ImageSource bitmap, Action<Image> onShowImage)
        {
            var fadeTime = TimeSpan.FromSeconds(3);
            var fadeInAnimation = new DoubleAnimation(1d, fadeTime);

            if (image.Source != null)
            {
                var fadeOutAnimation = new DoubleAnimation(0d, fadeTime);

                fadeOutAnimation.Completed += (o, e) =>
                {
                    image.Source = bitmap;
                    onShowImage(image);
                    image.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
                };

                image.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
            }
            else
            {
                image.Opacity = 0d;
                image.Source = bitmap;
                onShowImage(image);
                image.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
            }
        }
    }
}
