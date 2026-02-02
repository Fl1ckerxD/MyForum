using Minio;
using Minio.DataModel.Args;
using MyForum.Api.Core.Entities;
using MyForum.Api.Core.Interfaces.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Webp;

namespace MyForum.Api.Infrastructure.Services
{
    public class MinioObjectStorageService : IObjectStorageService
    {
        private readonly IMinioClient _minioClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MinioObjectStorageService> _logger;

        public MinioObjectStorageService(
            IMinioClient minioClient,
            IConfiguration configuration,
            ILogger<MinioObjectStorageService> logger)
        {
            _minioClient = minioClient;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Cохраняет файл в MinIO и создает превью для изображений.
        /// </summary>
        /// <param name="file">Файл для сохранения</param>
        /// <param name="post">Пост, к которому прикреплен файл</param>
        /// <returns>Объект PostFile с информацией о сохраненном файле</returns>
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

        /// <summary>
        /// Создает превью для изображения
        /// </summary>
        /// <param name="fileStream">Поток с изображением</param>
        /// <param name="bucketName">Имя бакета</param>
        /// <param name="originalKey">Ключ оригинального файла</param>
        /// <param name="extension">Расширение файла</param>
        /// <returns>Ключ превью</returns>
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

        /// <summary>
        /// Получает размеры изображения
        /// </summary>
        /// <param name="Width">Ширина изображения</param>
        /// <param name="bucketName">Имя бакета</param>
        /// <param name="objectKey">Ключ объекта</param>
        /// <returns>Кортеж с шириной и высотой изображения</returns>
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

        /// <summary>
        /// Возвращает поток с содержимым файла
        /// </summary>
        /// <param name="bucketName">Имя бакета</param>
        /// <param name="objectKey">Ключ объекта</param>
        /// <returns>Поток с содержимым файла</returns>
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

        /// <summary>
        /// Возвращает URL для доступа к файлу
        /// </summary>
        /// <param name="bucketName">Имя бакета</param>
        /// <param name="objectKey">Ключ объекта</param>
        /// <param name="expiresSeconds">Срок действия URL в секундах</param>
        /// <returns>URL для доступа к файлу или null, если не удалось получить URL</returns>
        public async Task<string?> GetFileUrlAsync(string bucketName, string objectKey, int expiresSeconds = 3600)
        {
            var args = new PresignedGetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectKey)
                .WithExpiry(expiresSeconds);

            var url = await _minioClient.PresignedGetObjectAsync(args);

            return ReplaceHost(url, _configuration["MinIO:PublicEndpoint"] ?? "localhost");
        }

        /// <summary>
        /// Удаляет файл из MinIO
        /// </summary>
        /// <param name="postFile">Файл для удаления</param>
        /// <returns>True, если файл успешно удален, иначе False</returns>
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

        /// <summary>
        /// Проверяет существование бакета и создает его при необходимости
        /// </summary>
        /// <param name="bucketName">Имя бакета</param>
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

        /// <summary>
        /// Заменяет хост в URL на указанный публичный endpoint
        /// </summary>
        /// <param name="originalUrl">Оригинальный URL</param>
        /// <param name="publicEndpoint">Публичный endpoint</param>
        /// <returns>URL с замененным хостом</returns>
        private string ReplaceHost(string originalUrl, string publicEndpoint)
        {
            var originalUri = new Uri(originalUrl);

            if (!publicEndpoint.StartsWith("http://") &&
                !publicEndpoint.StartsWith("https://"))
            {
                publicEndpoint = "http://" + publicEndpoint;
            }

            var publicUri = new Uri(publicEndpoint);

            var builder = new UriBuilder(originalUri)
            {
                Scheme = publicUri.Scheme,
                Host = publicUri.Host,
                Port = publicUri.Port
            };

            return builder.Uri.ToString();
        }
    }
}