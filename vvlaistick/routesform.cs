using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CCWin;
using CCWin.SkinClass;
using CCWin.SkinControl;
using Microsoft.Win32;

namespace vvlaistick
{
    public partial class routesform : Skin_Mac
    {
        mainform mfrm;
        DataTable dt = new System.Data.DataTable();
        int i = 0; int j = 0;
        public routesform()
        {
            InitializeComponent();
        }
        public routesform(mainform mainfrm)
        {
            InitializeComponent();
            mfrm = mainfrm;
        }
        private void routesform_Load(object sender, EventArgs e)
        {
            loadRoutes();
        }
        private void loadRoutes()
        {
            string loadmarkersSql = "select * from routeid order by id";
            dt = DataBase.ExecDataSet(loadmarkersSql).Tables[0];
            int i = 0;
            foreach (DataRow drmk in dt.Rows)
            {
                i++;
                ListViewItem listviewitemMK = new ListViewItem();
                listviewitemMK.SubItems[0].Text = drmk["id"].ToString();
                listviewitemMK.SubItems.Add(drmk["startlng"].ToString());
                listviewitemMK.SubItems.Add(drmk["startlat"].ToString());
                listviewitemMK.SubItems.Add(drmk["endlng"].ToString());
                listviewitemMK.SubItems.Add(drmk["endlat"].ToString());
                listviewitemMK.SubItems.Add(drmk["date"].ToString("yyyy-MM-dd HH:mm:ss"));
                listViewmarkers.Items.Add(listviewitemMK);
            }
        }
        //路线详细信息
        private void btnDetail_Click(object sender, EventArgs e)
        {
            if (listViewmarkers.CheckedItems.Count > 0)
            {
                string checkid = listViewmarkers.CheckedItems[0].SubItems[0].Text.ToString();
                routedetailform rdfrm = new routedetailform(checkid);
                rdfrm.ShowDialog();
            }
        }
        //删除路线
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listViewmarkers.CheckedItems.Count > 0)
            {
                foreach (ListViewItem selectItem in listViewmarkers.CheckedItems)
                {
                    if (selectItem.SubItems[3].Text.ToString() == "0" && selectItem.SubItems[4].Text.ToString() == "0")
                    { }
                    else
                    {
                        string selectid = selectItem.SubItems[0].Text.ToString();
                        string delMKidSql = "delete from routeid where id='" + selectid + "'";
                        DataBase.ExecuteSql(delMKidSql);
                        string delMKSql = "delete from route where id='" + selectid + "'";
                        DataBase.ExecuteSql(delMKSql);
                        listViewmarkers.Items.Clear();
                        loadRoutes();
                    }
                }
            }
        }
        //在地图上显示路线
        private void btnShow_Click(object sender, EventArgs e)
        {
            if (j < 10)
            {
                //foreach (ListViewItem selectItem in listViewmarkers.CheckedItems)
                //{
                //    mfrm.SelectRoute[j] = selectItem.SubItems[0].Text.ToString();                    
                //    j++;
                //}
                if (listViewmarkers.CheckedItems[0].SubItems[3].Text.ToString() != "0" && listViewmarkers.CheckedItems[0].SubItems[4].Text.ToString() != "0")
                {
                    mfrm.SelectRoute[j] = listViewmarkers.CheckedItems[0].SubItems[0].Text.ToString();
                    j++;
                }
            }
        }

        private void routesform_FormClosing(object sender, FormClosingEventArgs e)
        {
            mfrm.routefrm= null;
        }
    }
}
