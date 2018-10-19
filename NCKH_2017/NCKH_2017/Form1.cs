using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
// Giao tiếp qua Serial
using System.IO;
using System.IO.Ports;
using System.Xml;

// Thêm ZedGraph
using ZedGraph;

namespace NCKH_2017
{
    public partial class Form1 : Form
    {
        #region Quan Ly Bien

        string SDatas = String.Empty; // Khai báo chuỗi để lưu dữ liệu cảm biến gửi qua Serial
        string SRealTime = String.Empty; // Khai báo chuỗi để lưu thời gian gửi qua Serial
        int status = 0; // Khai báo biến để xử lý sự kiện vẽ đồ thị
        double realtime = 0; //Khai báo biến thời gian để vẽ đồ thị
        double datas = 0; //Khai báo biến dữ liệu cảm biến để vẽ đồ thị

        #endregion

        #region Quan Ly Ham
        private void _KhoiTao()
        {
            try
            {
                comboBox1.DataSource = SerialPort.GetPortNames();
                if (comboBox1.Items.Count > 0)
                {
                    comboBox1.SelectedIndex = 0;
                }
            }
            catch (Exception)
            {


            }
        }

        private void Data_Listview()
        {
            if (status == 0)
                return;
            else
            {
                ListViewItem item = new ListViewItem(realtime.ToString()); // Gán biến realtime vào cột đầu tiên của ListView
                item.SubItems.Add(datas.ToString());
                listView1.Items.Add(item); // Gán biến datas vào cột tiếp theo của ListView
                // Không nên gán string SDatas vì khi xuất dữ liệu sang Excel sẽ là dạng string, không thực hiện các phép toán được
                listView1.Items[listView1.Items.Count - 1].EnsureVisible(); // Hiện thị dòng được gán gần nhất ở ListView,tức là mình cuộn ListView theo dữ liệu gần nhất đó
            }
        }
        // Vẽ đồ thị
        private void Draw()
        {

            if (zedGraphControl1.GraphPane.CurveList.Count <= 0)
                return;

            LineItem curve = zedGraphControl1.GraphPane.CurveList[0] as LineItem;

            if (curve == null)
                return;

            IPointListEdit list = curve.Points as IPointListEdit;

            if (list == null)
                return;

            list.Add(realtime, datas); // Thêm điểm trên đồ thị

            Scale xScale = zedGraphControl1.GraphPane.XAxis.Scale;
            Scale yScale = zedGraphControl1.GraphPane.YAxis.Scale;

            // Tự động Scale theo trục x
            if (realtime > xScale.Max - xScale.MajorStep)
            {
                xScale.Max = realtime + xScale.MajorStep;
                xScale.Min = xScale.Max - 30;
            }

            // Tự động Scale theo trục y
            if (datas > yScale.Max - yScale.MajorStep)
            {
                yScale.Max = datas + yScale.MajorStep;
            }
            else if (datas < yScale.Min + yScale.MajorStep)
            {
                yScale.Min = datas - yScale.MajorStep;
            }

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
        }
        // Xóa đồ thị, với ZedGraph thì phải khai báo lại như ở hàm Form1_Load, nếu không sẽ không hiển thị
        private void ClearZedGraph()
        {
            zedGraphControl1.GraphPane.CurveList.Clear(); // Xóa đường
            zedGraphControl1.GraphPane.GraphObjList.Clear(); // Xóa đối tượng

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();

            GraphPane myPane = zedGraphControl1.GraphPane;
            myPane.Title.Text = "Đồ thị điện dung theo thời gian";
            myPane.XAxis.Title.Text = "Thời gian (s)";
            myPane.YAxis.Title.Text = "Điện dung";

            RollingPointPairList list = new RollingPointPairList(60000);
            LineItem curve = myPane.AddCurve("Dữ liệu", list, Color.Red, SymbolType.None);

            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = 30;
            myPane.XAxis.Scale.MinorStep = 1;
            myPane.XAxis.Scale.MajorStep = 5;
            myPane.YAxis.Scale.Min = -100;
            myPane.YAxis.Scale.Max = 100;

            zedGraphControl1.AxisChange();
        }

        // Hàm xóa dữ liệu
        private void ResetValue()
        {
            realtime = 0;
            datas = 0;
            SDatas = String.Empty;
            SRealTime = String.Empty;
            status = 0; // Chuyển status về 0
        }

        #endregion

        #region Quan Ly Form
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _KhoiTao();

