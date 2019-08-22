using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.CartographyTools;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _190820SmoothLine
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region 定义变量
        IFeatureClass inFeatureClass = null;//待平滑的要素类
        string outPath = "";//输出的路径
        #endregion

        //窗体加载时初始化下拉框
        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }

        //选择待平滑的线要素
        private void button1_Click(object sender, EventArgs e)
        {
            string fullPath;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "shp数据(*.shp)|*.shp";
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fullPath = openFileDialog.FileName;
                inFeatureClass = LoadFeaClassFromShp(fullPath);
                textBox1.Text = fullPath;
            }
        }

        //选择输出位置
        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog DialogSaveShpPath = new SaveFileDialog();
            DialogSaveShpPath.Title = "保存要素类";
            DialogSaveShpPath.Filter = "shp文件(*.shp) | *.shp";
            DialogSaveShpPath.OverwritePrompt = false;
            DialogSaveShpPath.OverwritePrompt = false;
            if (DialogSaveShpPath.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(DialogSaveShpPath.FileName))
                {
                    MessageBox.Show("文件已存在！");
                    return;
                }
                outPath = DialogSaveShpPath.FileName;
                textBox2.Text = outPath;
            }
        }

        //根据选择的算法确定输入参数控件的enable属性
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //贝塞尔插值法需要输入容差参数
            if (comboBox1.SelectedIndex == 0)
            {
                textBox3.Enabled = false;
                comboBox2.Enabled = false;
            }
            else
            {
                textBox3.Enabled = true;
                comboBox2.Enabled = true;
            }
        }

        //取消
        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //确定按钮，执行平滑线操作
        private void button3_Click(object sender, EventArgs e)
        {
            //判断文本框下拉框是否为空
            if (textBox1.Text.Trim() == "" || textBox2.Text.Trim() == null ||
                comboBox1.Text.Trim() == "" || textBox3.Text.Trim() == "" ||
                comboBox2.Text.Trim() == "")
            {
                MessageBox.Show("请输入正确的信息!");
                return;
            }
            //判断输入的容差是否为
            if (!Regex.IsMatch(textBox3.Text.Trim(), "^-?\\d+$|^(-?\\d+)(\\.\\d+)?$") && (Convert.ToDouble(textBox3.Text.Trim()) >= 0))
            {
                MessageBox.Show("容差输入错误");
                return;
            }
            string pAlgorithm = "";
            double pTolerence;
            try
            {
                //根据选择的平滑算法来确定参数
                switch (comboBox1.Text.Trim())
                {
                    case "贝塞尔插值":
                        pAlgorithm = "BEZIER_INTERPOLATION";//该方法容差为0
                        SmoothContour(inFeatureClass, outPath, pAlgorithm, 0);
                        break;
                    case "指数核多项式逼近":
                        pAlgorithm = "PAEK";
                        pTolerence = Convert.ToDouble(textBox3.Text.Trim());
                        SmoothContour(inFeatureClass, outPath, pAlgorithm, pTolerence);
                        break;
                    default:
                        break;
                }
            }
            catch (COMException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        #region 封装方法
        /// <summary>
        /// 加载shp的方法
        /// </summary>
        /// <param name="fullpath">shp文件的路径</param>
        /// <returns>返回shp要素类IFeatureClass</returns>
        private IFeatureClass LoadFeaClassFromShp(string fullpath)
        {
            IWorkspaceFactory pWorkspaceFactory = new ShapefileWorkspaceFactory();
            IFeatureWorkspace pFeatureWorkspace;
            int index = fullpath.LastIndexOf("\\");
            string path = fullpath.Substring(0, index);
            string name = fullpath.Substring(index + 1);
            pFeatureWorkspace = pWorkspaceFactory.OpenFromFile(path, 0) as IFeatureWorkspace;
            return pFeatureWorkspace.OpenFeatureClass(name);
        }

        /// <summary>
        /// 平滑等高线
        /// </summary>
        /// <param name="pContourFeatureClass">输入的等高线要素类</param>
        /// <param name="outFeatureClass">输出的平滑等高线的位置</param>
        /// <param name="pAlgorithm">平滑算法，贝塞尔算法或指数核多项式逼近法</param>
        /// <param name="pTolerance">等高线平滑的容差</param>
        /// <returns>返回输出的等高线要素类</returns>
        private void SmoothContour(IFeatureClass pContourFeatureClass, string outPath, string pAlgorithm, double pTolerance)
        {
            Geoprocessor pGeoprocessor = new Geoprocessor();
            pGeoprocessor.OverwriteOutput = false;
            IFeatureLayer pFeatureLayer = new FeatureLayerClass();
            pFeatureLayer.FeatureClass = pContourFeatureClass;
            SmoothLine pSmoothLine = new SmoothLine(pContourFeatureClass, outPath, pAlgorithm, pTolerance);
            IGeoProcessorResult pResult = pGeoprocessor.Execute(pSmoothLine, null) as IGeoProcessorResult;
            //在调试的时候，针对工具箱工具用以下代码可以找到参数、环境设置等等问题
            //catch (Exception ex)
            //{
            //    object sev = null;
            //    MessageBox.Show(pGeoprocessor.GetMessages(ref sev));
            //}
        }
        #endregion
    }
}
