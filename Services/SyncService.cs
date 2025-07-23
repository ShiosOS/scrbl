using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using scrbl.Managers;
using Spectre.Console;
using System;
using System.IO;

namespace scrbl.Services
{
    public class SyncService
    {
        private readonly HttpClient _httpClient;
        private readonly string _notesPath;
        private readonly bool _isSyncEnabled;

        public SyncService()
        {
            var config = ConfigManager.LoadConfig();
            _notesPath = config.NotesFilePath;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

            // Check if both URL and API Key are present.
            if (!string.IsNullOrEmpty(config.ServerUrl) && !string.IsNullOrEmpty(config.ServerApiKey))
            {
                _isSyncEnabled = true;
                _httpClient.BaseAddress = new Uri(config.ServerUrl);
                // Add the API Key to the default headers for every request.
                _httpClient.DefaultRequestHeaders.Add("X-Api-Key", config.ServerApiKey);
            }
            else
            {
                _isSyncEnabled = false;
            }
        }

        public async Task TrySyncAsync(bool showMessages = true)
        {
            if (!_isSyncEnabled)
            {
                return;
            }

            try
            {
                if (showMessages) AnsiConsole.MarkupLine("[dim]Attempting to sync with server...[/]");
                
                var remoteMetadata = await GetRemoteMetadataAsync();
                if (remoteMetadata == null)
                {
                    if (showMessages) AnsiConsole.MarkupLine("[yellow]Server not reachable. Skipping sync.[/]");
                    return;
                }

                var localTimestamp = File.GetLastWriteTimeUtc(_notesPath);

                if (Math.Abs((localTimestamp - remoteMetadata.LastModified).TotalSeconds) < 2)
                {
                    if (showMessages) AnsiConsole.MarkupLine("[green]✓ Notes are already up to date.[/]");
                }
                else if (localTimestamp > remoteMetadata.LastModified)
                {
                    if (showMessages) AnsiConsole.MarkupLine("[cyan]Local changes detected. Pushing to server...[/]");
                    await PushChangesAsync();
                }
                else
                {
                    if (showMessages) AnsiConsole.MarkupLine("[cyan]Server has newer changes. Pulling from server...[/]");
                    if (AnsiConsole.Confirm("[yellow]Overwrite local notes with changes from the server?[/]"))
                    {
                        await PullChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                if (showMessages) AnsiConsole.MarkupLine($"[red]Sync failed: {ex.Message}[/]");
            }
        }

        private async Task<NotesMetadata?> GetRemoteMetadataAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/notes/metadata");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    AnsiConsole.MarkupLine("[red]Sync failed: Invalid API Key.[/]");
                    return null;
                }
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<NotesMetadata>();
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        private async Task PushChangesAsync()
        {
            var fileContent = await File.ReadAllTextAsync(_notesPath);
            var request = new { Content = fileContent };
            var response = await _httpClient.PostAsJsonAsync("/api/notes/content", request);
            if (response.IsSuccessStatusCode)
            {
                AnsiConsole.MarkupLine("[green]✓ Successfully pushed changes.[/]");
            }
            else
            {
                 AnsiConsole.MarkupLine($"[red]Error pushing changes: {await response.Content.ReadAsStringAsync()}[/]");
            }
        }

        private async Task PullChangesAsync()
        {
            var response = await _httpClient.GetAsync("/api/notes/content");
            if (response.IsSuccessStatusCode)
            {
                var remoteContent = await response.Content.ReadAsStringAsync();
                await File.WriteAllTextAsync(_notesPath, remoteContent);
                AnsiConsole.MarkupLine("[green]✓ Successfully pulled changes.[/]");
            }
        }
    }

    public class NotesMetadata
    {
        public DateTimeOffset LastModified { get; set; }
    }
}
