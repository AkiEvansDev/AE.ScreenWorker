﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenWorkerWPF.Common;

internal static class Extensions
{
    public static async Task DownloadAsync(this HttpClient client, string requestUri, Stream destination, Action<float> progress = null, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var contentLength = response.Content.Headers.ContentLength;

        using var download = await response.Content.ReadAsStreamAsync(cancellationToken);

        if (progress == null || !contentLength.HasValue)
        {
            await download.CopyToAsync(destination, cancellationToken);
            return;
        }

        void relativeProgress(long totalBytes) => progress((float)totalBytes / contentLength.Value);
        await download.CopyToAsync(destination, 81920, relativeProgress, cancellationToken);
        progress(1);
    }

    public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, Action<long> progress = null, CancellationToken cancellationToken = default)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (!source.CanRead)
            throw new ArgumentException("Has to be readable", nameof(source));

        if (destination == null)
            throw new ArgumentNullException(nameof(destination));

        if (!destination.CanWrite)
            throw new ArgumentException("Has to be writable", nameof(destination));

        if (bufferSize < 0)
            throw new ArgumentOutOfRangeException(nameof(bufferSize));

        var buffer = new byte[bufferSize];
        long totalBytesRead = 0;
        int bytesRead;

        while ((bytesRead = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) != 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
            totalBytesRead += bytesRead;
            progress?.Invoke(totalBytesRead);
        }
    }
}
