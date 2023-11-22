using BeiUpdater.Entities;
using System.Text.Json;

namespace BeiUpdater.Helpers
{
    public static class BingHelper
    {
        /// <summary>
        /// 获取 Bing 每日一图
        /// </summary>
        /// <returns></returns>
        public static async ValueTask<Stream> GetBingImageStreamAsync(string imgArchiveApi)
        {
            // 获取每日一图存档信息
            var client = new HttpClient();
            var bingImg = (JsonSerializer.Deserialize<BingArchiveEntity>(await client.GetStringAsync(imgArchiveApi), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })?.Images.FirstOrDefault()) ?? throw new Exception("Failed to get bing image archive");
            // 获取图片链接
            var bingImgUri = new Uri(new Uri("https://cn.bing.com"), bingImg.Url);
            return await client.GetAsync(bingImgUri).Result.Content.ReadAsStreamAsync();
        }
    }
}
