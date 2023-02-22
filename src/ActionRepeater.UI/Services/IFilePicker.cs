using System.Threading.Tasks;

namespace ActionRepeater.UI.Services;

public interface IFilePicker
{
    Task<FileInfo?> PickSaveFileAsync(params (string typeName, string[] typeExtensions)[] fileTypeChoices);

    Task<FileInfo?> PickSingleFileAsync(params string[] fileTypeFilter);
}
