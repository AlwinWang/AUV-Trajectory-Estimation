using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CCWin;
using CCWin.SkinClass;
using CCWin.SkinControl;
using Microsoft.Win32;
using System.IO.Ports;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace vvlaistick
{
    public partial class setform : Skin_Mac
    {       
        string filepath = null;       
        public setform()
        {
            InitializeComponent();
            LoadParameterXml();
        }

        private void setform_Load(object sender, EventArgs e)
        {                  
            txtfileurl.Text = filepath;              
        }
        private void LoadParameterXml()
        {
            XmlDocument xDoc = new XmlDocument();

            xDoc.Load(Application.StartupPath + "\\" + "Parameter.xml");
            XmlNode xn = xDoc.SelectSingleNode("root");
            XmlNodeList xlist = xn.ChildNodes;
            foreach (XmlNode xmln in xlist)
            {
                ListViewItem eqitem = new ListViewItem();
                XmlElement xe = (XmlElement)xmln;
                XmlNodeList xlist1 = xe.ChildNodes;                     
                filepath = xlist1.Item(0).InnerText;                      
            }
        }
       
        private void deleteXml(string port)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(Application.StartupPath + "\\" + "Parameter.xml");
            XmlElement root = xDoc.DocumentElement;
            foreach (XmlElement xe in root.ChildNodes)
            {
                foreach (XmlElement xel in xe)
                {
                    if (xel.InnerText == filepath)
                    {
                        root.RemoveChild(xe);
                        xDoc.Save(Application.StartupPath + "\\" + "Parameter.xml");
                    }
                }
            }
        }
        private void addXml(string filepath)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(Application.StartupPath + "\\" + "Parameter.xml");
            XmlNode root = xDoc.SelectSingleNode("root");
            XmlElement xmlkey = xDoc.CreateElement("Equipment");          
          
            XmlElement xmlaikeypath = xDoc.CreateElement("filepath");
            xmlaikeypath.InnerText = filepath;
            xmlkey.AppendChild(xmlaikeypath);
           
            root.AppendChild(xmlkey);
            xDoc.Save(Application.StartupPath + "\\" + "Parameter.xml");
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog Folderimg = new FolderBrowserDialog();
            Folderimg.Description = "选择录像截图文件保存路径";
            if (Folderimg.ShowDialog() == DialogResult.OK)
            {
                txtfileurl.Text = Folderimg.SelectedPath;
            }
        }

        private void setform_FormClosing(object sender, FormClosingEventArgs e)
        {
            deleteXml(filepath);
            addXml(txtfileurl.Text);
            this.DialogResult = DialogResult.OK;
            //this.Close();
        }
    }
}
