using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace QUAN_LY_APP.Services
{
    public class GoogleResult { public List<GoogleItem> Items { get; set; } }
    public class GoogleItem { public GoogleVolume VolumeInfo { get; set; } }
    public class GoogleVolume
    {
        public string Title { get; set; }
        public GoogleImage ImageLinks { get; set; }
    }
    public class GoogleImage { public string Thumbnail { get; set; } }

    public class GoogleBooksService
    {
        public async Task<GoogleVolume> GetBookInfo(string isbn)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"https://www.googleapis.com/books/v1/volumes?q=isbn:{isbn}";
                try
                {
                    string json = await client.GetStringAsync(url);
                    var data = JsonConvert.DeserializeObject<GoogleResult>(json);

                    if (data.Items != null && data.Items.Count > 0)
                        return data.Items[0].VolumeInfo;
                }
                catch { return null; }
                return null;
            }
        }
    }
}