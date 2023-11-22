using BeiUpdater.Common;
using BeiUpdater.Helpers;
using COSXML;
using log4net;
using MimeKit;
using Task = System.Threading.Tasks.Task;
using Timer = System.Timers.Timer;

namespace BeiUpdater
{
    public class Program
    {
        /// <summary>
        /// 日志对象
        /// </summary>
        private static ILog? _logger;
        public static ILog Logger
        {
            get
            {
                if (_logger is null)
                {
                    _logger = LogManager.GetLogger("*");
                    log4net.Config.XmlConfigurator.Configure(configFile: new FileInfo("log4net.config"));
                }
                return _logger;
            }
        }


        public static async Task Main()
        {
            
            var appSetting = AppSetting.Load();
            try
            {
                Init(appSetting);
                while (true)
                {
                    Thread.Sleep(60 * 1000);
                }
            }
            catch(Exception e)
            {
                Logger.Error("应用已退出", e);
                await SendErrorNotifyEmail(appSetting.Notification, e);
            }
        }


        /// <summary>
        /// 发送异常邮件
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static async Task SendErrorNotifyEmail(Notification notification, Exception e)
        {
            var emailSender = new MailboxAddress("BeiUpdater", notification.Sender.Account);
            var emailReceiver = new MailboxAddress("Administrator", notification.Receiver);
            var emailTitle = "BeiUpdater";
            var emailBody = $"服务发生错误已强制退出，请知悉\n" +
                            "------Error Stack Begin------\n" +
                            $"[ERROR] {e.Message}\n" +
                            $"{e.StackTrace}\n" +
                            "-------Error Stack End------- ";
            await new Email(emailSender, emailReceiver, 
                            emailTitle, emailBody,
                            notification.Sender.AccessCode, false).SendAsync();
        }


        /// <summary>
        /// 发送图片正常更新邮件
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static async Task SendUpdateNormalNotifyEmail(Notification notification, string imagePreviewUrl)
        {
            var emailSender = new MailboxAddress("BeiUpdater", notification.Sender.Account);
            var emailReceiver = new MailboxAddress("Administrator", notification.Receiver);
            var emailTitle = "BeiUpdater";
            var emailBody = $"图片已更新，请知悉\n" +
                            $"<img src=\"{imagePreviewUrl}\" width=\"100%\" alt=\"Bing每日一图加载失败\">";
            await new Email(emailSender, emailReceiver,
                            emailTitle, emailBody,
                            notification.Sender.AccessCode, true).SendAsync();
            // var emailBody = "图片已更新，请知悉";
            await new Email(emailSender, emailReceiver,
                            emailTitle, emailBody,
                            notification.Sender.AccessCode, true).SendAsync();
        }


        private static async void Init(AppSetting appSetting)
        {
            // 图片 CDN 地址
            var imageCdnUrl = new Uri(new Uri(appSetting.Image.CdnUrl), appSetting.TencentCloud.Bucket.ImageKey).AbsoluteUri;
            // 初始化日志服务
            var logger = log4net.Config.XmlConfigurator.Configure(configFile: new FileInfo("log4net.config"));
            // 初始化 COS 临时鉴权
            var cosXml = CosHelper.InitTencentCosSdk(appSetting.TencentCloud);
            // 初始化更新定时器
            var imageUpdateTimer = new Timer()
            {
                AutoReset = true
            };
            // 初始化定时器事件
            await BeiUpdateAsync(appSetting, imageUpdateTimer, cosXml, imageCdnUrl);
            imageUpdateTimer.Elapsed += async (s, e) =>
            {
                await BeiUpdateAsync(appSetting, imageUpdateTimer, cosXml, imageCdnUrl);
            };
            imageUpdateTimer.Start();
        }


        private static async Task BeiUpdateAsync(AppSetting appSetting, Timer imageUpdateTimer, CosXml cosXml, string imageCdnUrl)
        {
            // 计算下次触发间隔
            var todayTriggerTime = DateTime.Today.Add(appSetting.Trigger.TimeSpan);
            if (DateTime.Now < todayTriggerTime)
            {
                imageUpdateTimer.Interval = (todayTriggerTime - DateTime.Now).TotalMilliseconds;
            }
            else
            {
                imageUpdateTimer.Interval = (todayTriggerTime.AddDays(1) - DateTime.Now).TotalMilliseconds;
            }
            // 获取 Bing 每日一图并上传至 COS
            using var beiReadStream = await BingHelper.GetBingImageStreamAsync(appSetting.Image.BingEverydayUrl);
            if (CosHelper.UploadImageToCOS(beiReadStream, cosXml, appSetting.TencentCloud.Bucket))
            {
                CosHelper.UpdateTencentCDN(appSetting.TencentCloud, imageCdnUrl);
                if((appSetting.Notification.LastNotifyTime is null) || ((DateTime.Now - appSetting.Notification.LastNotifyTime).Value.Days > appSetting.Notification.IntervalDays))
                {
                    // 更新设置
                    appSetting.Notification.LastNotifyTime = DateTime.Now;
                    appSetting.Save();
                    // 等待 CDN 刷新
                    await Task.Delay(60 * 1000);
                    // 发送通知邮件
                    await SendUpdateNormalNotifyEmail(appSetting.Notification, imageCdnUrl);
                }
            }
            else
            {
                imageUpdateTimer.Stop(); throw new Exception("图片上传至 COS 失败");
            }
            // 释放图片
            await beiReadStream.FlushAsync();
            await beiReadStream.DisposeAsync();
        }
    }
}