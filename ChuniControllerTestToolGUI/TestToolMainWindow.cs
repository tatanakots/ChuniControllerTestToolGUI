using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChuniControllerTestToolGUI
{
    public partial class TestToolMainWindow : Form
    {
        private const string DEVICE_VID = "VID_AFF1";
        private const string DEVICE_PID = "PID_52A4";
        private bool isDeviceConnected = false;
        private Thread? readThread = null; // 用于后台读取数据的线程
        private int THRESHOLD = 140;
        private bool isTouchChangeColor = false;
        // 定义 93 字节的 LED 颜色数据（从右往左 BRG）
        private byte[] rgbdata = Enumerable.Repeat((byte)0, 93).ToArray();
        // private byte[] rgbdata_air = Enumerable.Repeat((byte)0, 3).ToArray();
        private byte[] rgbdata_air = [0,0,0]; // brg
        private byte[] rgbdatabackup = Enumerable.Repeat((byte)0, 93).ToArray();
        private byte[] touchedColor = Enumerable.Repeat((byte)255, 3).ToArray();
        bool[] hasBackup = new bool[16];  // 每个色块是否已备份
        bool airLedNeedUpdate = true;

        public TestToolMainWindow()
        {
            InitializeComponent();
            UpdateUIDisplay(null, 0);
            RegisterSliderLightButtons();
            RegisterAirLightButtons();
            linkLabel1.Links.Add(0, 10, "https://github.com/tatanakots/ChuniControllerTestToolGUI");
        }

        // 获取模块句柄（使用 ANSI 字符集）
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // 获取模块内导出符号的地址
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        // 遍历所有 sliderLight 按钮，并注册同一个点击事件处理器
        private void RegisterSliderLightButtons()
        {
            // 假设 sliderLight1 到 sliderLight31 都在当前窗体中
            // 你可以手动放入数组，或者根据控件名称查找
            Button[] sliderLights = new Button[]
            {
                sliderLight1, sliderLight2, sliderLight3, sliderLight4, sliderLight5,
                sliderLight6, sliderLight7, sliderLight8, sliderLight9, sliderLight10,
                sliderLight11, sliderLight12, sliderLight13, sliderLight14, sliderLight15,
                sliderLight16, sliderLight17, sliderLight18, sliderLight19, sliderLight20,
                sliderLight21, sliderLight22, sliderLight23, sliderLight24, sliderLight25,
                sliderLight26, sliderLight27, sliderLight28, sliderLight29, sliderLight30,
                sliderLight31
            };

            // 为每个按钮设置 Tag（例如，Tag = 30 对应 sliderLight1，Tag = 0 对应 sliderLight31）
            // 这里假设按钮数组顺序是 sliderLight1, sliderLight2, …, sliderLight31，
            // 那么你可以这样赋值 Tag: Tag = (31 - index - 1)
            for (int i = 0; i < sliderLights.Length; i++)
            {
                sliderLights[i].Tag = i; // Tag: 30,29,...,0
                sliderLights[i].Click += SliderLightButton_Click;
            }
        }

        // 所有 sliderLight 按钮的点击事件处理器
        private void SliderLightButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is int ledIndex)
            {
                using (ColorDialog cd = new ColorDialog())
                {
                    // 弹出调色盘
                    if (cd.ShowDialog() == DialogResult.OK)
                    {
                        Color chosen = cd.Color;
                        // 将选择的颜色转换为符合 rgbdata 格式的字节数据（BRG顺序）
                        // Color 的属性顺序为 R, G, B
                        byte blue = chosen.B;
                        byte red = chosen.R;
                        byte green = chosen.G;

                        // 根据映射关系，计算该 LED 在 rgbdata 数组中的偏移
                        // 假设 rgbdata 中 LED 数据从右往左排列，
                        // 且每个 LED 占 3 字节，最右边的 LED 对应 sliderLight1，其 Tag 为 30
                        // 对应 rgbdata 的 LED index = ledIndex，即偏移 = ledIndex * 3，
                        // 但由于我们之前定义 Tag = (31 - index - 1)，此时直接使用 ledIndex 即可
                        // 如果你想更直观，也可以先计算逻辑 LED 编号：
                        int offset = ledIndex * 3;  // LED 占3字节

                        // 按照数组原有的格式：顺序为 BRG
                        rgbdata[offset] = blue;
                        rgbdata[offset + 1] = red;
                        rgbdata[offset + 2] = green;

                        // 更新该按钮的背景颜色，以便直观显示选择的颜色
                        btn.BackColor = chosen;
                    }
                }
            }
        }

        private void RegisterAirLightButtons()
        {
            // 假设 sliderLight1 到 sliderLight31 都在当前窗体中
            // 你可以手动放入数组，或者根据控件名称查找
            Button[] airLights = new Button[]
            {
                airLight1, airLight2, airLight3, airLight4, airLight5, airLight6,
                airLight7, airLight8, airLight9, airLight10, airLight11, airLight12
            };

            // 为每个按钮设置 Tag（例如，Tag = 30 对应 sliderLight1，Tag = 0 对应 sliderLight31）
            // 这里假设按钮数组顺序是 sliderLight1, sliderLight2, …, sliderLight31，
            // 那么你可以这样赋值 Tag: Tag = (31 - index - 1)
            for (int i = 0; i < airLights.Length; i++)
            {
                airLights[i].Tag = i; // Tag: 30,29,...,0
                airLights[i].Click += AirLightButton_Click;
            }
        }

        private void AirLightButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                // 取当前三通道状态（0/1）
                bool curBlue = rgbdata_air[0] != 0;
                bool curGreen = rgbdata_air[1] != 0;
                bool curRed = rgbdata_air[2] != 0;

                using (var dlg = new AirColorDialog(curBlue, curGreen, curRed))
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        // 更新数组（BGR 顺序）
                        rgbdata_air[0] = dlg.BlueEnabled ? (byte)1 : (byte)0;
                        rgbdata_air[1] = dlg.RedEnabled ? (byte)1 : (byte)0;
                        rgbdata_air[2] = dlg.GreenEnabled ? (byte)1 : (byte)0;

                        // 按钮底色预览
                        btn.BackColor = dlg.SelectedColor;

                        Color SelectedAirColor = Color.FromArgb(
                            rgbdata_air[1] != 0 ? 255 : 0,
                            rgbdata_air[2] != 0 ? 255 : 0,
                            rgbdata_air[0] != 0 ? 255 : 0
                        );
                        Button[] airLights = new Button[]
                        {
                            airLight1, airLight2, airLight3, airLight4, airLight5, airLight6,
                            airLight7, airLight8, airLight9, airLight10, airLight11, airLight12
                        };
                        for (int i = 0; i < airLights.Length; i++)
                        {
                            airLights[i].BackColor = SelectedAirColor;
                        }
                        //Chuniio.slider_send_air_leds(rgbdata_air);
                        airLedNeedUpdate = true;
                    }
                }
            }
        }

        private void AutoConnectButton_Click(object sender, EventArgs e)
        {
            // 调用 DLL 函数，获取串口名称的指针
            IntPtr ptr = Chuniio.GetSerialPortByVidPid(DEVICE_VID, DEVICE_PID);
            // 将指针转换为 ANSI 字符串
            string? portString = Marshal.PtrToStringAnsi(ptr);

            // 假设返回的串口名称至少6个字符
            string? comPort = portString;

            // 如果首字节为 0x48（ASCII 'H'），则使用默认串口 "COM1"
            if ((!string.IsNullOrEmpty(comPort) && comPort[0] == (char)0x48) || string.IsNullOrEmpty(comPort))
            {
                if (MessageBox.Show("无法自动获取设备串口号，请检查您的设备是否正确连接或是否使用了自定义的VID和PID，将尝试使用默认串口COM1进行连接！", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                {
                    return;
                }
                comPort = "COM1";
            }

            // 可将最终的串口名称显示到界面上，或记录到变量中
            //MessageBox.Show("使用的串口：" + comPort);
            ConnectedPortLabel.Text = comPort;

            // —— 在这里增加 COM 号数值判断 —— 
            if (!string.IsNullOrEmpty(comPort) &&
                comPort.StartsWith("COM", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(comPort.Substring(3), out int portNum) &&
                portNum >= 10)
            {
                MessageBox.Show(
                    "当前COM端口大于等于10，可能会引起程序意外错误。\n只是测试程序还没有修复这个问题，不影响连接游戏。",
                    "警告 - COM端口号过大",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            // 获取 DLL 模块句柄
            IntPtr hModule = GetModuleHandle("chuniio_affine.dll");
            if (hModule == IntPtr.Zero)
            {
                MessageBox.Show("无法获取 chuniio DLL 模块句柄！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 获取全局变量 comPort 的地址（注意符号名称区分大小写）
            IntPtr comPortAddr = GetProcAddress(hModule, "comPort");
            if (comPortAddr == IntPtr.Zero)
            {
                MessageBox.Show("无法获取 chuniio 全局变量 comPort 的地址！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 将 comPortValue 转换为 ANSI 字节数组
            // 根据原代码，C 中 comPort 定义为 char comPort[13]，但在 open_port() 调用中只复制前6个字节
            byte[] comPortBytes = new byte[6];
            byte[] tempBytes = Encoding.ASCII.GetBytes(comPort);
            int copyLen = Math.Min(tempBytes.Length, 6);
            Array.Copy(tempBytes, comPortBytes, copyLen);
            // 剩余字节如果不足6个则保持为0

            // 将字节数组写入 DLL 内部的全局变量 comPort 的内存区域
            Marshal.Copy(comPortBytes, 0, comPortAddr, 6);

            // 调用 open_port() 打开串口设备
            bool openSuccess = Chuniio.open_port();
            if (!openSuccess)
            {
                MessageBox.Show("设备连接失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DisconnectButton_Click(this, EventArgs.Empty);
            }
            else
            {
                //MessageBox.Show("串口已打开！");
                AutoConnectButton.Enabled = false;
                DisconnectButton.Enabled = true;
                isDeviceConnected = true;
                // 开启后台线程循环查询设备状态
                readThread = new Thread(ReadDeviceLoop);
                readThread.IsBackground = true;
                readThread.Start();
            }
        }

        private unsafe void UpdateUIDisplay(int[,]? data, int airdata)
        {
            if (data == null)
            {
                data = new int[2, 16];
            }
            // 定义奇数和偶数的 Label 控件数组
            Button[] oddLabels = new Button[]
            {
                sliderInfo31, sliderInfo29, sliderInfo27, sliderInfo25,
                sliderInfo23, sliderInfo21, sliderInfo19, sliderInfo17,
                sliderInfo15, sliderInfo13, sliderInfo11, sliderInfo9,
                sliderInfo7,  sliderInfo5,  sliderInfo3,  sliderInfo1
            };

            Button[] evenLabels = new Button[]
            {
                sliderInfo32, sliderInfo30, sliderInfo28, sliderInfo26,
                sliderInfo24, sliderInfo22, sliderInfo20, sliderInfo18,
                sliderInfo16, sliderInfo14, sliderInfo12, sliderInfo10,
                sliderInfo8,  sliderInfo6,  sliderInfo4,  sliderInfo2
            };

            // 循环赋值：数组 data 为 2行16列，data[0,i] 对应奇数标签（31,29,...,1），data[1,i] 对应偶数标签（32,30,...,2）
            for (int i = 0; i < 16; i++)
            {
                // 奇数标签：例如 sliderInfo31, sliderInfo29, ...
                oddLabels[i].Text = $"{data[0, i]}";
                oddLabels[i].BackColor = data[0, i] >= THRESHOLD ? Color.Pink : Color.White;

                // 偶数标签：例如 sliderInfo32, sliderInfo30, ...
                evenLabels[i].Text = $"{data[1, i]}";
                evenLabels[i].BackColor = data[1, i] >= THRESHOLD ? Color.Pink : Color.White;

                int offset = (15 - i) * 2;  // 根据原代码计算出色块在 rgbdata 中的位置

                // 当任一数据满足阈值时且触摸改变颜色的标志为 true
                if ((data[0, i] >= THRESHOLD || data[1, i] >= THRESHOLD) && isTouchChangeColor)
                {
                    // 如果该色块还未备份，则备份一次
                    if (!hasBackup[i])
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            rgbdatabackup[(offset * 3) + j] = rgbdata[(offset * 3) + j];
                        }
                        hasBackup[i] = true;
                    }
                    for (int j = 0;j < 3; j++)
                    {
                        rgbdata[(offset * 3) + j] = touchedColor[j];
                    }
                }
                // 当数据不满足阈值时，并且该色块之前曾备份过颜色
                else if (isTouchChangeColor && hasBackup[i])
                {
                    for (int j = 0; j < 3; j++)
                    {
                        rgbdata[(offset * 3) + j] = rgbdatabackup[(offset * 3) + j];
                    }
                    hasBackup[i] = false;  // 清除备份标记
                }
            }

            // 将 airdata 转换为 6 位二进制字符串（左侧为最高位）
            string binaryAir = Convert.ToString(airdata, 2).PadLeft(6, '0');

            // 如果你已经在窗体上放置了 airInfo1 到 airInfo6 的 Label 控件，可以将它们放入数组
            Button[] airInfoLabels = new Button[]
            {
                airInfo1, airInfo2, airInfo3, airInfo4, airInfo5, airInfo6
            };

            for (int i = 0; i < 6; i++)
            {
                // 获取当前位字符
                char bit = binaryAir[5 - i];
                airInfoLabels[i].Text = bit.ToString();
                airInfoLabels[i].BackColor = (bit == '1') ? Color.Pink : Color.White;
            }

            // 将 31 个控件放入数组，确保数组顺序与 UI 上显示的编号一致：
            // sliderLight1 对应 1 号LED，sliderLight31 对应 31 号LED
            Button[] sliderLights = new Button[]
            {
                sliderLight1, sliderLight2, sliderLight3, sliderLight4, sliderLight5,
                sliderLight6, sliderLight7, sliderLight8, sliderLight9, sliderLight10,
                sliderLight11, sliderLight12, sliderLight13, sliderLight14, sliderLight15,
                sliderLight16, sliderLight17, sliderLight18, sliderLight19, sliderLight20,
                sliderLight21, sliderLight22, sliderLight23, sliderLight24, sliderLight25,
                sliderLight26, sliderLight27, sliderLight28, sliderLight29, sliderLight30,
                sliderLight31
            };

            // 由于数组 rgbdata 共93字节，每个LED占3字节，共31个LED
            // 数组是“从右往左”排列，所以 sliderLight1 对应的 LED数据在 rgbdata 的最后3字节
            for (int i = 0; i < 31; i++)
            {
                // 计算数据在 rgbdata 中的偏移：sliderLight1 对应索引 30，sliderLight2 对应索引 29，以此类推
                int offset = i * 3;
                // 按 BRG 顺序取出颜色分量
                byte blue = rgbdata[offset];
                byte red = rgbdata[offset + 1];
                byte green = rgbdata[offset + 2];
                // 构造颜色（Color.FromArgb 的参数顺序为 R, G, B）
                Color color = Color.FromArgb(red, green, blue);
                sliderLights[i].BackColor = color;
            }
            byte touchedblue = touchedColor[0];
            byte touchedred = touchedColor[1];
            byte touchedgreen = touchedColor[2];
            Color touchedcolor = Color.FromArgb(touchedred, touchedgreen, touchedblue);
            TouchedColorButton.BackColor = touchedcolor;

            Color SelectedAirColor = Color.FromArgb(
                rgbdata_air[1] != 0 ? 255 : 0,
                rgbdata_air[2] != 0 ? 255 : 0,
                rgbdata_air[0] != 0 ? 255 : 0
            );
            Button[] airLights = new Button[]
            {
                airLight1, airLight2, airLight3, airLight4, airLight5, airLight6,
                airLight7, airLight8, airLight9, airLight10, airLight11, airLight12
            };
            for (int i = 0; i < airLights.Length; i++)
            {
                airLights[i].BackColor = SelectedAirColor;
            }
        }

        private unsafe void ReadDeviceLoop()
        {
            // 启动扫描命令
            Chuniio.slider_start_air_scan();
            Chuniio.slider_start_scan();

            // 初始化 data 数组：2行16列
            int[,] data = new int[2, 16];
            int airdata = 0;



            // 发送 LED 数据一次
            Chuniio.slider_send_leds(rgbdata);
            Chuniio.slider_send_air_leds(rgbdata_air);
            Thread.Sleep(50);  // 等待50毫秒，防止因为更新速度太快设备反应不过来
            // 初始化 response
            slider_packet_t response = new slider_packet_t();
            Chuniio.package_init(ref response);
            // airLedNeedUpdate = true; // 我真的服了为什么它不会初始化置黑 // 好了现在解决了，原来是被吃了
            while (isDeviceConnected)
            {
                byte cmd = Chuniio.serial_read_cmd(ref response);
                switch (cmd)
                {
                    case (byte)SliderCmd.SLIDER_CMD_AUTO_SCAN:
                        for (int j = 0; j < 16; j++)
                        {
                            data[0, 15 - j] = response.pressure[2 * j];
                            data[1, 15 - j] = response.pressure[2 * j + 1];
                        }
                        //for (int j = 0; j < 93; j++)
                        //{
                        //    rgb[j] = response.leds[j];
                        //}
                        airdata = response.air_status;
                        // 重置 response 数据
                        Chuniio.package_init(ref response);
                        this.BeginInvoke(new Action(() =>
                        {
                            // 更新UI
                            UpdateUIDisplay(data, airdata);
                        }));
                        break;
                    case (byte)SliderCmd.SLIDER_CMD_AUTO_AIR:
                        Chuniio.package_init(ref response);
                        break;
                    case 0xff: // 0xff 表示错误（连接失败）
                        // TODO: 错误处理
                        break;
                    default:
                        break;
                }
                Chuniio.slider_send_leds(rgbdata);
                if (airLedNeedUpdate)
                {
                    // 天键LED更新太快会影响地键LED的灵敏性
                    Chuniio.slider_send_air_leds(rgbdata_air);
                    airLedNeedUpdate = false;
                    Thread.Sleep(50);  // 等待50毫秒，防止因为更新速度太快设备反应不过来
                }
            }
        }

        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            isDeviceConnected = false;
            if (readThread != null && readThread.IsAlive)
            {
                // 等待线程结束，最多等待1秒
                readThread.Join(1000);
            }
            Chuniio.close_port();
            AutoConnectButton.Enabled = true;
            DisconnectButton.Enabled = false;
            ConnectedPortLabel.Text = "未连接";
        }

        private void ApplyThresButton_Click(object sender, EventArgs e)
        {
            THRESHOLD = (int)ThresNumericUpDown.Value;
        }

        private void TestToolMainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisconnectButton_Click(this, EventArgs.Empty);
        }

        private void AboutButton_Click(object sender, EventArgs e)
        {
            // 获取当前正在执行的程序集
            Assembly assembly = Assembly.GetExecutingAssembly();
            // 通过程序集位置获取文件版本信息
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            ushort chuniioapiVersion = Chuniio.chuni_io_get_api_version();
            string chuniioVersion = "0x" + chuniioapiVersion.ToString("X4");
            MessageBox.Show($"中二节奏控制器测试工具（图形界面版）\n\n程序版本：Ver. {fvi.FileVersion}\nChuniio版本：{chuniioVersion}\n\n软件作者：Tatanako\n硬件作者：Qinh\n特别鸣谢：Soda（我的硬件生产商）\n\n本软件基于\nhttps://github.com/QHPaeek/Affine_IO/blob/master/chuniio/test.c\n编写完成，感谢Qinh开源~\n\n本软件完全免费，代码开源于GitHub：\nhttps://github.com/tatanakots/ChuniControllerTestToolGUI", "关于本软件", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // 获取链接数据，这里是 URL 字符串
            string url = e!.Link!.LinkData!.ToString() == null ? e!.Link!.LinkData!.ToString()! : "https://github.com/tatanakots/ChuniControllerTestToolGUI";
            var processStartInfo = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(processStartInfo);
        }

        private void changeGroundLEDButton_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                // 弹出调色盘
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    Color chosen = cd.Color;
                    // 将选择的颜色转换为符合 rgbdata 格式的字节数据（BRG顺序）
                    // Color 的属性顺序为 R, G, B
                    byte blue = chosen.B;
                    byte red = chosen.R;
                    byte green = chosen.G;

                    rgbdata = Enumerable.Repeat(new byte[] { blue, red, green }, 31)
                             .SelectMany(x => x)
                             .ToArray();
                    rgbdatabackup = Enumerable.Repeat(new byte[] { blue, red, green }, 31)
                             .SelectMany(x => x)
                             .ToArray();
                }
            }
        }

        private void isTouchChangeColorCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            isTouchChangeColor = isTouchChangeColorCheckBox.Checked;
        }

        private void TouchedColorButton_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                // 弹出调色盘
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    Color chosen = cd.Color;
                    // 将选择的颜色转换为符合 rgbdata 格式的字节数据（BRG顺序）
                    // Color 的属性顺序为 R, G, B
                    byte blue = chosen.B;
                    byte red = chosen.R;
                    byte green = chosen.G;

                    // 按照数组原有的格式：顺序为 BRG
                    touchedColor[0] = blue;
                    touchedColor[1] = red;
                    touchedColor[2] = green;

                    // 更新该按钮的背景颜色，以便直观显示选择的颜色
                    TouchedColorButton.BackColor = chosen;
                }
            }
        }
    }
}
