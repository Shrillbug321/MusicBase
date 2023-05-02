using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace MusicBase.Controllers
{
	public class BlobController
	{

		public static void GetBlobServiceClient(ref BlobServiceClient blobServiceClient, string accountName, string accountKey)
		{
			StorageSharedKeyCredential sharedKeyCredential = new(accountName, accountKey);

			string blobUri = $"https://{accountName}.blob.core.windows.net";

			blobServiceClient = new BlobServiceClient
				(new Uri(blobUri), sharedKeyCredential);
		}

		public static void GetBlobClient(ref BlobClient blobClient, string accountName, string accountKey)
		{
			StorageSharedKeyCredential sharedKeyCredential = new(accountName, accountKey);

			string blobUri = $"https://{accountName}.blob.core.windows.net/kontener/text.txt";

			blobClient = new BlobClient
				(new Uri(blobUri), sharedKeyCredential);
		}

		public static async Task UploadFile(BlobContainerClient containerClient, string localFilePath)
		{
			string fileName = Path.GetFileName(localFilePath);
			BlobClient blobClient = containerClient.GetBlobClient(fileName);

			await blobClient.UploadAsync(localFilePath, true);
		}

		public static async Task DownloadFile(BlobContainerClient containerClient, string localFilePath)
		{
			string fileName = Path.GetFileName(localFilePath);
			BlobClient blobClient = containerClient.GetBlobClient(fileName);
			try
			{
				await blobClient.DownloadToAsync(localFilePath);
			}
			catch (DirectoryNotFoundException ex)
			{
				Console.WriteLine($"Directory not found: {ex.Message}");
			}
		}

		public static async Task<BinaryData> DownloadFileContent(BlobContainerClient containerClient, string fileName)
		{
			BlobClient blobClient = containerClient.GetBlobClient(fileName);
			BlobDownloadResult blob = blobClient.DownloadContent().Value;
			return blob.Content;
		}
	}
}
