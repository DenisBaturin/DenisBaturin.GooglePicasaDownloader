using System.Collections.Generic;

namespace DenisBaturin.GooglePicasaDownloader
{
    public class PicasaAlbum
    {
        public string Id { get; set; }
        public string Date { get; set; }
        public string Name { get; set; }
        public string RssUrl { get; set; }
        public List<string> ImagesUrlsList = new List<string>();

        public override string ToString()
        {
            return $"{Date}_{Name}_{Id}";
        }
    }
}