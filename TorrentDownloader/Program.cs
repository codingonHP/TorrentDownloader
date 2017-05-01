using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace TorrentDownloader
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                                            | SecurityProtocolType.Tls11
                                                                            | SecurityProtocolType.Tls12
                                                                            | SecurityProtocolType.Ssl3;

            try
            {
               AsyncContext.Run(MainAsync);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        public static async Task MainAsync()
        {
            UiHandler uiHandler = new UiHandler();

            while (uiHandler.UserChoiceInput())
            {
                while (true)
                {
                    Console.Clear();
                    var movieName = uiHandler.EnterMovieName();
                    if (!string.IsNullOrEmpty(movieName))
                    {
                        try
                        {
                            TorrentDownloader downloader = new TorrentDownloader();
                            var movieList = await downloader.GetMovieList(movieName);
                            if (movieList.Count > 0)
                            {
                                uiHandler.PrintMovieList(movieList, movieName);
                                int index = uiHandler.ChooseMovieToDownloadByIndex(movieList.Count);
                                if (index <= 0)
                                {
                                    break;
                                }

                                var fileDownloaded = await downloader.DownloadAsync(index, 0);
                                Console.Clear();
                                if (!string.IsNullOrEmpty(fileDownloaded))
                                {
                                    Process.Start("explorer.exe", $@"/select, {ConfigurationManager.AppSettings["DownloadLocation"]}{fileDownloaded}");
                                    Console.WriteLine("Download successful! Press any key to go to main menu.");
                                }
                                else if (!string.IsNullOrEmpty(fileDownloaded) && fileDownloaded.Equals("DownloadedUsingFdm"))
                                {
                                    Console.WriteLine("Download successful using FDM! Press any key to go to main menu.");
                                }
                                else
                                {
                                    Console.WriteLine("Download failed! Press any key to go to main menu.");
                                }

                                Console.ReadKey();
                                break;
                            }

                            Console.WriteLine($"No movies found by name {movieName}. Would you like to enter again? ");
                            var inputAgain = Console.ReadLine();
                            if (inputAgain != null && (inputAgain.Equals("N", StringComparison.OrdinalIgnoreCase) || inputAgain.Equals("NO", StringComparison.OrdinalIgnoreCase)))
                            {
                                break;
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                            Console.WriteLine("Press Key to continue");
                            Console.ReadKey();
                        }
                    }
                }
            }
        }


    }
}
