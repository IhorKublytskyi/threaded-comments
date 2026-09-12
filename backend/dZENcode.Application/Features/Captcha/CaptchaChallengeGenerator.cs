using System.Security.Cryptography;
using dZENcode.Application.Abstractions;
using dZENcode.Application.Features.Captcha.DTOs;
using SkiaSharp;

namespace dZENcode.Application.Features.Captcha;

public class CaptchaChallengeGenerator : ICaptchaChallengeGenerator
{
    private const string Characters = "abcdefghjkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private const int Length = 5;

    private const int Width = 150, Height = 70; 

    private const int BackgroundLinesCount = 50;

    // For Linux
    static readonly SKTypeface Typeface = SKTypeface.Default;

    public CaptchaChallenge Generate()
    {
        string body = GenerateBody();
        byte[] image = GenerateImage(body);

        return new CaptchaChallenge(body, image);
    }

    private string GenerateBody()
    {
        char[] body = new char[Length];

        for (int i = 0; i < Length; i++)
        {
            body[i] = Characters[RandomNumberGenerator.GetInt32(Characters.Length)];
        }

        return new string(body);
    }

    private byte[] GenerateImage(string body)
    {
        // Init canvas
        using SKBitmap bitmap = new(Width, Height);
        using SKCanvas canvas = new SKCanvas(bitmap);

        DrawLines(canvas);
        DrawLetters(canvas, body);
        DrawForegroundNoise(canvas);

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        
        return data.ToArray();
    }

    private void DrawLines(SKCanvas canvas)
    {
        using SKPaint linesPaint = new()
        {
            IsAntialias = true,
            StrokeWidth = 1.2f,
            Style = SKPaintStyle.Stroke,    
        };

        // Draw background noise
        for (int i = 0; i < BackgroundLinesCount; i++)
        {
            linesPaint.Color = new(
                (byte)Random.Shared.Next(150, 240),
                (byte)Random.Shared.Next(150, 240),
                (byte)Random.Shared.Next(150, 240)
            );

            canvas.DrawLine(
                Random.Shared.Next(Width), Random.Shared.Next(Height),
                Random.Shared.Next(Width), Random.Shared.Next(Height),
                linesPaint);
        }
    }

    private void DrawLetters(SKCanvas canvas, string body)
    {
        // Draw letters
        using SKPaint bodyPaint = new()
        {
            IsAntialias = true,
            StrokeWidth = 2.0f,
           
        };

        float gap = (float)Width / (Length + 1);

        for (int i = 0; i < body.Length; i++)
        {
            bodyPaint.Color = new(
                (byte)Random.Shared.Next(20, 100),
                (byte)Random.Shared.Next(20, 100),
                (byte)Random.Shared.Next(20, 100));

            // step + letter_x_offset + random_offset
            float x = (i + 1) * gap - 10 + Random.Shared.Next(-3, 3);
            // first part of the screen + letter_y_offset + random_offset
            float y = (Height / 2) + 10 + Random.Shared.Next(-3, 3);
            float angle = Random.Shared.Next(-25, 25);

            // Randomizing letter's style and typography
            bodyPaint.Style = Random.Shared.Next(2) == 0 ? SKPaintStyle.Fill : SKPaintStyle.Stroke;
            using var font = new SKFont(Typeface, Random.Shared.Next(22, 26));

            canvas.Save();
            canvas.RotateDegrees(angle, x, y);
            canvas.DrawText(body[i].ToString(), x, y, font, bodyPaint);
            canvas.Restore();
        }
    }

    private void DrawForegroundNoise(SKCanvas canvas)
    {
        // Draw foreground noise
        using SKPaint wavePaint = new()
        {
            Color = new SKColor(48, 48, 48, 120),
            StrokeWidth = 2.0f,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true 
        };

        using SKPath drawPath = new();
        drawPath.MoveTo(0, Random.Shared.Next(Height));
        drawPath.CubicTo(
            Width * 0.44f, Random.Shared.Next(Height + 10),
            Width * 0.88f, Random.Shared.Next(Height - 10),
            Width, Random.Shared.Next(Height)
        );

        canvas.DrawPath(drawPath, wavePaint);
    }
}
