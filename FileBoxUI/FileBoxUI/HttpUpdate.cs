using System.Text.Json;
using FileBoxUI.Models;

namespace FileBoxUI
{
    static class HttpUpdate
    {
        
        public static string url = "https://localhost:5001/api/fileitems/";
        
        public static async Task<List<FileItem>> GetAsync(HttpClient client)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var request = await client.GetAsync(url);
            var response = await request.Content.ReadAsStreamAsync();
            List<FileItem> files = await JsonSerializer.DeserializeAsync<List<FileItem>>(response, options);
            return files;
        }

        public static async Task PostAsync(HttpClient client, byte[] data, string fileName)
        {
           

            using (var form = new MultipartFormDataContent())
            {
                form.Add(new StreamContent(new MemoryStream(data)), "uploadedFile", fileName);
                    var result = await client.PostAsync(url, form); 
            }
        }

        public static async Task GetIdAsync(HttpClient client, string Id, string path, string name)
        {   
            var response = await client.GetAsync(url+Id);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                
                using (var fs = new FileStream(path + $"\\{name}", FileMode.Create))
                {
                    await response.Content.CopyToAsync(fs);
                }
            }

        }

        public static async Task DeleteAsync(HttpClient client, String Id)
        {
            await client.DeleteAsync(url + Id);

        }

    }
}
