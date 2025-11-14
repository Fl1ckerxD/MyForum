using System.ComponentModel.DataAnnotations;
using MyForum.Core.Interfaces.Services;

namespace MyForum.Core.Entities
{
    public class PostFile
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(255)]
        public string StoredFileName { get; set; }

        [MaxLength(10)]
        public string? FileExtension { get; set; }

        public long FileSize { get; set; }
        public string? ContentType { get; set; }

        // Метаданные для изображений
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? ThumbnailKey { get; set; }
        public int? ThumbnailWidth { get; set; }
        public int? ThumbnailHeight { get; set; }

        // MinIO информация
        public string BucketName { get; set; } = "uploads";
        public string ObjectKey => $"{StoredFileName}";

        // Внешние ключи
        public int PostId { get; set; }

        // Навигационные свойства
        public Post Post { get; set; }

        // Вычисляемое свойство для URL
        public string GetFileUrl(IFileService fileService)
            => fileService.GetFileUrl(BucketName, ObjectKey);

        public string? GetThumbnailUrl(IFileService fileService)
            => ThumbnailKey != null ? fileService.GetFileUrl(BucketName, ThumbnailKey) : null;
    }
}