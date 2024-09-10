using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Quingo.Infrastructure.Files;

public class FileStoreService
{
    private readonly IMinioClient _minio;
    private readonly FileStoreSettings _settings;

    public FileStoreService(IMinioClient minio, IOptions<FileStoreSettings> storageOptions)
    {
        _minio = minio;
        _settings = storageOptions.Value;
    }

    public async Task UploadFile(Stream data, string filename, string contentType)
    {
        var args = new PutObjectArgs()
            .WithBucket(_settings.Bucket)
            .WithStreamData(data)
            .WithFileName(filename)
            .WithContentType(contentType);
        await _minio.PutObjectAsync(args);
    }

    public string GetFileUrl(string filename)
    {
        var prefix = _settings.UrlPrefix.EndsWith('/') ? _settings.UrlPrefix : _settings.UrlPrefix + "/";
        return prefix + filename;
    }
}
