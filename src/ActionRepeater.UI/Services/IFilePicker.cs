using System.Threading.Tasks;

namespace ActionRepeater.UI.Services;

public interface IFilePicker
{
    Task<FileInfo?> PickSaveFile(params (string typeName, string[] typeExtensions)[] fileTypeChoices);

    Task<FileInfo?> PickSingleFile(params string[] fileTypeFilter);
}
