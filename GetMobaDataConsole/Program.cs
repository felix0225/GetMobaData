using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Data.SQLite;
using Dapper;
using HtmlAgilityPack;

namespace GetMobaDataConsole
{
    class Program
    {
        private const string ConnectString = @"Data Source=D:\Dropbox\MyProject\韋仲\GetMobaData\GetMobaDataWeb\App_Data\moba.sqlite3";

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var client = new WebClient();

            GetHeros(client);

            GetHeroRuneIDs(client);

            GetRune(client);

            Console.WriteLine("enter any key...");
            Console.ReadKey();
        }

        private static void GetHeros(WebClient client)
        {
            const string siteUri = "https://moba.garena.tw/game/heroes";

            //try
            //{
            using (var ms = new MemoryStream(client.DownloadData(siteUri)))
            {
                //讀取所有HTML
                var hdc = new HtmlDocument();
                hdc.Load(ms, Encoding.UTF8);

                var heroSortID = 0;

                //讀取英雄
                var herosCol = hdc.DocumentNode.SelectNodes("//*[@id='h_list']/ul/li");
                foreach (var hero in herosCol.Reverse())
                {
                    //設定排序編號
                    heroSortID++;

                    //讀取英雄ID和名稱
                    var id = hero.SelectSingleNode("a").GetAttributeValue("href", "").Replace(siteUri.Replace("/heroes", "/hero/"), "");
                    var name = hero.GetAttributeValue("data-filter", "");
                    Console.WriteLine(id + "，" + name);

                    using (var connection = new SQLiteConnection(ConnectString))
                    {
                        try
                        {
                            var result = connection.Execute("INSERT INTO Hero VALUES (@Seq, @Name, @Ids, @Ids2, @SortID, @Runes)",
                                        new { Seq = id, Name = name, Ids = "", Ids2 = "", SortID = heroSortID, Runes = "" });
                            Console.WriteLine("result=" + result);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
        }

        private static void GetHeroRuneIDs(WebClient client)
        {
            const string siteUri = "https://pro.moba.garena.tw/hero/";

            using (var connection = new SQLiteConnection(ConnectString))
            {

                var heroList = connection.Query("SELECT * FROM Hero");
                foreach (var hero in heroList)
                {
                    try
                    {
                        var id = hero.Seq;
                        Console.WriteLine("id=" + id);
                        using (var ms = new MemoryStream(client.DownloadData(siteUri + id)))
                        {
                            //讀取所有HTML
                            var hdc = new HtmlDocument();
                            hdc.Load(ms, Encoding.UTF8);

                            //取攻略ID
                            var itemwCol = hdc.DocumentNode.SelectNodes("/html/body/div/div/div[2]/div/div[4]/div[2]/div");
                            var itemwStr = "";
                            foreach (var itemw in itemwCol)
                            {
                                var a = itemw.SelectSingleNode("a");
                                if (a == null) continue;
                                itemwStr += a.GetAttributeValue("href", "").Replace("https://pro.moba.garena.tw/guide/", "") + ",";
                            }
                            Console.WriteLine("itemwStr=" + itemwStr);

                            connection.Execute("UPDATE Hero SET Ids=@Ids WHERE Seq=@Seq",
                                    new { Ids = itemwStr, Seq = id });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private static void GetRune(WebClient client)
        {
            const string siteUri = "https://pro.moba.garena.tw/guide/";

            using (var connection = new SQLiteConnection(ConnectString))
            {
                //try
                //{
                var heroList = connection.Query("SELECT * FROM Hero");
                //var heroList = connection.Query("SELECT * FROM Hero where seq=1");
                foreach (var hero in heroList)
                {
                    var seq = hero.Seq;
                    Console.WriteLine("seq=" + seq);

                    var ids = hero.Ids + hero.Ids2 as string;
                    if (!string.IsNullOrWhiteSpace(ids))
                    {
                        var dataSB = new StringBuilder();

                        var idArr = ids.TrimEnd(',').Split(',');
                        foreach (var id in idArr)
                        {
                            Console.WriteLine("id=" + id);
                            try
                            {
                                using (var ms = new MemoryStream(client.DownloadData(siteUri + id)))
                                {
                                    //讀取所有HTML
                                    var hdc = new HtmlDocument();
                                    hdc.Load(ms, Encoding.UTF8);

                                    //讀取主題
                                    var norguideTitle = hdc.DocumentNode
                                        .SelectSingleNode("/html/body/div/div/div[2]/div/div[1]/div[1]/div").InnerText
                                        .Trim();
                                    dataSB.AppendFormat("<b>主題：<a href='{0}{1}' target='_blank'>{2}</a></b><br/>", siteUri, id, norguideTitle);

                                    //讀取英雄的資料
                                    var herodataName = hdc.DocumentNode
                                        .SelectSingleNode("/html/body/div/div/div[2]/div/div[2]/div[2]/div[1]").InnerHtml
                                        .Trim();
                                    Console.WriteLine("Name：" + herodataName);

                                    //判斷是否有逆風局出裝順序，如果沒有，抓的順序要往前移
                                    var aitem = hdc.DocumentNode
                                        .SelectSingleNode("/html/body/div/div/div[2]/div/div[4]/div[1]")
                                        .InnerText.Trim();
                                    var divIndex = aitem == "逆風局出裝順序" ? 6 : aitem == "技能點法順序" ? 5 : 4;

                                    //讀取奧義
                                    var runesCol =
                                        hdc.DocumentNode.SelectNodes($"/html/body/div/div/div[2]/div/div[{divIndex}]/div");
                                    foreach (var rune in runesCol)
                                    {
                                        //奧義名稱
                                        dataSB.AppendFormat("<b>{0}</b><br/>", rune.SelectSingleNode("div").InnerText.Trim());

                                        var mainCol = rune.SelectNodes("div[2]/div");
                                        foreach (var item in mainCol)
                                        {
                                            dataSB.AppendFormat("&nbsp;【{0}】：{1}<br/>", item.SelectSingleNode("div").InnerText.Trim(), item.SelectSingleNode("div[2]").InnerText.Trim().Replace("\n", "，"));
                                        }
                                    }
                                    dataSB.AppendFormat("<br/>");
                                }
                            }
                            catch (WebException ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }

                        connection.Execute("UPDATE Hero SET Runes=@Runes WHERE Seq=@Seq",
                            new { Runes = dataSB.ToString(), Seq = seq });
                    }
                    else
                    {
                        connection.Execute("UPDATE Hero SET Runes=@Runes WHERE Seq=@Seq",
                            new { Runes = "目前尚無攻略喔！", Seq = seq });
                    }
                }
                //}
                //catch (WebException ex)
                //{
                //    Console.WriteLine(ex.Message);
                //}
            }
        }
    }
}
