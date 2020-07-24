using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using Dapper;

namespace GetMobaDataWeb
{
    public partial class moba : System.Web.UI.Page
    {
        private const string ConnectString = @"Data Source=|DataDirectory|\moba.sqlite3";

        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                BuildddlHero();
            }
        }

        void BuildddlHero()
        {
            var heroList = new Dictionary<string, string>();

            using (var connection = new SQLiteConnection(ConnectString))
            {
                var heroDataList = connection.Query("SELECT * FROM Hero Order By SortID Desc");
                foreach (var hero in heroDataList)
                {
                    var name = hero.Name.ToString();
                    if (hero.Runes == "目前尚無攻略喔！")
                        name += "(尚無攻略)";
                    heroList.Add(hero.Seq.ToString(), name);
                }
            }

            ddlHero.DataSource = heroList;
            ddlHero.DataTextField = "value";
            ddlHero.DataValueField = "Key";
            ddlHero.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            using (var connection = new SQLiteConnection(ConnectString))
            {
                var heroData = connection.Query("SELECT * FROM Hero Where Seq=@Seq", new { Seq = ddlHero.SelectedValue }).FirstOrDefault();
                if (heroData != null) liData.Text = heroData.Runes.ToString();
            }
        }
    }
}