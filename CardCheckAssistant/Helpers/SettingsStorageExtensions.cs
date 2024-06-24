using CardCheckAssistant.Core.Helpers;

using System.Diagnostics;
using System.Reflection;

using Windows.Storage;
using Windows.Storage.Streams;

namespace CardCheckAssistant.Helpers;

// Use these extension methods to store and retrieve local and roaming app data
// More details regarding storing and retrieving app data at https://docs.microsoft.com/windows/apps/design/app-settings/store-and-retrieve-app-data
public static class SettingsStorageExtensions
{
    private const string FileExtension = ".json";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="appData"></param>
    /// <returns></returns>
    public static bool IsRoamingStorageAvailable(this ApplicationData appData)
    {
        return appData.RoamingStorageQuota == 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="folder"></param>
    /// <param name="name"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static async Task SaveAsync<T>(this StorageFolder folder, string name, T content)
    {
        EventLog eventLog = new("Application", ".", Assembly.GetEntryAssembly().GetName().Name);

        try
        {
            var file = await folder.CreateFileAsync(GetFileName(name), CreationCollisionOption.ReplaceExisting);
            var fileContent = await Json.StringifyAsync(content);

            await FileIO.WriteTextAsync(file, fileContent);
        }
        catch (Exception ex)
        {
            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="folder"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static async Task<T?> ReadAsync<T>(this StorageFolder folder, string name)
    {
        EventLog eventLog = new("Application", ".", Assembly.GetEntryAssembly().GetName().Name);

        try
        {
            if (!File.Exists(Path.Combine(folder.Path, GetFileName(name))))
            {
                return default;
            }

            var file = await folder.GetFileAsync($"{name}.json");
            var fileContent = await FileIO.ReadTextAsync(file);
            return await Json.ToObjectAsync<T>(fileContent);
        }
        catch (Exception ex)
        {
            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);

            return default;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="settings"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task SaveAsync<T>(this ApplicationDataContainer settings, string key, T value)
    {
        EventLog eventLog = new("Application", ".", Assembly.GetEntryAssembly().GetName().Name);

        try
        {
            settings.SaveString(key, await Json.StringifyAsync(value));
        }
        catch (Exception ex)
        {
            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void SaveString(this ApplicationDataContainer settings, string key, string value)
    {
        EventLog eventLog = new("Application", ".", Assembly.GetEntryAssembly().GetName().Name);

        try
        {
            settings.Values[key] = value;
        }
        catch (Exception ex)
        {
            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
        } 
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="settings"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static async Task<T?> ReadAsync<T>(this ApplicationDataContainer settings, string key)
    {
        EventLog eventLog = new("Application", ".", Assembly.GetEntryAssembly().GetName().Name);

        try
        {
            object? obj;

            if (settings.Values.TryGetValue(key, out obj))
            {
                return await Json.ToObjectAsync<T>((string)obj);
            }
            return default;
        }
        catch (Exception ex)
        {
            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
            return default;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="content"></param>
    /// <param name="fileName"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<StorageFile> SaveFileAsync(this StorageFolder folder, byte[] content, string fileName, CreationCollisionOption options = CreationCollisionOption.ReplaceExisting)
    {
        EventLog eventLog = new("Application", ".", Assembly.GetEntryAssembly().GetName().Name);

        try
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name is null or empty. Specify a valid file name", nameof(fileName));
            }

            var storageFile = await folder.CreateFileAsync(fileName, options);
            await FileIO.WriteBytesAsync(storageFile, content);
            return storageFile;
        }
        catch (Exception ex)
        {
            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);

            return default;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static async Task<byte[]?> ReadFileAsync(this StorageFolder folder, string fileName)
    {
        EventLog eventLog = new("Application", ".", Assembly.GetEntryAssembly().GetName().Name);

        try
        {
            var item = await folder.TryGetItemAsync(fileName).AsTask().ConfigureAwait(false);

            if ((item != null) && item.IsOfType(StorageItemTypes.File))
            {
                var storageFile = await folder.GetFileAsync(fileName);
                var content = await storageFile.ReadBytesAsync();
                return content;
            }

            return null;
        }
        catch (Exception ex)
        {
            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);

            return default;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static async Task<byte[]?> ReadBytesAsync(this StorageFile file)
    {
        EventLog eventLog = new("Application", ".", Assembly.GetEntryAssembly().GetName().Name);

        try
        {
            if (file != null)
            {
                using IRandomAccessStream stream = await file.OpenReadAsync();
                using var reader = new DataReader(stream.GetInputStreamAt(0));
                await reader.LoadAsync((uint)stream.Size);
                var bytes = new byte[stream.Size];
                reader.ReadBytes(bytes);
                return bytes;
            }

            return null;
        }
        catch (Exception ex)
        {
            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);

            return null;
        }
    }

    private static string GetFileName(string name)
    {
        EventLog eventLog = new("Application", ".", Assembly.GetEntryAssembly().GetName().Name);

        try
        {
            return string.Concat(name, FileExtension);
        }
        catch (Exception ex)
        {
            eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);

            return string.Empty;
        }
        
    }
}
