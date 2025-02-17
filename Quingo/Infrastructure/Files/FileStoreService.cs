using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Quingo.Infrastructure.Files;

public partial class FileStoreService
{
    private readonly FileStoreSettings _fileSettings;
    private readonly IAmazonS3 _client;

    public FileStoreService(IOptions<FileStoreSettings> storageOptions, IAmazonS3 client)
    {
        _fileSettings = storageOptions.Value;
        _client = client;
    }

    [GeneratedRegex("^https{0,1}:\\/\\/")]
    private static partial Regex UrlRegex();

    private readonly Regex _urlRegex = UrlRegex();

    public async Task<string> UploadBrowserFile(IBrowserFile file)
    {
        await using var data = file.OpenReadStream();
        return await UploadFile(file.Name, file.ContentType, data);
    }

    public async Task<string> UploadFile(string fileName, string contentType, Stream data, string? prefix = null)
    {
        var keyPrefix = prefix ?? Guid.NewGuid().ToString("N");
        var prefixedFileName = $"{keyPrefix}_{fileName}";
        var req = new PutObjectRequest
        {
            BucketName = _fileSettings.Bucket,
            Key = prefixedFileName,
            InputStream = data,
            ContentType = contentType,
        };

        await _client.PutObjectAsync(req);
        return prefixedFileName;
    }

    public string GetFileUrl(string? filename)
    {
        if (filename != null && _urlRegex.IsMatch(filename)) return filename;

        var prefix = _fileSettings.UrlPrefix.EndsWith('/') ? _fileSettings.UrlPrefix : _fileSettings.UrlPrefix + "/";
        return prefix + filename;
    }
}
