namespace BeiUpdater.Entities
{
    public class BingArchiveEntity
    {
        public List<ImageItem> Images { get; set; }

        public Tooltips Tooltips { get; set; }
    }


    public class ImageItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string Startdate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Fullstartdate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? Enddate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Urlbase { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Copyright { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Copyrightlink { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Quiz { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Wp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Hsh { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Drk { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Bot { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string>? Hs { get; set; }
    }


    /// <summary>
    /// 工具提示
    /// </summary>
    public class Tooltips
    {
        /// <summary>
        /// 正在加载...
        /// </summary>
        public string Loading { get; set; }

        /// <summary>
        /// 上一个图像
        /// </summary>
        public string Previous { get; set; }

        /// <summary>
        /// 下一个图像
        /// </summary>
        public string Next { get; set; }

        /// <summary>
        /// 此图片不能下载用作壁纸。
        /// </summary>
        public string Walle { get; set; }

        /// <summary>
        /// 下载今日美图。仅限用作桌面壁纸。
        /// </summary>
        public string Walls { get; set; }
    }
}
