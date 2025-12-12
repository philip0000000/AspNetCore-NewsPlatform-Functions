using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

public class ResizeArticleImage
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<ResizeArticleImage> _logger;

    // Supported file types for this function
    private static readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png" };

    // Max allowed file size for processing (5 MB)
    private const int MaxInputBytes = 5 * 1024 * 1024;

    public ResizeArticleImage(BlobServiceClient blobServiceClient, ILogger<ResizeArticleImage> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    [Function("ResizeArticleImage")]
    public async Task Run(
        [BlobTrigger("article-images/{name}", Connection = "AzureWebJobsStorage")]
        byte[] originalImageBytes,
        string name)
    {
        try
        {
            _logger.LogInformation("Triggered by blob upload: {FileName}", name);

            // Extract extension and validate that we support this image type
            var ext = Path.GetExtension(name).ToLowerInvariant();
            if (!_allowedExtensions.Contains(ext))
            {
                _logger.LogWarning("File skipped. Unsupported file type: {FileName}", name);
                return; // Stop here for unsupported files
            }

            // Validate the image size to prevent very large uploads from being processed
            if (originalImageBytes.Length > MaxInputBytes)
            {
                _logger.LogWarning("File skipped. Too large: {FileName} ({Size} bytes)",
                    name, originalImageBytes.Length);
                return;
            }

            // Load the image into ImageSharp for processing
            using var inputStream = new MemoryStream(originalImageBytes);
            using var image = await Image.LoadAsync(inputStream);

            // Fixed output size for article thumbnails
            int targetWidth = 300;
            int targetHeight = 200;

            // Resize the image using cropping mode to maintain composition
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(targetWidth, targetHeight),
                Mode = ResizeMode.Crop
            }));

            // Log the image’s new dimensions after resizing
            _logger.LogInformation(
                "Image resized. New size: {Width}x{Height}",
                image.Width, image.Height
            );

            // Connect to the output container where resized images are stored
            var containerClient = _blobServiceClient.GetBlobContainerClient("article-images-resized");
            await containerClient.CreateIfNotExistsAsync();

            // Get a blob reference using the same file name as original
            var blobClient = containerClient.GetBlobClient(name);

            // Select correct encoder based on file extension
            bool isPng = ext == ".png";
            IImageEncoder encoder = isPng ? new PngEncoder() : new JpegEncoder();

            // Set proper content-type for browser rendering
            var headers = new BlobHttpHeaders
            {
                ContentType = isPng ? "image/png" : "image/jpeg"
            };

            // Save resized image to memory so it can be uploaded
            using var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, encoder);
            outputStream.Position = 0;

            // Upload the resized image back to storage
            var uploadOptions = new BlobUploadOptions { HttpHeaders = headers };
            await blobClient.UploadAsync(outputStream, uploadOptions);

            _logger.LogInformation("Image {FileName} resized and uploaded.", name);
        }
        catch (Exception ex)
        {
            // Catch and log any unexpected failures for easier debugging
            _logger.LogError(ex, "Error while processing file {FileName}", name);
        }
    }
}
