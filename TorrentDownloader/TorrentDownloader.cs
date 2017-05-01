using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace TorrentDownloader
{
    public class TorrentDownloader
    {
        private readonly Uri _baseUri = new Uri(ConfigurationManager.AppSettings["BaseUri"]);
        public List<Movie> SearchedMovieList = new List<Movie>();

        public async Task<List<Movie>> GetMovieListAsync(string movieName)
        {
            try
            {
                SearchedMovieList = await QueryExtraTorrentAsync(movieName);
                return SearchedMovieList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine($"Searching for movie: {movieName}");
                return await GetMovieListAsync(movieName);
            }
        }

        public async Task<string> DownloadAsync(int index, int attempt)
        {
            try
            {
                if (attempt > int.Parse(ConfigurationManager.AppSettings["AttemptCount"]))
                {
                    Console.WriteLine("Maximum attempt of 3 crossed. Abandoning the process");
                    Console.ReadKey();
                    return string.Empty;
                }

                var movie = SearchedMovieList[index - 1];
                var downloadUrl = movie.DownloadUrl.Replace("torrent", "download").Replace("html", "torrent");

                Console.WriteLine($"Downloading ... {movie.Name} FROM {downloadUrl}");

                if (bool.Parse(ConfigurationManager.AppSettings["OpenTorrenLinkInFdmDirectly"]))
                {
                    LaunchFdm(_baseUri + downloadUrl);
                    return "DownloadedUsingFdm";
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = _baseUri;
                    var result = await client.GetAsync(downloadUrl).ConfigureAwait(false);
                    var html = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (html.ToUpper().Contains("ERROR"))
                    {
                        Console.WriteLine(html);
                        return await DownloadAsync(index, ++attempt);
                    }

                    var stream = await result.Content.ReadAsStreamAsync().ConfigureAwait(false);

                    string file = $"{ConfigurationManager.AppSettings["DownloadLocation"]}{movie.Name}.torrent";

                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }

                    using (var fs = new FileStream(file, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite, int.Parse(stream.Length.ToString())))
                    {
                        if (fs.CanRead && fs.CanWrite)
                        {
                            stream.CopyTo(fs);
                        }
                    }
                }

                return movie.Name + ".torrent";
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return await DownloadAsync(index, ++attempt);
            }
        }

        private async Task<List<Movie>> QueryExtraTorrentAsync(string movieName)
        {
            string url = $"search/?search={movieName}{ConfigurationManager.AppSettings["SearchOptions"]}";
            List<Movie> movies = new List<Movie>();

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = _baseUri;

                Console.WriteLine("Loading...");
                var response = await client.GetAsync(url).ConfigureAwait(false);
                var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(data);
                var xPath = ConfigurationManager.AppSettings["XPath"];
                var links = document.DocumentNode.SelectNodes(xPath);

                foreach (var link in links)
                {
                    if (link.InnerText != null && link.InnerText.ToUpper().Contains(movieName.ToUpper()))
                    {

                        Movie movie = new Movie
                        {
                            Name = link.InnerText,
                            DownloadUrl = link.Attributes["href"].Value,
                            Added = link.ParentNode.ParentNode.ChildNodes[3].InnerText,
                            Size = link.ParentNode.ParentNode.ChildNodes[4].InnerText.Replace("&nbsp;", ""),
                            Seed = link.ParentNode.ParentNode.ChildNodes[5].InnerText,
                            Leech = link.ParentNode.ParentNode.ChildNodes[6].InnerText,
                        };

                        movies.Add(movie);
                    }
                }
            }

            Console.Clear();

            return movies;
        }

        public void LaunchFdm(string downloadUrl)
        {
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = @"C:\Program Files\FreeDownloadManager.ORG\Free Download Manager\fdm.exe",
                    Arguments = $"-fs {downloadUrl}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                }
            };

            pProcess.Start();
            string output = pProcess.StandardOutput.ReadToEnd();
            Console.WriteLine(output);
            pProcess.WaitForExit();
        }
    }
}