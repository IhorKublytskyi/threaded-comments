namespace dZENcode.Application.Features.Comments.DTOs;

public record CommentAttachment(
    ReadOnlyMemory<byte> Content,
    string FileName)
{
    public int Length => Content.Length;
    
    public AttachmentKind AttachmentKind => 
        Path.GetExtension(FileName).Equals(".txt", StringComparison.OrdinalIgnoreCase) 
            ? AttachmentKind.Text 
            : AttachmentKind.Image;
}

public enum AttachmentKind
{
    Image,
    Text
}
