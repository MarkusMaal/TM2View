using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PixelFormat = Avalonia.Remote.Protocol.Viewport.PixelFormat;

namespace TM2View;

public class Tim2
{
    private byte[] bitmap;
    private byte[] pallette;
    
    public int Width { get; private set; }
    public int Height { get; private set; }
    
    public Tim2(byte[] data)
    {
        var headerSize = BitConverter.ToInt16(data, 0x1C);
        var bitmapSize = BitConverter.ToInt32(data, 0x18);
        var palleteSize = BitConverter.ToInt32(data, 0x14);
        this.Width = BitConverter.ToInt16(data, 0x24);
        this.Height = BitConverter.ToInt16(data, 0x26);
        this.bitmap = data.Skip(0x10+headerSize).Take(bitmapSize).ToArray();
        this.pallette = data.Skip(0x10+headerSize + bitmapSize).Take(palleteSize).ToArray();
    }

    private byte[] GenerateRGBAArray()
    {
        List<Color> paletteArray = [];
        for (int i = 0; i < this.pallette.Length; i += 4)
        {
            paletteArray.Add(Color.FromArgb(this.pallette[i+3], this.pallette[i], this.pallette[i+1], this.pallette[i+2 ]));
        }

        List<byte> bitmapArray = [];
        foreach (var b in bitmap)
        {
            try
            {
                bitmapArray.Add(paletteArray[b].B);
                bitmapArray.Add(paletteArray[b].G);
                bitmapArray.Add(paletteArray[b].R);
                bitmapArray.Add(paletteArray[b].A);
            }
            catch
            {
                bitmapArray.Add(paletteArray[0].B);
                bitmapArray.Add(paletteArray[0].G);
                bitmapArray.Add(paletteArray[0].R);
                bitmapArray.Add(paletteArray[0].A);
            }
        }

        return bitmapArray.ToArray();
    }

    private WriteableBitmap CreateBitmapFromPixelData()
    {
        // Standard may need to change on some devices 
        var dpi = new Vector(96, 96);

        var bmp = new WriteableBitmap(
            new PixelSize(this.Width, this.Height),
            dpi,
            Avalonia.Platform.PixelFormat.Bgra8888,
            AlphaFormat.Premul);

        using var frameBuf = bmp.Lock();
        var pixData = GenerateRGBAArray();
        Marshal.Copy(pixData, 0, frameBuf.Address, pixData.Length);

        return bmp;
    }

    public Bitmap ToBitmap()
    {
        return CreateBitmapFromPixelData();
    }
}