using dZENcode.Application.Abstractions;
using dZENcode.Application.Features.Captcha.DTOs;
using dZENcode.Application.Features.Comments.DTOs;
using dZENcode.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace dZENcode.Application.Features.Comments.Commands;

public sealed record CreateCommentCommand(
    string Username,
    string Email,
    string? HomePageUrl,
    string Body,
    CaptchaChallengeAnswer CaptchaChallengeAnswer,
    int? ParentCommentId,
    CommentAttachment? Attachment
) : ICommand<int>;

internal sealed class CreateCommentCommandHandler : ICommandHandler<CreateCommentCommand, int>
{
    private readonly IApplicationDbContext _dbContext;
    
    private readonly ICaptchaService _captchaService;

    private readonly IHtmlSanitizerService _htmlSanitizer;

    private readonly IFileProcessor _imageProcessor;
    
    private readonly IFileStorage _fileStorage;

    public CreateCommentCommandHandler(
        IApplicationDbContext dbContext,
        ICaptchaService captchaService,
        IFileStorage fileStorage,
        IHtmlSanitizerService htmlSanitizer,
        IFileProcessor imageProcessor)
    {
        _dbContext = dbContext;
        _captchaService = captchaService;
        _fileStorage = fileStorage;
        _htmlSanitizer = htmlSanitizer;
        _imageProcessor = imageProcessor;
    }

    public async Task<int> HandleAsync(CreateCommentCommand command, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        bool isValidCaptchaAnswer = await _captchaService.ValidateAsync(
            command.CaptchaChallengeAnswer.Token,
            command.CaptchaChallengeAnswer.Input, cancellationToken);

        if (isValidCaptchaAnswer is false)
        {
            throw new ArgumentException($"Captcha answer is not valid.");
        }
        
        string sanitizedBody = _htmlSanitizer.Sanitize(command.Body);
        string? attachmentPath = null;;
        
        if (command.ParentCommentId is not null)
        {
            bool parentExists = await _dbContext.Comments
                .AsNoTracking()
                .AnyAsync(x => x.Id == command.ParentCommentId, cancellationToken);

            if (parentExists is false)
            {
                throw new InvalidOperationException($"Parent comment with ID {command.ParentCommentId} does not exist.");
            }
        }

        if (command.Attachment is not null)
        {
            ReadOnlyMemory<byte> attachmentBytes = command.Attachment.AttachmentKind switch
            {
                AttachmentKind.Image => _imageProcessor.Process(command.Attachment.Content),
                AttachmentKind.Text => command.Attachment.Content,
                _ => throw new InvalidOperationException("Invalid attachment kind.")
            };

            attachmentPath = await _fileStorage.SaveAsync(attachmentBytes, command.Attachment.FileName, command.Username, cancellationToken);
        }

        Comment comment = new()
        {
            ParentCommentId = command.ParentCommentId,
            Username = command.Username,
            Email = command.Email,
            HomePageUrl = command.HomePageUrl,
            Body = sanitizedBody,
            CreatedAt = DateTimeOffset.UtcNow,
            AttachmentPath = attachmentPath,
        };
        
        await _dbContext.Comments.AddAsync(comment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }
}