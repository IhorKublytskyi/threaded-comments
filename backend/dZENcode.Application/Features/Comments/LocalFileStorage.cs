using dZENcode.Application.Abstractions;
using dZENcode.Application.Abstractions.Exceptions;
using dZENcode.Application.Features.Comments.DTOs;
using Microsoft.Extensions.Options;

namespace dZENcode.Application.Features.Comments;

public record FileWrapper(
    byte[] Content,
    string Extension);

public class LocalAttachmentsFileStorage : IFileStorage
{
    private const string FilesDirectoryName = "Attachments"; 

    private readonly string _baseDirectory;

    public LocalAttachmentsFileStorage(IOptions<StorageOptions> options)
    {
        _baseDirectory = Path.Combine(options.Value.RootPath, FilesDirectoryName);
    }
    public async Task<FileWrapper> GetFileAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new BadRequestException("Invalid path.");
        }

        string filePath = ResolveSafePath(path);

        if (File.Exists(filePath) is false)
        {
            throw new FileNotFoundException("File not found.");
        }

        byte[] content = await File.ReadAllBytesAsync(filePath, cancellationToken);
        
        return new FileWrapper(content, Path.GetExtension(filePath));
    }

    public async Task<string> SaveAsync(ReadOnlyMemory<byte> source, string fileName, string uniqueDirectory, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(uniqueDirectory))
        {
            throw new ArgumentNullException(nameof(uniqueDirectory));
        }

        string directory = ResolveSafePath(uniqueDirectory);

        if (Directory.Exists(directory) is false)
        {
            Directory.CreateDirectory(directory);
        }

        string extension = Path.GetExtension(fileName);

        string uniqueFileName = $"{Guid.NewGuid().ToString()}{extension}";

        string destinationPath = Path.Combine(directory, uniqueFileName);

        await using FileStream fileStream = File.Create(destinationPath);
        
        await fileStream.WriteAsync(source, cancellationToken);

        // Relative path
        return Path.Combine(uniqueDirectory, uniqueFileName).Replace('\\', '/');
    }
    
    private string ResolveSafePath(params string[] segments)
    {
        string full = Path.GetFullPath(Path.Combine(_baseDirectory, Path.Combine(segments)));
        string baseFull = Path.GetFullPath(_baseDirectory);

        if (full != baseFull && full.StartsWith(baseFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) is false)
            throw new BadRequestException("Invalid path.");

        return full;
    }
}