using Alma.Data;

namespace Alma.Services;

public interface IMetadataHandler
{
    Task SaveLinkedItemsAsync(List<ItemToLink> successfulLinks, DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory);
}