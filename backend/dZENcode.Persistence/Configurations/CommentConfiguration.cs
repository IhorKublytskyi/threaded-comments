using dZENcode.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dZENcode.Persistence.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments", "Core");

        builder.HasKey(x => x.Id);

        builder
            .HasIndex(x => x.Username)
            .HasDatabaseName("IX_Comment_Username");

        builder
            .HasIndex(x => x.Email)
            .HasDatabaseName("IX_Comment_Email");

        builder
            .HasIndex(x => new { x.ParentCommentId, x.CreatedAt} )
            .HasDatabaseName("IX_Comment_ParentCommentId_CreatedAt");
        
        builder
            .Property(x => x.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder
            .Property(x => x.Username)
            .HasMaxLength(64)
            .IsRequired();

        builder
            .Property(x => x.HomePageUrl)
            .HasMaxLength(256)
            .IsRequired(false);

        builder
            .Property(x => x.Body)
            .HasMaxLength(1024)
            .IsRequired();

        builder
            .Property(x => x.CreatedAt)
            .IsRequired();
        
        builder
            .Property(x => x.AttachmentPath)
            .HasMaxLength(512)
            .IsRequired(false);

        builder
            .HasOne(x => x.Parent)
            .WithMany(y => y.Replies)
            .HasForeignKey(x => x.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
