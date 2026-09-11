using System;
using System.Globalization;
using System.Threading.Tasks;

public partial class BackgroundWorker
{
    /// <summary>
    /// 清除過期的檔案 log 與資料庫 log（保留 7 天）
    /// </summary>
    public async Task TaskDeleteLog()
    {
        try
        {

            #region 刪除檔案LOG

            string LogPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "log"
            );

            if (Directory.Exists(LogPath))
            {
                var directories = Directory.GetDirectories(LogPath);

                foreach (var dir in directories)
                {

                    try
                    {
                        string dirName = Path.GetFileName(dir);
                        DateTime dirDate = DateTime.ParseExact(dirName, "yyyyMMdd", CultureInfo.InvariantCulture);

                        if (dirDate < DateTime.Now.AddDays(-7))
                        {

                            _logger.LogInformation($"Directory: {dir}");

                            var subDirectories = Directory.GetDirectories(dir);

                            foreach (var subDir in subDirectories)
                            {
                                var files = Directory.GetFiles(subDir);

                                foreach (var file in files)
                                {
                                    _logger.LogInformation($"Deleting file: {file}");
                                    File.Delete(file);
                                }

                                Directory.Delete(subDir);

                            }

                            Directory.Delete(dir);
                        }
                    }
                    catch
                    {

                    }

                }
            }

            #endregion

            #region 刪除資料庫LOG
            // TokenTime 是 varchar(20) 的時間戳記字串（yyyyMMddHHmmssffff），非真正的 timestamp 欄位，
            // 故取前 8 碼（日期部分）與 7 天前的日期以文字方式比較（LEFT 對應原 T-SQL 的 SUBSTRING(TokenTime,0,9)）。
            clsPostgreDB.execNonQuery(
                @"
                    Delete
                    From SYS_ttLog
                    where SystemTime < now() - interval '7 days';

                    Delete
                    From SYS_ttToken
                    Where LEFT(TokenTime, 8) < to_char(now() - interval '7 days', 'YYYYMMDD');
                "
            , new { });

            #endregion


            // 成功結果寫回設定檔
            RecordRes("TaskDeleteLog", "Success");

            _ = DoAppRecLog("TaskDeleteLog", "Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TaskDeleteLog failed.");

            // 失敗結果寫回設定檔
            RecordRes("TaskDeleteLog", "Failed:" + ex.ToString());

            _ = DoAppRecLog("TaskDeleteLog", "Failed:" + ex.ToString());
        }
    }
}
