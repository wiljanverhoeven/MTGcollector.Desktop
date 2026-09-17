using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace MTGcollector_app.Helpers;

public static class WebImage
{
    private static readonly HttpClient HttpClient = CreateClient();
    private static readonly ConcurrentDictionary<string, Bitmap> Cache = new();

    public static readonly AttachedProperty<string?> UrlProperty =
        AvaloniaProperty.RegisterAttached<Image, string?>("Url", typeof(WebImage));

    private static readonly AttachedProperty<CancellationTokenSource?> LoadCtsProperty =
        AvaloniaProperty.RegisterAttached<Image, CancellationTokenSource?>("LoadCts", typeof(WebImage));

    static WebImage()
    {
        UrlProperty.Changed.AddClassHandler<Image>(OnUrlChanged);
    }

    public static string? GetUrl(Image image) => image.GetValue(UrlProperty);

    public static void SetUrl(Image image, string? value) => image.SetValue(UrlProperty, value);

    private static HttpClient CreateClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("MTGcollector", "1.0"));
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("image/jpeg"));
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("image/png"));
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("image/*"));
        return client;
    }

    private static void OnUrlChanged(Image image, AvaloniaPropertyChangedEventArgs args)
    {
        image.GetValue(LoadCtsProperty)?.Cancel();

        var url = args.GetNewValue<string?>();
        if (string.IsNullOrWhiteSpace(url))
        {
            image.Source = null;
            return;
        }

        if (Cache.TryGetValue(url, out var cached))
        {
            image.Source = cached;
            return;
        }

        image.Source = null;

        var cts = new CancellationTokenSource();
        image.SetValue(LoadCtsProperty, cts);
        _ = LoadAsync(image, url, cts.Token);
    }

    private static async Task LoadAsync(Image image, string url, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await HttpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory, cancellationToken);
            memory.Position = 0;

            var bitmap = new Bitmap(memory);
            var stored = Cache.GetOrAdd(url, bitmap);
            if (!ReferenceEquals(stored, bitmap))
                bitmap.Dispose();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!cancellationToken.IsCancellationRequested && GetUrl(image) == url)
                    image.Source = stored;
            });
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
        }
    }
}
