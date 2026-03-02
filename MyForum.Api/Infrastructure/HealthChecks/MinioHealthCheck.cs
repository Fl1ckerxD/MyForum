using Microsoft.Extensions.Diagnostics.HealthChecks;
using Minio;
using Minio.DataModel.Args;

namespace MyForum.Api.Infrastructure.HealthChecks
{
    public class MinioHealthCheck : IHealthCheck
    {
        private readonly IMinioClient _minioClient;

        public MinioHealthCheck(IMinioClient minioClient)
        {
            _minioClient = minioClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var listArgs = new BucketExistsArgs()
                    .WithBucket("uploads");

                bool bucketExists = await _minioClient.BucketExistsAsync(listArgs, cancellationToken);

                if (bucketExists)
                {
                    return HealthCheckResult.Healthy("MinIO storage is available.");
                }
                else
                {
                    return HealthCheckResult.Degraded("MinIO is reachable but the target bucket was not found.");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    description: "Failed to connect to MinIO storage.",
                    exception: ex);
            }
        }
    }
}