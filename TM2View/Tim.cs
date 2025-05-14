using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace TM2View;

// specific to PlayStation 2 save icons
public class Tim
{
     private byte[] bitmap;
    private byte[] pallette;
    
    public int Width { get; private set; }
    public int Height { get; private set; }
    
    public Tim(byte[] data)
    {
        Width = 128;
        Height = 128;
        bitmap = new byte[Width * Height * 4];
        var isCompressed = false;
        var Length = BitConverter.ToInt32(data, 0);
        if (Length == data.Length - 4)
        {
            isCompressed = true;
        }

        byte[] decompressed = [];
        if (!isCompressed)
        {
            decompressed = data;
        }
        else
        {
            List<byte> RLEDecoded = new();
            var i = 4;
            while (i < Length)
            {
                var code = BitConverter.ToUInt16(data, i);
                if (code < 0xFF00)
                {
                    byte[] replicableData = [data[i+2], data[i+3]];
                    if (i % 2 != 0)
                    {
                        i += 1;
                        continue;
                    }
                    for (var c = 0; c < code; c++)
                    {
                        RLEDecoded.Add(replicableData[0]);
                        RLEDecoded.Add(replicableData[1]);
                    }

                    i += 4;
                }
                else
                {
                    var blockSize = 0xFFFF - BitConverter.ToUInt16(data, i);
                    var coll = data.Skip(i + 2).Take((blockSize+1) * 2);
                    RLEDecoded.AddRange(coll);
                    i += (blockSize*2)+4;
                }
            }
            decompressed = RLEDecoded.ToArray();
        }
        var bp = 0;
        const int alpha = 0xFF;
        for (var i = 0; i < Width*Height*2; i += 2)
        {
            try
            {
                var pixelData = BitConverter.ToInt16(decompressed, i);
                var red = 8 * (pixelData & 0x1F);
                var green = 8 * ((pixelData >> 5) & 0x1F);
                var blue = 8 * (pixelData >> 10);
                bitmap[bp] = (byte)blue;
                bitmap[bp + 1] = (byte)green;
                bitmap[bp + 2] = (byte)red;
                bitmap[bp + 3] = alpha;
                bp += 4;
            }
            catch
            {
                bitmap[bp] = 0;
                bitmap[bp + 1] = 0;
                bitmap[bp + 2] = 0;
                bitmap[bp + 3] = alpha;
                bp += 4;
            }
        }
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
        var pixData = bitmap;
        Marshal.Copy(pixData, 0, frameBuf.Address, pixData.Length);

        return bmp;
    }

    public Bitmap ToBitmap()
    {
        return CreateBitmapFromPixelData();
    }
}