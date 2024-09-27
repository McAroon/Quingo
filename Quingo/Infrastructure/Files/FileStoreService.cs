using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;

namespace Quingo.Infrastructure.Files;

public class FileStoreService
{
    private readonly FileStoreSettings _fileSettings;
    private readonly IAmazonS3 _client;

    public FileStoreService(IOptions<FileStoreSettings> storageOptions, IAmazonS3 client)
    {
        _fileSettings = storageOptions.Value;
        _client = client;
    }

    public async Task<string> UploadBrowserFile(IBrowserFile file)
    {
        var filename = $"{Guid.NewGuid():N}_{file.Name}";
        using var data = file.OpenReadStream();

        var req = new PutObjectRequest
        {
            BucketName = _fileSettings.Bucket,
            Key = filename,
            InputStream = data,
            ContentType = file.ContentType,
        };

        var res = await _client.PutObjectAsync(req);
        return filename;
    }

    public string GetFileUrl(string? filename)
    {
        var prefix = _fileSettings.UrlPrefix.EndsWith('/') ? _fileSettings.UrlPrefix : _fileSettings.UrlPrefix + "/";
        return prefix + filename;
    }
}
