using System.Text;
using Alma.Data;
using Alma.Helper;

namespace Alma.Services;

public class MetadataHandler : IMetadataHandler
{
    private const string MetadataFilename = "moduleHashes.txt";
    private readonly IFolderService _folderService;

    public MetadataHandler(IFolderService folderService)
    {
        _folderService = folderService;
    }

    public async Task SaveLinkedItemsAsync(List<ItemToLink> successfulLinks, DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory)
    {
        var sourcePathHash = MD5Helper.GetMD5Hash(sourceDirectory.FullName);
        var targetPathHash = MD5Helper.GetMD5Hash(targetDirectory.FullName);
        var modulePathHash = MD5Helper.GetMD5Hash(sourcePathHash + targetPathHash);
        var appDataDirectory = new DirectoryInfo(_folderService.AppData);

        var moduleFolderMetadataPath = Path.Combine(appDataDirectory.FullName, modulePathHash + ".txt");

        var previousData = new List<string>();

        if (File.Exists(moduleFolderMetadataPath))
        {
            var content = await File.ReadAllLinesAsync(moduleFolderMetadataPath);
            previousData.AddRange(content.Skip(1));
        }

        var newContent = previousData.Concat(successfulLinks.Select(s => s.TargetPath)).Distinct();
        await File.WriteAllLinesAsync(moduleFolderMetadataPath, newContent.Prepend("text"));

        //TODO write md5 & path to a common file
        var hashAlreadySaved = false;

        var metadataFilePath = Path.Combine(_folderService.AppData, MetadataFilename);
        if (File.Exists(metadataFilePath))
        {
            await using var metadataFileStream = File.OpenRead(metadataFilePath);
            using var metadataFileStream2 = new StreamReader(metadataFileStream);
            while (await metadataFileStream2.ReadLineAsync() is { } s)
            {
                if (!s.StartsWith(modulePathHash)) continue;

                hashAlreadySaved = true;
                break;
            }
        }

        if (!hashAlreadySaved)
        {
            var newLineContent = modulePathHash + " " + EncodePath(sourceDirectory.FullName) + " " + EncodePath(targetDirectory.FullName);
            await File.AppendAllLinesAsync(metadataFilePath, new[] {newLineContent});
        }

        static string EncodePath(string path)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(path);
            return Convert.ToBase64String(toEncodeAsBytes);
        }
    }
}