            comboBox1.Enabled = true;
            // Khởi tạo ZedGraph
            GraphPane myPane = zedGraphControl1.GraphPane;
            myPane.Title.Text = "Đồ thị điện dung theo thời gian";
            myPane.XAxis.Title.Text = "Thời gian (s)";
            myPane.YAxis.Title.Text = "Điện dung";

            RollingPointPairList list = new RollingPointPairList(60000);
            LineItem curve = myPane.AddCurve("Dữ liệu", list, Color.Red, SymbolType.None);

            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = 30;
            myPane.XAxis.Scale.MinorStep = 1;
            myPane.XAxis.Scale.MajorStep = 5;
            myPane.YAxis.Scale.Min = -100;
            myPane.YAxis.Scale.Max = 100;

            myPane.AxisChange();

        }
        #endregion
        #region Xu ly su kien
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string[] arrList = serialPort1.ReadLine().Split('|'); // Đọc một dòng của Serial, cắt chuỗi khi gặp ký tự gạch đứng
                SRealTime = arrList[0]; // Chuỗi đầu tiên lưu vào SRealTime
                SDatas = arrList[1]; // Chuỗi thứ hai lưu vào SDatas
                double.TryParse(SDatas, out datas); // Chuyển đổi sang kiểu double
                double.TryParse(SRealTime, out realtime);
                datas = datas / 100;
                realtime = realtime / 1000.0; // Đối ms sang s
                status = 1; // Bắt sự kiện xử lý xong chuỗi, đổi starus về 1 để hiển thị dữ liệu trong ListView và vẽ đồ thị
            }
            catch
            {
                return;
            }
        }
        private void btConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox1.Text.Trim().Length == 0)
                {
                    MessageBox.Show("Vui lòng chọn cổng COM kết nối.");
                    return;
                }
                
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write("2"); //Gửi ký tự "2" qua Serial, tương ứng với state = 2
                    serialPort1.Close();
                    progressBar1.Value = 0;
                    btConnect.Text = "Kết nối";
                    btExit.Enabled = true;
                    btRun.Enabled = false;
                    btPause.Enabled = false;
                    btClear.Enabled = false;
                    
                }
                else
                {
                    serialPort1.PortName = comboBox1.Text; // Lấy cổng COM
                    serialPort1.BaudRate = 9600; // Baudrate là 9600, trùng với baudrate của Arduino
                    
                   
                    try
                    {
                        serialPort1.Open();
                        progressBar1.Value = 100;
                        btConnect.Text = "Ngắt kết nối";
                        btExit.Enabled = true;
                        btRun.Enabled = true;
                        btPause.Enabled = true;
                        btClear.Enabled = true;
                        comboBox1.Enabled = false;
                        
                    }
                    catch
                    {
                        MessageBox.Show("Không thể mở cổng" + serialPort1.PortName, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception)
            { 
            
            }
        }
        private void btRun_Click(object sender, EventArgs e)
        {
            
            if (serialPort1.IsOpen)
            {
                
                btPause.Enabled = true;
                btClear.Enabled = true;
                btRun.Enabled = false;
                serialPort1.Write("1"); //Gửi ký tự "1" qua Serial, chạy hàm tạo Random ở Arduino
            }
            else
                MessageBox.Show("Bạn không thể chạy khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void btPause_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                btRun.Enabled = true;
                serialPort1.Write("0"); //Gửi ký tự "0" qua Serial, Dừng Arduino
            }
            else
                MessageBox.Show("Bạn không thể dừng khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void btClear_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen) {
            DialogResult traloi;
            traloi = MessageBox.Show("Bạn có chắc muốn xóa?", "Xóa dữ liệu", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (traloi == DialogResult.OK) {
                btRun.Enabled = true;
                if (serialPort1.IsOpen) {
                    serialPort1.Write("2"); //Gửi ký tự "2" qua Serial
                    listView1.Items.Clear(); // Xóa listview
 
                    //Xóa đường trong đồ thị
                    ClearZedGraph();
 
                    //Xóa dữ liệu trong Form
                    ResetValue();
                }
                else
                    MessageBox.Show("Bạn không thể dừng khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
            MessageBox.Show("Bạn không thể xóa khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
        private void btExit_Click_1(object sender, EventArgs e)
        {
            DialogResult traloi;
            traloi = MessageBox.Show("Bạn có chắc muốn thoát?", "Thoát", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (traloi == DialogResult.OK)
            {
                Application.Exit(); // Đóng ứng dụng
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
                Draw();
                Data_Listview();
                status = 0;
        }
        #endregion

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
