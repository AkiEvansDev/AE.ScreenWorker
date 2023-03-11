using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AE;
using AE.Core;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;

using File = Google.Apis.Drive.v3.Data.File;

namespace WebWork;

public static class DriveHelper
{
    public static Action<string> OnLog;
    public static Action<Action> Invoke;

    private const string USER_AGENT = "ScreenWorker";
    private const string USERS_FOLDER = "Users";
    private const string SCRIPTS_FOLDER = "Scripts";

    public async static Task<List<DriveFileInfo>> SearchFiles(DriveService service, string folderId, string text, CancellationToken token)
    {
        var files = new List<DriveFileInfo>();

        try
        {
            var request = service.Files.List();
            request.Fields = "files(id, size, name, description)";
            request.Q = $"parents in '{folderId}' and name contains '{text}'";

            var result = await request.ExecuteAsync(token);
            if (result != null)
            {
                foreach (var file in result.Files)
                    files.Add(new DriveFileInfo(file.Id, file.Size ?? 0, file.Name ?? "", file.Description ?? ""));

                return files;
            }
        }
        catch (Exception ex)
        {
            OnLog?.Invoke($"<E>[Error]</E> {nameof(GetFile)} =<AL></AL><NL></NL>{ex.Message}");
        }

        return files;
    }

    public async static Task<string> CreateFile(DriveService service, string folderId, string name, string description, object data, Action<float> progress, CancellationToken token)
    {
        try
        {
            var fileMetadata = new File
            {
                Name = name,
                MimeType = "text/plain",
                Parents = new List<string> { folderId },
                Description = description
            };

            var dataStr = data.Serialize();
            using var stream = new MemoryStream();
            using var sw = new StreamWriter(stream);

            await sw.WriteAsync(dataStr);
            sw.Flush();

            var request = service.Files.Create(fileMetadata, stream, fileMetadata.MimeType);
            request.ChunkSize = ResumableUpload.MinimumChunkSize;
            request.ProgressChanged += p =>
            {
                Invoke?.Invoke(() =>
                {
                    progress((float)p.BytesSent / stream.Length);
                });
            };

            var result = await request.UploadAsync(token);
            progress(1);

            if (result.Status == UploadStatus.Completed)
                return await GetFileId(service, folderId, name, token);

            OnLog?.Invoke($"<E>[Error]</E> {nameof(CreateFile)} =<AL></AL><NL></NL>{result.Exception.Message}");
        }
        catch (Exception ex)
        {
            OnLog?.Invoke($"<E>[Error]</E> {nameof(CreateFile)} =<AL></AL><NL></NL>{ex.Message}");
        }

        return null;
    }

    public async static Task<bool> DeleteFile(DriveService service, string fileId, CancellationToken token)
    {
        try
        {
            var request = service.Files.Delete(fileId);
            var result = await request.ExecuteAsync(token);

            return true;
        }
        catch (Exception ex)
        {
            OnLog?.Invoke($"<E>[Error]</E> {nameof(DeleteFile)} =<AL></AL><NL></NL>{ex.Message}");
        }

        return false;
    }

    public async static Task<string> UpdateFile(DriveService service, string fileId, string name, string description, CancellationToken token)
    {
        try
        {
            var fileMetadata = new File
            {
                Name = name,
                Description = description
            };

            var request = service.Files.Update(fileMetadata, fileId);
            var result = await request.ExecuteAsync(token);

            return result.Id;
        }
        catch (Exception ex)
        {
            OnLog?.Invoke($"<E>[Error]</E> {nameof(UpdateFile)} =<AL></AL><NL></NL>{ex.Message}");
        }

        return null;
    }

    public async static Task<string> GetFileData(DriveService service, string fileId, Action<float> progress, CancellationToken token)
    {
        try
        {
            var request = service.Files.Get(fileId);
            request.Fields = "size";

            var fileInfo = await request.ExecuteAsync(token);
            var len = fileInfo.Size ?? 1;

            request.MediaDownloader.ChunkSize = 1024;
            request.MediaDownloader.ProgressChanged += p =>
            {
                Invoke?.Invoke(() =>
                {
                    progress((float)p.BytesDownloaded / len);
                });
            };

            using var stream = new MemoryStream();
            var result = await request.DownloadAsync(stream, token);
            progress(1);

            if (result.Status == DownloadStatus.Completed)
            {
                stream.Position = 0;
                using var sr = new StreamReader(stream);

                return await sr.ReadToEndAsync();
            }

            OnLog?.Invoke($"<E>[Error]</E> {nameof(GetFileData)} =<AL></AL><NL></NL>{result.Exception.Message}");
        }
        catch (Exception ex)
        {
            OnLog?.Invoke($"<E>[Error]</E> {nameof(GetFileData)} =<AL></AL><NL></NL>{ex.Message}");
        }

        return null;
    }

    public static Task<string> GetUsersFolderId(DriveService service, CancellationToken token)
    {
        return GetFolderId(service, USERS_FOLDER, token);
    }

    public static Task<string> GetScriptsFolderId(DriveService service, CancellationToken token)
    {
        return GetFolderId(service, SCRIPTS_FOLDER, token);
    }

    private async static Task<string> GetFolderId(DriveService service, string name, CancellationToken token)
    {
        var file = await GetFile(service, $"mimeType = 'application/vnd.google-apps.folder' and name contains '{name}'", token);
        if (file != null)
            return file.Id;

        return null;
    }

    public async static Task<string> GetFileId(DriveService service, string folderId, string name, CancellationToken token)
    {
        var file = await GetFile(service, $"parents in '{folderId}' and name contains '{name}'", token);
        if (file != null)
            return file.Id;

        return null;
    }

    private async static Task<File> GetFile(DriveService service, string filter, CancellationToken token)
    {
        try
        {
            var request = service.Files.List();
            request.Q = filter;

            var result = await request.ExecuteAsync(token);
            if (result != null && result.Files.Any())
                return result.Files.First();
        }
        catch (Exception ex)
        {
            OnLog?.Invoke($"<E>[Error]</E> {nameof(GetFile)} =<AL></AL><NL></NL>{ex.Message}");
        }

        return null;
    }

    public static DriveService GetDriveService()
    {
        var scopes = new string[] { DriveService.Scope.Drive };
        using var stream = Secrets.GetGoogleJson();

        var credential = GoogleCredential.FromStream(stream);
        credential = credential.CreateScoped(scopes);

        return new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = USER_AGENT,
        });
    }
}
