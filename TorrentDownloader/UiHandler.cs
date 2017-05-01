using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TorrentDownloader
{
    public class UiHandler
    {
        public UserChoice UserChoice { get; set; } = new UserChoice();
        readonly TorrentDownloader _downloader = new TorrentDownloader();

        public UiHandler ShowMainMenu(bool continueAskingTillExit, string[] args)
        {
            if (args != null && args.Length > 0)
            {
                return this;
            }

            bool firstTime = true;
            while (continueAskingTillExit || firstTime)
            {
                firstTime = false;
                Console.Clear();
                Console.WriteLine("\n1.Enter Movie Name \n2.Exit\n What do you want? ");
                var response = Console.ReadLine();
                if (response != null && (response.Equals("2") || response.Equals("exit", StringComparison.OrdinalIgnoreCase)))
                {
                    Environment.Exit(0);
                }

                continueAskingTillExit = false;
            }

            return this;
        }

        public UiHandler ThenEnterMovieToSearch(bool continueAskingTillValid, string[] args)
        {
            if (args != null && args.Length > 0)
            {
                UserChoice.Movie = args[0].Trim();
                return this;
            }

            bool firstTime = true;

            while (firstTime || continueAskingTillValid)
            {
                firstTime = false;
                Console.WriteLine(">> Movie Name : ");
                var movieName = Console.ReadLine();

                if (!string.IsNullOrEmpty(movieName))
                {
                    UserChoice.Movie = movieName.Trim();
                    continueAskingTillValid = false;
                }
            }

            return this;
        }

        public async Task<UiHandler> ThenSearchAsync()
        {
            await _downloader.GetMovieListAsync(UserChoice.Movie).ConfigureAwait(false);
            return this;
        }

        public UiHandler ThenShowSearchResult()
        {
            Console.Clear();
            Console.WriteLine(_downloader.SearchedMovieList.Count > 0 ? $"Found Movies for keyword {UserChoice.Movie} : \n" : $"No movies found for keyword {UserChoice.Movie}");

            int index = 1;
            foreach (var movie in _downloader.SearchedMovieList)
            {
                Console.WriteLine($"{index++} {movie.Name}\n Size:{movie.Size} --- Added:{movie.Added} --- Seed:{movie.Seed} --- L:{movie.Leech}\n");
            }

            Console.WriteLine();
            return this;
        }

        public UiHandler ThenAskToChooseMovieToDownloadByIndex()
        {
            do
            {
                int choice = -1;
                Console.WriteLine("Enter the index of the Movie you want to download or -1 to exit : ");
                var input = Console.ReadLine();
                if (input != null && int.TryParse(input, out choice))
                {
                    UserChoice.Index = choice;
                }

                if (choice <= 0 || choice > _downloader.SearchedMovieList.Count)
                {
                    Console.WriteLine($"\nInvalid value entered. Enter between 1 and {_downloader.SearchedMovieList.Count} or -1 to exit.");
                }

            }
            while (UserChoice.Index <= 0 && UserChoice.Index != -1 || UserChoice.Index > _downloader.SearchedMovieList.Count);

            return this;
        }

        public async Task<UiHandler> ThenDownloadAsync()
        {
            if (UserChoice.Index <= 0)
            {
                return this;
            }

            var fileDownloaded = await _downloader.DownloadAsync(UserChoice.Index, attempt: 0).ConfigureAwait(false);
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
            return this;
        }

    }
}