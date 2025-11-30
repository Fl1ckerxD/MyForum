using Minio;
using Minio.DataModel.Args;
using MyForum.Core.Entities;
using MyForum.Core.Interfaces.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Webp;

namespace MyForum.Infrastructure.Services
{
    public class MinioFileService : IFileService
{
    private readonly IMinioClient _minioClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MinioFileService> _logger;

    public MinioFileService(
        IMinioClient minioClient, 
        IConfiguration configuration, 
        ILogger<MinioFileService> logger)
    {
        _minioClient = minioClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<PostFile> SaveFileAsync(IFormFile file, Post post, CancellationToken cancellationToken = default)
    {
        // Создаем уникальное имя файла
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var bucketName = _configuration["MinIO:BucketName"] ?? "uploads";
        
        // Убеждаемся, что бакет существует
        await EnsureBucketExistsAsync(bucketName, cancellationToken);
        
        // Сохраняем оригинальный файл в MinIO
        using var fileStream = file.OpenReadStream();
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(storedFileName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(file.ContentType);
        
        await _minioClient.PutObjectAsync(putObjectArgs);
        
        // Создаем превью для изображений
        string? thumbnailKey = null;
        int? thumbnailWidth = null, thumbnailHeight = null;
        int? imageWidth = null, imageHeight = null;
        
        if (IsImageFile(extension))
        {
            try
            {
                // Сбрасываем позицию потока для повторного чтения
                fileStream.Position = 0;
                
                // Получаем размеры оригинального изображения
                using var image = await Image.LoadAsync(fileStream);
                imageWidth = image.Width;
                imageHeight = image.Height;
                
                // Создаем превью
                thumbnailKey = await CreateThumbnailAsync(fileStream, bucketName, storedFileName, extension, cancellationToken);
                
                // Получаем размеры превью
                var thumbnailSize = await GetImageDimensionsAsync(bucketName, thumbnailKey);
                thumbnailWidth = thumbnailSize.Width;
                thumbnailHeight = thumbnailSize.Height;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось создать превью для файла {FileName}", file.FileName);
            }
        }
        
        return new PostFile
        {
            FileName = file.FileName,
            StoredFileName = storedFileName,
            FileExtension = extension,
            FileSize = file.Length,
            ContentType = file.ContentType,
            Width = imageWidth,
            Height = imageHeight,
            ThumbnailKey = thumbnailKey,
            ThumbnailWidth = thumbnailWidth,
            ThumbnailHeight = thumbnailHeight,
            Post = post,
            BucketName = bucketName
        };
    }

    private async Task<string> CreateThumbnailAsync(Stream fileStream, string bucketName, string originalKey, string extension, CancellationToken cancellationToken)
    {
        var thumbnailKey = $"{Path.GetFileNameWithoutExtension(originalKey)}_thumb{Path.GetExtension(originalKey)}";
        
        fileStream.Position = 0;
        using var image = await Image.LoadAsync(fileStream);
        
        // Ресайзим изображение с сохранением пропорций
        var resizeOptions = new ResizeOptions
        {
            Size = new Size(250, 250),
            Mode = ResizeMode.Max // Сохраняем пропорции, вписывая в квадрат 250x250
        };
        
        image.Mutate(x => x.Resize(resizeOptions));
        
        // Настройки кодирования для уменьшения размера
        var encoder = GetImageEncoder(extension);
        
        // Сохраняем превью в MemoryStream
        using var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, encoder, cancellationToken);
        outputStream.Position = 0;
        
        // Загружаем превью в MinIO
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(thumbnailKey)
            .WithStreamData(outputStream)
            .WithObjectSize(outputStream.Length)
            .WithContentType(GetContentType(extension));
        
        await _minioClient.PutObjectAsync(putObjectArgs);
        
        return thumbnailKey;
    }

    private async Task<(int Width, int Height)> GetImageDimensionsAsync(string bucketName, string objectKey)
    {
        try
        {
            using var stream = await GetFileStreamAsync(bucketName, objectKey);
            using var image = await Image.LoadAsync(stream);
            return (image.Width, image.Height);
        }
        catch
        {
            return (0, 0);
        }
    }

    public async Task<Stream> GetFileStreamAsync(string bucketName, string objectKey)
    {
        try
        {
            var memoryStream = new MemoryStream();
            
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectKey)
                .WithCallbackStream(stream => stream.CopyTo(memoryStream));
            
            await _minioClient.GetObjectAsync(getObjectArgs);
            memoryStream.Position = 0;
            
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении файла {BucketName}/{ObjectKey}", bucketName, objectKey);
            throw;
        }
    }

    public string GetFileUrl(string bucketName, string objectKey)
    {
        // Для MinIO можно генерировать URL напрямую
        var endpoint = _configuration["MinIO:Endpoint"] ?? "localhost:9000";
        var useSsl = _configuration.GetValue<bool>("MinIO:WithSSL");
        var protocol = useSsl ? "https" : "http";
        
        return $"{protocol}://{endpoint}/{bucketName}/{objectKey}";
    }

    public async Task<bool> DeleteFileAsync(PostFile postFile, CancellationToken cancellationToken = default)
    {
        try
        {
            // Удаляем основной файл
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(postFile.BucketName)
                .WithObject(postFile.StoredFileName);
            
            await _minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken);
            
            // Удаляем превью если есть
            if (!string.IsNullOrEmpty(postFile.ThumbnailKey))
            {
                var removeThumbArgs = new RemoveObjectArgs()
                    .WithBucket(postFile.BucketName)
                    .WithObject(postFile.ThumbnailKey);
                
                await _minioClient.RemoveObjectAsync(removeThumbArgs);
            }
            
            _logger.LogInformation("Файл {FileName} успешно удален", postFile.FileName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении файла {FileName}", postFile.FileName);
            return false;
        }
    }

    private async Task EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            var bucketExistsArgs = new BucketExistsArgs().WithBucket(bucketName);
            var found = await _minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);
            
            if (!found)
            {
                var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
                await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
                _logger.LogInformation("Бакет {BucketName} создан", bucketName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании бакета {BucketName}", bucketName);
            throw;
        }
    }

    private IImageEncoder GetImageEncoder(string extension) => extension.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => new JpegEncoder { Quality = 75 },
        ".png" => new PngEncoder(),
        ".gif" => new GifEncoder(),
        ".webp" => new WebpEncoder(),
        _ => new JpegEncoder()
    };

    private string GetContentType(string extension) => extension.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        _ => "application/octet-stream"
    };

    private bool IsImageFile(string extension) => 
        new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }.Contains(extension.ToLowerInvariant());
}
}