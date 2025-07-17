using scrbl.Services;
using Spectre.Console.Cli;

namespace scrbl.Commands
{
    /// <summary>
    /// Abstract class to sync notes to the server
    /// </summary>
    /// <typeparam name="TSettings"></typeparam>
    public abstract class AutoSyncCommand<TSettings> : AsyncCommand<TSettings> where TSettings : CommandSettings
    {
        
        public override async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
        {
            var result = await ExecuteLocalAsync(context, settings);

            if (result != 0) return result;
            var syncService = new SyncService();
            await syncService.TrySyncAsync(showMessages: true);

            return result;
        }

        protected abstract Task<int> ExecuteLocalAsync(CommandContext context, TSettings settings);
    }
}