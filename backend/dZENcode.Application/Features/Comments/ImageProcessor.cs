using dZENcode.Application.Abstractions;
using dZENcode.Application.Abstractions.Exceptions;
using SkiaSharp;

namespace dZENcode.Application.Features.Comments;

public class ImageProcessor : IFileProcessor
{
	private const int MaxWidth = 320;
	private const int MaxHeight = 240;

	private const long MaxSourcePixels = 8192L * 8192L;

	public byte[] Process(ReadOnlyMemory<byte> source)
	{
		if (source is {Length: 0})
		{
			throw new BadRequestException("Empty source.");
		}
		
		using MemoryStream input = new();
		input.Write(source.Span);

		byte[] originalBytes = input.ToArray();

		input.Position = 0;

		using SKCodec codec = SKCodec.Create(input)
			?? throw new BadRequestException(
				"Invalid or unsupported image.");

		SKEncodedImageFormat format = codec.EncodedFormat;

		if (IsSupportedFormat(format) is false)
		{
			throw new BadRequestException(
				"Only JPG, PNG and GIF images are supported.");
		}

		int originalWidth = codec.Info.Width;
		int originalHeight = codec.Info.Height;

		long sourcePixels = (long) originalWidth * originalHeight;

		if (sourcePixels > MaxSourcePixels)
		{
			throw new BadRequestException(
				"Image dimensions are too large.");
		}

		if (originalWidth <= MaxWidth &&
			originalHeight <= MaxHeight)
		{
			return originalBytes;
		}

		double scale = Math.Min(
			(double) MaxWidth / originalWidth,
			(double) MaxHeight / originalHeight);

		int width = (int) Math.Floor(originalWidth * scale);
		int height = (int) Math.Floor(originalHeight * scale);

		width = Math.Min(width, MaxWidth);
		height = Math.Min(height, MaxHeight);

		using SKBitmap bitmap = SKBitmap.Decode(codec)
			?? throw new InvalidOperationException(
				"Failed to decode the image.");

		using SKBitmap resizedBitmap = bitmap.Resize(
				new SKImageInfo(width, height),
				SKSamplingOptions.Default)
			?? throw new InvalidOperationException(
				"Failed to resize the image.");

		using SKImage? image = SKImage.FromBitmap(resizedBitmap);

		SKEncodedImageFormat outputFormat = format == SKEncodedImageFormat.Gif
			? SKEncodedImageFormat.Png
			: format;

		int quality = outputFormat == SKEncodedImageFormat.Jpeg
			? 90
			: 100;

		using SKData encoded = image.Encode(outputFormat, quality)
			?? throw new InvalidOperationException(
				"Failed to encode the image.");

		return encoded.ToArray();
	}

	private static bool IsSupportedFormat(SKEncodedImageFormat format)
	{
		return format is
			SKEncodedImageFormat.Jpeg or
			SKEncodedImageFormat.Png or
			SKEncodedImageFormat.Gif;
	}
}