using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Xml;

namespace DenisBaturin.GooglePicasaDownloader
{
    internal class Program
    {
        private static readonly List<PicasaAlbum> AlbumsList = new List<PicasaAlbum>();

        private static void Main(string[] args)
        {
            Console.WriteLine("DenisBaturin.GooglePicasaDownloader");
            Console.WriteLine();
            Console.WriteLine(DateTime.Now);

            try
            {
                // получаем UserID
                // это набор цифр в конце главной страницы с альбомами
                // например, вот такой: https://picasaweb.google.com/117399782941050049827
                Console.Write("Input UserId: ");
                var userId = Console.ReadLine();

                // каталог для сохранения альбомов
                Console.Write("Input directory path for download: ");
                var rootDirectory = Console.ReadLine();
                
                // формируем ссылку на rss с главной страницы с альбомами
                var albumsRssLink = $"https://picasaweb.google.com/data/feed/api/user/{userId}";

                // получаем список альбомов из rss
                FillAlbumsFromRss(albumsRssLink);

                // заполняем список ссылок фотографий (в полном размере, для скачивания) для каждого альбома из его rss
                FillImagesUrlsForAlbums();

                // создаём файл со списком альбомов и фотографий
                CreateFileForAlbums(rootDirectory);

                // скачиваем все фотографии альбомов
                DownloadAlbums(rootDirectory);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            Console.WriteLine($"Downloaded {AlbumsList.Count} albums and {AlbumsList.SelectMany(x => x.ImagesUrlsList).Count()} pictures.");
            Console.WriteLine(DateTime.Now);
            Console.WriteLine("press any key to continue...");
            Console.ReadKey();
        }

        private static void FillAlbumsFromRss(string rssUrl)
        {
            var rssFormatter = new Atom10FeedFormatter();

            using (var xmlReader = XmlReader.Create(rssUrl))
            {
                rssFormatter.ReadFrom(xmlReader);
            }

            foreach (var syndicationItem in rssFormatter.Feed.Items)
            {
                var album = new PicasaAlbum();
                var startIndex = syndicationItem.Id.LastIndexOf("/", StringComparison.Ordinal) + 1;
                album.Id = syndicationItem.Id.Substring(startIndex);
                album.Date = syndicationItem.PublishDate.DateTime.ToString("yyyy-MM-dd");
                album.Name = Path.GetInvalidFileNameChars()
                    .Aggregate(syndicationItem.Title.Text, (current, c) => current.Replace(c, '_'));
                album.RssUrl = syndicationItem.Id.Replace("entry", "feed");
                AlbumsList.Add(album);
            }
        }

        private static void FillImagesUrlsForAlbums()
        {
            var rssFormatter = new Atom10FeedFormatter();

            foreach (var album in AlbumsList)
            {
                using (var xmlReader = XmlReader.Create(album.RssUrl))
                {
                    rssFormatter.ReadFrom(xmlReader);
                }

                foreach (var syndicationItem in rssFormatter.Feed.Items)
                {
                    var imageUrl = ((UrlSyndicationContent) syndicationItem.Content).Url.ToString();
                    var start = imageUrl.LastIndexOf("/", StringComparison.Ordinal) + 1;
                    var imageUrlForDownload = imageUrl.Insert(start, "d/");
                    album.ImagesUrlsList.Add(imageUrlForDownload);
                }
            }
        }

        private static void CreateFileForAlbums(string rootDirectory)
        {
            using (var outputFile = new StreamWriter(Path.Combine(rootDirectory, "albums.txt")))
            {
                foreach (var album in AlbumsList)
                {
                    outputFile.WriteLine(album);
                    foreach (var imageUrl in album.ImagesUrlsList)
                    {
                        outputFile.WriteLine(imageUrl);
                    }
                    outputFile.WriteLine();
                }
            }
        }

        private static void DownloadAlbums(string rootDirectory)
        {
            var webClient = new WebClient();

            foreach (var album in AlbumsList)
            {
                var dirName = Path.Combine(rootDirectory, album.ToString());
                Directory.CreateDirectory(dirName);

                foreach (var imageUrl in album.ImagesUrlsList)
                {
                    var fileName = Path.GetFileName(imageUrl);
                    if (fileName == null || fileName.Length > 20)
                    {
                        fileName = Guid.NewGuid() + ".jpg";
                    }
                    webClient.DownloadFile(imageUrl, Path.Combine(dirName, fileName));
                }
            }
        }
    }
}