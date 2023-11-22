#define DEBUG 1

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BeiUpdater.Common
{
    public class AppSetting
    {
        /// <summary>
        /// 
        /// </summary>
        public Notification Notification { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public TencentCloud TencentCloud { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Image Image { get; set; }

        public Trigger Trigger { get; set; }


        #if DEBUG
        /// <summary>
        /// 加载设置文件
        /// </summary>
        /// <param name="path">设置文件路径</param>
        /// <returns></returns>
        public static AppSetting Load(string path = "appsettings.Development.json")
        {
            return JsonSerializer.Deserialize<AppSetting>(File.ReadAllText(path)) ?? throw new JsonException($"Failed to load settings from {path}");
        }

        
        /// <summary>
        /// 保存设置文件
        /// </summary>
        /// <param name="path">设置文件保存路径</param>
        public void Save(string path = "appsettings.Development.json")
        {
            File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));
        }
        #else
        /// <summary>
        /// 加载设置文件
        /// </summary>
        /// <param name="path">设置文件路径</param>
        /// <returns></returns>
        public static AppSetting Load(string path = "appsettings.json")
        {
            return JsonSerializer.Deserialize<AppSetting>(File.ReadAllText(path)) ?? throw new JsonException($"Failed to load settings from {path}");
        }

        /// <summary>
        /// 保存设置文件
        /// </summary>
        /// <param name="path">设置文件保存路径</param>
        public void Save(string path = "appsettings.json")
        {
            File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));
        }   
        #endif
    }

    public class Sender
    {
        /// <summary>
        /// 
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AccessCode { get; set; }
    }

    public class Notification
    {
        /// <summary>
        /// 
        /// </summary>
        public string Receiver { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Sender Sender { get; set; }

        /// <summary>
        /// 间隔天数
        /// </summary>
        public int IntervalDays { get; set; }

        /// <summary>
        /// 上次通知时间
        /// </summary>
        public DateTime? LastNotifyTime { get; set; }
    }

    public class Account
    {
        public string AppId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SecretId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SecretKey { get; set; }
    }

    public class TencentCloud
    {
        /// <summary>
        /// 
        /// </summary>
        public Account Account { get; set; }

        public Bucket Bucket { get; set; }
    }

    public class Bucket
    {
        public string Name { get; set; }

        public string Region { get; set; }

        public string ImageKey { get; set; }
    }

    public class Image
    {
        /// <summary>
        /// 
        /// </summary>
        public string BingEverydayUrl { get; set; }

        public string CdnUrl { get; set; }
    }

    public class Trigger
    {
        public string Time { get; set; }

        [JsonIgnore]
        public TimeSpan TimeSpan
        {
            get
            {
                var baseTimeStrings = Time.Split(':');
                if (baseTimeStrings.Length == 3)
                {
                    return new TimeSpan(int.Parse(baseTimeStrings[0]),
                                        int.Parse(baseTimeStrings[1]),
                                        int.Parse(baseTimeStrings[2]));
                }
                return new TimeSpan(0, 0, 0);
            }
        }
    }
}
