using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ClaudiaIDE.Helpers
{
    internal static class ImageSourceAnimator
    {
        public static void AnimateImageSourceChange(this Brush background, ImageBrush newImage,
            Action<ImageBrush> onChangeImage, AnimateImageChangeParams animateImageChangeParams = null)
        {
            var animationParameters = animateImageChangeParams ?? new AnimateImageChangeParams();

            if (background != null)
            {
                var fadeOutAnimation = new DoubleAnimation(0d, animationParameters.FadeTime) { AutoReverse = false };
                var fadeInAnimation =
                    new DoubleAnimation(0d, animationParameters.TargetOpacity, animationParameters.FadeTime)
                        { AutoReverse = false };

                fadeOutAnimation.Completed += (o, e) =>
                {
                    newImage.Opacity = 0d;
                    onChangeImage(newImage);
                    background.Opacity = animationParameters.TargetOpacity;
                    newImage.BeginAnimation(Brush.OpacityProperty, fadeInAnimation);
                };

                background.BeginAnimation(Brush.OpacityProperty, fadeOutAnimation);
            }
            else
            {
                newImage.Opacity = animateImageChangeParams.TargetOpacity;
                onChangeImage(newImage);
            }
        }

        public static void AnimateImageSourceChange(this Brush background, VisualBrush newVisual,
            Action<VisualBrush> onChangeVisual, AnimateImageChangeParams animateImageChangeParams = null)
        {
            var animationParameters = animateImageChangeParams ?? new AnimateImageChangeParams();

            if (background != null)
            {
                var fadeOutAnimation = new DoubleAnimation(0d, animationParameters.FadeTime) { AutoReverse = false };
                var fadeInAnimation =
                    new DoubleAnimation(0d, animationParameters.TargetOpacity, animationParameters.FadeTime)
                        { AutoReverse = false };

                fadeOutAnimation.Completed += (o, e) =>
                {
                    newVisual.Opacity = 0d;
                    onChangeVisual(newVisual);
                    background.Opacity = animationParameters.TargetOpacity;
                    newVisual.BeginAnimation(Brush.OpacityProperty, fadeInAnimation);
                };

                background.BeginAnimation(Brush.OpacityProperty, fadeOutAnimation);
            }
            else
            {
                newVisual.Opacity = animateImageChangeParams.TargetOpacity;
                onChangeVisual(newVisual);
            }
        }

        public static void AnimateImageSourceChange(this Image image,
            ImageSource newImage,
            Action<Image> onChangeImage,
            AnimateImageChangeParams animateImageChangeParams = null)
        {
            var animationParameters = animateImageChangeParams ?? new AnimateImageChangeParams();

            if (image != null)
            {
                var fadeOutAnimation = new DoubleAnimation(0d, animationParameters.FadeTime) { AutoReverse = false };
                var fadeInAnimation =
                    new DoubleAnimation(0d, animationParameters.TargetOpacity, animationParameters.FadeTime)
                        { AutoReverse = false };

                fadeOutAnimation.Completed += (o, e) =>
                {
                    onChangeImage(image);
                    image.Source = newImage;
                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Fant);
                    image.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
                };
                image.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
            }
            else
            {
                image.Opacity = animateImageChangeParams.TargetOpacity;
                image.Source = newImage;
                onChangeImage(image);
            }
        }
    }
}