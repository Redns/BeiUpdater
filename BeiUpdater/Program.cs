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
                    Thread.Sleep(50 * 1000);
                }
            }
            catch(Exception e)
            {
                await SendErrorNotifyEmail(appSetting.Notification, e);
                Logger.Error("应用已退出", e);
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
            await new Email(emailSender, emailReceiver, emailTitle, emailBody, notification.Sender.AccessCode, false).SendAsync();
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
            var emailBody = $"图片已更新，请知悉<br/>" +
                            $"<img src=\"{imagePreviewUrl}\" width=\"100%\" alt=\"Bing每日一图加载失败\">";
            await new Email(emailSender, emailReceiver,
                            emailTitle, emailBody,
                            notification.Sender.AccessCode, true).SendAsync();
        }

        /// <summary>
        /// 初始化应用
        /// </summary>
        /// <param name="appSetting"></param>
        private static void Init(AppSetting appSetting)
        {
            var cosXml = CosHelper.InitTencentCosSdk(appSetting.TencentCloud);
            var imageCdnUrl = new Uri(new Uri(appSetting.Image.CdnUrl), appSetting.TencentCloud.Bucket.ImageKey).AbsoluteUri;
            var imageUpdateTimer = new Timer()
            {
                AutoReset = true,
                Interval = GetTimerInterval(appSetting.Trigger.TimeSpan)
            };
            imageUpdateTimer.Elapsed += async (s, e) =>
            {
                await BeiUpdateAsync(appSetting, imageUpdateTimer, cosXml, imageCdnUrl);
            };
            imageUpdateTimer.Start();
        }

        /// <summary>
        /// 获取触发间隔
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        private static double GetTimerInterval(TimeSpan timeSpan)
        {
            var todayTriggerTime = DateTime.Today.Add(timeSpan);
            if (DateTime.Now < todayTriggerTime)
            {
                return (todayTriggerTime - DateTime.Now).TotalMilliseconds;
            }
            return (todayTriggerTime.AddDays(1) - DateTime.Now).TotalMilliseconds;
        }

        /// <summary>
        /// 更新必应每日一图
        /// </summary>
        /// <param name="appSetting"></param>
        /// <param name="imageUpdateTimer"></param>
        /// <param name="cosXml"></param>
        /// <param name="imageCdnUrl"></param>
        /// <returns></returns>
        private static async Task BeiUpdateAsync(AppSetting appSetting, Timer imageUpdateTimer, CosXml cosXml, string imageCdnUrl)
        {
            // 计算下次触发间隔
            imageUpdateTimer.Interval = GetTimerInterval(appSetting.Trigger.TimeSpan);
            if((appSetting.Notification.LastNotifyTime is not null) && ((DateTime.Now - appSetting.Notification.LastNotifyTime).Value.Days < appSetting.Notification.IntervalDays))
            {
                return;
            }

            // 获取 Bing 每日一图并上传至 COS
            using var beiReadStream = await BingHelper.GetBingImageStreamAsync(appSetting.Image.BingEverydayUrl);
            if(!CosHelper.UploadImageToCOS(beiReadStream, cosXml, appSetting.TencentCloud.Bucket))
            {
                Logger.Error("图片上传至 COS 失败");
                return;
            }
            CosHelper.UpdateTencentCDN(appSetting.TencentCloud, imageCdnUrl);

            // 更新设置
            appSetting.Notification.LastNotifyTime = DateTime.Now;
            appSetting.Save();

            // 发送通知邮件
            await Task.Delay(appSetting.TencentCloud.CdnWaitUpdateSeconds * 1000);
            await SendUpdateNormalNotifyEmail(appSetting.Notification, imageCdnUrl);

            // 释放图片
            await beiReadStream.FlushAsync();
            await beiReadStream.DisposeAsync();

            Logger.Info("图片已更新");
        }
    }
}