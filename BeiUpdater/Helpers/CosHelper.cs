using BeiUpdater.Common;
using COSXML;
using COSXML.Model.Object;
using TencentCloud.Cdn.V20180606;
using TencentCloud.Common;
using TencentCloud.Cdn.V20180606.Models;

namespace BeiUpdater.Helpers
{
    public static class CosHelper
    {
        /// <summary>
        /// 初始化 COS SDK
        /// </summary>
        /// <param name="tencentCloud"></param>
        /// <returns></returns>
        public static CosXml InitTencentCosSdk(Common.TencentCloud tencentCloud)
        {
            // 初始化 CosXmlConfig 
            var config = new CosXmlConfig.Builder()
                                         .IsHttps(true)                         // 设置默认 HTTPS 请求
                                         .SetRegion(tencentCloud.Bucket.Region) // 设置一个默认的存储桶地域
                                         .SetDebugLog(true)                     // 显示日志
                                         .Build();                              // 创建 CosXmlConfig 对象
            // 提供访问凭证
            var cosCredentialProvider = new CustomQCloudCredentialProvider(tencentCloud);
            // 初始化 CosXmlServer
            return new CosXmlServer(config, cosCredentialProvider);
        }


        /// <summary>
        /// 上传图片至 COS
        /// </summary>
        /// <param name="imageStream">图片文件流</param>
        /// <param name="cosXml"></param>
        /// <param name="bucket"></param>
        /// <returns>上传成功返回 true，否则返回 false</returns>
        public static bool UploadImageToCOS(Stream imageStream, CosXml cosXml, Bucket bucket)
        {
            // 组装上传请求
            var offset = 0L;
            var sendLength = imageStream.Length;
            var request = new PutObjectRequest(bucket.Name, bucket.ImageKey, imageStream, offset, sendLength);
            //执行请求
            return cosXml.PutObject(request).IsSuccessful();
        }


        /// <summary>
        /// 更新腾讯云 CDN
        /// </summary>
        /// <param name="tencentCloud"></param>
        /// <param name="cdnUrl"></param>
        public static void UpdateTencentCDN(Common.TencentCloud tencentCloud, string cdnUrl)
        {
            // 实例化一个认证对象
            var cred = new Credential
            {
                SecretId = tencentCloud.Account.SecretId,
                SecretKey = tencentCloud.Account.SecretKey
            };
            // 更新 cdn
            new CdnClient(cred, string.Empty).PurgeUrlsCacheSync(new PurgeUrlsCacheRequest()
            {
                Urls = new string[] { cdnUrl }
            });
        }
    }
}
