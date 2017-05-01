using System;
using System.Collections.Generic;

namespace TorrentDownloader
{
    public class UiHandler
    {
        public  bool UserChoiceInput()
        {
            Console.Clear();
            Console.WriteLine("\n1.Enter Movie Name \n2.Exit\n What do you want? ");
            var response = Console.ReadLine();
            if (response != null && response.Equals("2"))
            {
                return false;
            }

            return true;
        }

        public  string EnterMovieName()
        {
            Console.WriteLine(">> Movie Name : ");
            var movieName = Console.ReadLine();

            if (string.IsNullOrEmpty(movieName))
            {
                return EnterMovieName();
            }

            return movieName;
        }

        public  int ChooseMovieToDownloadByIndex(int movieListCount)
        {
            int choice = -1;
            Console.WriteLine("Enter the index of the Movie you want to download or -1/0 to exit : ");
            var input = Console.ReadLine();
            if (input != null && int.TryParse(input, out choice) && choice > 0 && choice <= movieListCount)
            {
                return choice;
            }

            if (choice <= 0)
            {
                return -1;
            }

            Console.WriteLine($"\nInvalid value entered. Enter between 1 and {movieListCount} or -1/0 to exit.");
            return ChooseMovieToDownloadByIndex(movieListCount);
        }

        public  void PrintMovieList(List<Movie> movieList, string searchQuery)
        {
            Console.Clear();
            Console.WriteLine(movieList.Count > 0 ? $"Found Movies for keyword {searchQuery} : \n" : $"No movies found for keyword {searchQuery}");

            int index = 1;
            foreach (var movie in movieList)
            {
                Console.WriteLine($"{index++} {movie.Name} --- S:{movie.Size} --- A:{movie.Added} --- S:{movie.Seed} --- L:{movie.Leech}");
            }

            Console.WriteLine();
        }
    }
}