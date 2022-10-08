namespace LudumDareTools;

using SkiaSharp;

using System.Net.Http;
using System.Text.Json;

public static class Utils
{
    static JsonSerializerOptions jsonOptions;

    public static JsonSerializerOptions GetJsonOptions()
    {
        if (jsonOptions is null)
        {
            jsonOptions = new()
            {
                IncludeFields = true,
                WriteIndented = true
            };
        }
        return jsonOptions;
    }

    static JsonSettings jsonSettings;

    public static JsonSettings GetJsonSettings()
    {
        if (jsonSettings == null)
        {
            jsonSettings = new JsonSettings
            {
                Formatting = JsonFormatting.Indented
            };
        }
        return jsonSettings;
    }

    public static HttpClient CreateHttpClient(string token = null)
    {
        var httpClient = new HttpClient { BaseAddress = new(Constants.API_BASE_URL) };

        if (token is not null)
            httpClient.DefaultRequestHeaders.Add("Cookie", Constants.TOKEN_PREFIX + token);

        return httpClient;
    }

    public static SKBitmap CreateBitmap(Stream stream, BitmapOptions options)
    {
        using var original = SKBitmap.Decode(stream);

        SKBitmap bitmap;

        var width = (float) original.Width;
        var height = (float) original.Height;
        var aspectRatio = width / height;
        int[] cropOffset = null;

        if (options.crop && options.maxWidth.HasValue && options.maxHeight.HasValue)
        {
            if (width > options.maxWidth.Value)
            {
                var factor = options.maxWidth.Value / width;
                width *= factor;
                height *= factor;
            }

            if (height < options.maxHeight.Value)
            {
                var factor = options.maxHeight.Value / height;
                width *= factor;
                height *= factor;
                cropOffset = new[] { (int) Math.Round(width - options.maxWidth.Value) / 2, 0 };
            }
            else
            {
                cropOffset = new[] { 0, (int) Math.Round(height - options.maxHeight.Value) / 2 };
            }
        }
        else
        {
            if (options.maxWidth.HasValue && width > options.maxWidth.Value)
            {
                var factor = options.maxWidth.Value / width;
                width *= factor;
                height *= factor;
            }

            if (options.maxHeight.HasValue && height > options.maxHeight.Value)
            {
                var factor = options.maxHeight.Value / height;
                width *= factor;
                height *= factor;
            }
        }

        var finalWidth = (int) Math.Round(width);
        var finalHeight = (int) Math.Round(height);

        if (finalWidth != original.Width || finalHeight != original.Height)
        {
            bitmap = original.Resize(new SKImageInfo(finalWidth, finalHeight), SKFilterQuality.High);

            if (cropOffset is not null)
            {
                var tempBitmap = new SKBitmap(options.maxWidth.Value, options.maxHeight.Value);
                using var tempCanvas = new SKCanvas(tempBitmap);
                tempCanvas.DrawBitmap(bitmap, -cropOffset[0], -cropOffset[1]);
                bitmap.Dispose();
                bitmap = tempBitmap;
            }
        }
        else
        {
            bitmap = original;
        }

        var processedBitmap = new SKBitmap(bitmap.Width, bitmap.Height);

        using var canvas = new SKCanvas(processedBitmap);
        using var paint = new SKPaint { BlendMode = SKBlendMode.Src };

        for (int i = 0; i < bitmap.Width; i++)
        {
            for (int j = 0; j < bitmap.Height; j++)
            {
                SKColor color = bitmap.GetPixel(i, j);
                var rgba = new[] {
                    ByteToFloat(color.Red),
                    ByteToFloat(color.Green),
                    ByteToFloat(color.Blue),
                    ByteToFloat(color.Alpha)
                };
                ProcessPixel(ref rgba, options);
                paint.Color = new SKColor(
                    FloatToByte(rgba[0]),
                    FloatToByte(rgba[1]),
                    FloatToByte(rgba[2]),
                    FloatToByte(rgba[3])
                );
                canvas.DrawPoint(i, j, paint);
            }
        }

        bitmap.Dispose();

        return processedBitmap;
    }

    static float ByteToFloat(byte value)
    {
        return value / 255f;
    }

    static byte FloatToByte(float value)
    {
        return (byte) Math.Clamp(Math.Round(value * 255), byte.MinValue, byte.MaxValue);
    }

    static void ProcessPixel(ref float[] rgba, BitmapOptions options)
    {
        float[] rgb = new[] { rgba[0], rgba[1], rgba[2] };

        for (int i = 0; i < 3; i++)
        {
            rgb[i] *= options.contrast + 1;
            rgb[i] += options.brightness - options.contrast;
        }

        float dot = Dot(rgb, new[] { 0.299f, 0.587f, 0.114f });
        rgb = Lerp(new[] { dot, dot, dot }, rgb, Math.Max(0, options.saturation + 1));

        rgba[0] = rgb[0];
        rgba[1] = rgb[1];
        rgba[2] = rgb[2];
    }

    static float Dot(float[] a, float[] b)
    {
        float result = 0;
        for (int i = 0; i < a.Length; i++)
            result += a[i] * b[i];
        return result;
    }

    static float[] Lerp(float[] a, float[] b, float t)
    {
        for (int i = 0; i < a.Length; i++)
            a[i] = a[i] + (b[i] - a[i]) * t;
        return a;
    }
}