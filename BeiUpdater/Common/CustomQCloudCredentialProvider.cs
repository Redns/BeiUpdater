using COSSTS;
using COSXML.Auth;
using System.Text.Json;

namespace BeiUpdater.Common
{
    public class CustomQCloudCredentialProvider : DefaultSessionQCloudCredentialProvider
    {
        private TencentCloud _tencentCloud { get; set; }

        public CustomQCloudCredentialProvider(TencentCloud tencentCloud) : base(null, null, 0L, null)
        {
            _tencentCloud = tencentCloud;
        }

        /// <summary>
        /// 更新临时密钥
        /// </summary>
        public override void Refresh()
        {
            // 获取临时密钥
            var tempoarySecrets = GetCredential(_tencentCloud);
            // 解析密钥信息
            var credentials = JsonSerializer.Deserialize<Credentials>(tempoarySecrets["Credentials"].ToString() ?? string.Empty) ?? throw new ArgumentException(nameof(tempoarySecrets));
            var tmpStartTime = long.Parse(tempoarySecrets["StartTime"].ToString() ?? "0L");
            var tmpExpiredTime = long.Parse(tempoarySecrets["ExpiredTime"].ToString() ?? "0L");
            // 更新临时密钥
            SetQCloudCredential(credentials.TmpSecretId, credentials.TmpSecretKey,
              $"{tmpStartTime};{tmpExpiredTime}", credentials.Token);
        }


        /// <summary>
        /// 获取联合身份临时访问凭证
        /// </summary>
        private Dictionary<string, object> GetCredential(TencentCloud tencentCloud)
        {
            // 允许的操作范围
            var allowActions = new string[] {  
                "name/cos:PutObject",
                "name/cos:PostObject",
                "name/cos:InitiateMultipartUpload",
                "name/cos:ListMultipartUploads",
                "name/cos:ListParts",
                "name/cos:UploadPart",
                "name/cos:CompleteMultipartUpload"
            };
            // 添加请求参数
            var values = new Dictionary<string, object>
            {
                { "bucket", tencentCloud.Bucket.Name },
                { "region", tencentCloud.Bucket.Region },
                { "allowPrefix", tencentCloud.Bucket.ImageKey },
                { "allowActions", allowActions },
                { "durationSeconds", 1800 },
                { "secretId", tencentCloud.Account.SecretId },
                { "secretKey", tencentCloud.Account.SecretKey }
            };
            // 解析响应请求
            // Credentials = {
            //   "Token": "4oztDXOAAI3c6qUE5TkNuVzSP1tUQz15f3f946eb08f9411d3d61505cc4bc74cczCZLchkvRmmrqzE09ELVw35gzYlBXsQp03PBpL79ubLvoAMWbBgSMmI6eApmhqv7NFeDdKJlikVe0fNCU2NNUe7cHrgttfTIK87ZnC86kww-HysFgIGeBNWpwo4ih0lV0z9a2WiTIjPoeDBwPU4YeeAVQAGPnRgHALoL2FtxNsutFzDjuryRZDK7Am4Cs9YxpZHhG7_F_II6363liKNsHTk8ONRZrNxKiOqvFvyhsJ-oTTUg0I0FT4_xo0lq5zR9yyySXHbE7z-2im4rgnK3sBagN47zkgltJyefJmaPUdDgGmvaQBO6TqxiiszOsayS7CxCZK1yi90H2KS3xRUYTLf94aVaZlufrIwntXIXZaHOKHmwuZuXl7HnHoXbfg_YENoLP6JAkDCw0GOFEGNOrkCuxRtcdJ08hysrwBw1hmYawDHkbyxYkirY-Djg7PswiC4_juBvG0iwjzVwE0W_rhxIa7YtamLnZJxQk9dyzbbl0F4DTYwS101Hq9wC7jtifkXFjBFTGRnfPe85K-hEnJLaEy7eYfulIPI9QiIUxi4BLPbzjD9j3qJ4Wdt5oqk9XcF9y5Ii2uQx1eymNl7qCA",
            //   "TmpSecretId": "xxxxxxxxxxxx",
            //   "TmpSecretKey": "PZ/WWfPZFYqahPSs8URUVMc8IyJH+T24zdn8V1cZaMs="
            // }
            // ExpiredTime = 1597916602
            // Expiration = 2020/8/20 上午9:43:22
            // RequestId = 2b731be1-ebe8-4638-8a72-906bc564a55a
            // StartTime = 1597914802
            return STSClient.genCredential(values);
        }
    }


    public class Credentials
    {
        public string Token { get; set; }

        public string TmpSecretId { get; set; }

        public string TmpSecretKey { get; set; }
    }
}
