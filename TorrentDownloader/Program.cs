using System;
using System.Net;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace TorrentDownloader
{
    internal class Program
    {
        private static readonly UiHandler UiHandler = new UiHandler();

        public static void Main(string[] args)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                                            | SecurityProtocolType.Tls11
                                                                            | SecurityProtocolType.Tls12
                                                                            | SecurityProtocolType.Ssl3;

            try
            {
                AsyncContext.Run(async () =>
                {
                    await StartApplicationAsync(args);
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        public static async Task StartApplicationAsync(string[] args)
        {
            await await UiHandler.ShowMainMenu(true, args)
                 .ThenEnterMovieToSearch(true,args)
                 .ThenSearchAsync()
                 .ContinueWith(async searchTask =>
                 {
                     var uiHandler = await searchTask;
                     await uiHandler.ThenShowSearchResult()
                         .ThenAskToChooseMovieToDownloadByIndex()
                         .ThenDownloadAsync()
                         .ContinueWith(async downloadTask =>
                         {
                             await downloadTask;
                             await StartApplicationAsync(null);
                         });
                 });
        }
    }
}
