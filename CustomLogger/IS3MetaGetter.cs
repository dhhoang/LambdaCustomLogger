using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.Extensions.Logging;

namespace CustomLogger;

internal interface IS3MetaGetter
{
    Task PrintMetaAsync(string bucket, string key);
}

internal class S3MetaGetter : IS3MetaGetter
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3MetaGetter> _logger;

    public S3MetaGetter(IAmazonS3 s3Client, ILogger<S3MetaGetter> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
    }

    public async Task PrintMetaAsync(string bucket, string key)
    {
        var meta = await _s3Client.GetObjectMetadataAsync(bucket, key);

        foreach (var metaKey in meta.Headers.Keys)
        {
            _logger.LogInformation("Key '{Key}' value '{Value}'", metaKey, meta.Headers[metaKey]);
        }
    }
}
