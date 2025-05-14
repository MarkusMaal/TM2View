using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;

namespace TM2View;

public partial class MainWindow : Window
{

    private bool mirrored = false;
    private bool flipped = false;
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private async void OpenMenuItem_OnClick(object? sender, RoutedEventArgs e)
    { // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Tim or Tim2 file",
            AllowMultiple = false
        });

        if (files.Count < 1) return;
        // Open reading stream from the first file.
        var file = files[0];
        if (file.Path.AbsolutePath.ToLower().EndsWith(".tim"))
        {
            var pic = new Tim(await File.ReadAllBytesAsync(Uri.UnescapeDataString(file.Path.AbsolutePath)));   
            PreviewImage.Source = pic.ToBitmap();
            PreviewImage.Width = pic.Width;
            PreviewImage.Height = pic.Height;
        }
        else
        {
            var pic = new Tim2(await File.ReadAllBytesAsync(Uri.UnescapeDataString(file.Path.AbsolutePath)));
            PreviewImage.Source = pic.ToBitmap();
            PreviewImage.Width = pic.Width;
            PreviewImage.Height = pic.Height;
        }
    }

    private void InvBg_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Equals(BackPanel.Background, Brushes.Black))
        {
            BackPanel.Background = Brushes.White;
        }
        else
        {
            BackPanel.Background = Brushes.Black;
        }
    }

    private void Mirror_OnClick(object? sender, RoutedEventArgs e)
    {
        mirrored = !mirrored;
        ApplyTransforms();
    }

    private void Flip_OnClick(object? sender, RoutedEventArgs e)
    {
        flipped = !flipped;
        ApplyTransforms();
    }

    private void ApplyTransforms()
    {
        var transGrp = new TransformGroup();
        transGrp.Children.Add(new ScaleTransform(mirrored ? -1.0 : 1.0, 1.0));
        transGrp.Children.Add(new RotateTransform(flipped ? 180 : 0));
        PreviewImage.RenderTransform = transGrp;
    }
}