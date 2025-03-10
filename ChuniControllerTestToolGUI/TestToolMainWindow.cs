using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChuniControllerTestToolGUI
{
    public partial class TestToolMainWindow : Form
    {
        private const string DEVICE_VID = "VID_AFF1";
        private const string DEVICE_PID = "PID_52A4";
        private bool isDeviceConnected = false;
        private Thread? readThread = null; // ���ں�̨��ȡ���ݵ��߳�
        private int THRESHOLD = 140;
        // ���� 93 �ֽڵ� LED ��ɫ���ݣ��������� BRG��
        private byte[] rgbdata = Enumerable.Repeat((byte)0, 93).ToArray();

        public TestToolMainWindow()
        {
            InitializeComponent();
            UpdateUIDisplay(null, 0);
            RegisterSliderLightButtons();
            linkLabel1.Links.Add(0, 10, "https://github.com/tatanakots/ChuniControllerTestToolGUI");
        }

        // ��ȡģ������ʹ�� ANSI �ַ�����
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // ��ȡģ���ڵ������ŵĵ�ַ
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        // �������� sliderLight ��ť����ע��ͬһ������¼�������
        private void RegisterSliderLightButtons()
        {
            // ���� sliderLight1 �� sliderLight31 ���ڵ�ǰ������
            // ������ֶ��������飬���߸��ݿؼ����Ʋ���
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

            // Ϊÿ����ť���� Tag�����磬Tag = 30 ��Ӧ sliderLight1��Tag = 0 ��Ӧ sliderLight31��
            // ������谴ť����˳���� sliderLight1, sliderLight2, ��, sliderLight31��
            // ��ô�����������ֵ Tag: Tag = (31 - index - 1)
            for (int i = 0; i < sliderLights.Length; i++)
            {
                sliderLights[i].Tag = i; // Tag: 30,29,...,0
                sliderLights[i].Click += SliderLightButton_Click;
            }
        }

        // ���� sliderLight ��ť�ĵ���¼�������
        private void SliderLightButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is int ledIndex)
            {
                using (ColorDialog cd = new ColorDialog())
                {
                    // ������ɫ��
                    if (cd.ShowDialog() == DialogResult.OK)
                    {
                        Color chosen = cd.Color;
                        // ��ѡ�����ɫת��Ϊ���� rgbdata ��ʽ���ֽ����ݣ�BRG˳��
                        // Color ������˳��Ϊ R, G, B
                        byte blue = chosen.B;
                        byte red = chosen.R;
                        byte green = chosen.G;

                        // ����ӳ���ϵ������� LED �� rgbdata �����е�ƫ��
                        // ���� rgbdata �� LED ���ݴ����������У�
                        // ��ÿ�� LED ռ 3 �ֽڣ����ұߵ� LED ��Ӧ sliderLight1���� Tag Ϊ 30
                        // ��Ӧ rgbdata �� LED index = ledIndex����ƫ�� = ledIndex * 3��
                        // ����������֮ǰ���� Tag = (31 - index - 1)����ʱֱ��ʹ�� ledIndex ����
                        // ��������ֱ�ۣ�Ҳ�����ȼ����߼� LED ��ţ�
                        int offset = ledIndex * 3;  // LED ռ3�ֽ�

                        // ��������ԭ�еĸ�ʽ��˳��Ϊ BRG
                        rgbdata[offset] = blue;
                        rgbdata[offset + 1] = red;
                        rgbdata[offset + 2] = green;

                        // ���¸ð�ť�ı�����ɫ���Ա�ֱ����ʾѡ�����ɫ
                        btn.BackColor = chosen;

                        // �����Ҫ�Ļ���Ҳ���Ե��� UpdateSliderLights() ��ˢ������ LED ��ʾ
                        // UpdateSliderLights();
                    }
                }
            }
        }

        private void AutoConnectButton_Click(object sender, EventArgs e)
        {
            // ���� DLL ��������ȡ�������Ƶ�ָ��
            IntPtr ptr = Chuniio.GetSerialPortByVidPid(DEVICE_VID, DEVICE_PID);
            // ��ָ��ת��Ϊ ANSI �ַ���
            string? portString = Marshal.PtrToStringAnsi(ptr);

            // ���践�صĴ�����������6���ַ�
            string? comPort = portString;

            // ������ֽ�Ϊ 0x48��ASCII 'H'������ʹ��Ĭ�ϴ��� "COM1"
            if ((!string.IsNullOrEmpty(comPort) && comPort[0] == (char)0x48) || string.IsNullOrEmpty(comPort))
            {
                if (MessageBox.Show("�޷��Զ���ȡ�豸���ںţ����������豸�Ƿ���ȷ���ӻ��Ƿ�ʹ�����Զ����VID��PID��������ʹ��Ĭ�ϴ���COM1�������ӣ�", "����", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                {
                    return;
                }
                comPort = "COM1";
            }

            // �ɽ����յĴ���������ʾ�������ϣ����¼��������
            //MessageBox.Show("ʹ�õĴ��ڣ�" + comPort);
            ConnectedPortLabel.Text = comPort;

            // ��ȡ DLL ģ����
            IntPtr hModule = GetModuleHandle("chuniio_affine.dll");
            if (hModule == IntPtr.Zero)
            {
                MessageBox.Show("�޷���ȡ chuniio DLL ģ������", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ��ȡȫ�ֱ��� comPort �ĵ�ַ��ע������������ִ�Сд��
            IntPtr comPortAddr = GetProcAddress(hModule, "comPort");
            if (comPortAddr == IntPtr.Zero)
            {
                MessageBox.Show("�޷���ȡ chuniio ȫ�ֱ��� comPort �ĵ�ַ��", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // �� comPortValue ת��Ϊ ANSI �ֽ�����
            // ����ԭ���룬C �� comPort ����Ϊ char comPort[13]������ open_port() ������ֻ����ǰ6���ֽ�
            byte[] comPortBytes = new byte[6];
            byte[] tempBytes = Encoding.ASCII.GetBytes(comPort);
            int copyLen = Math.Min(tempBytes.Length, 6);
            Array.Copy(tempBytes, comPortBytes, copyLen);
            // ʣ���ֽ��������6���򱣳�Ϊ0

            // ���ֽ�����д�� DLL �ڲ���ȫ�ֱ��� comPort ���ڴ�����
            Marshal.Copy(comPortBytes, 0, comPortAddr, 6);

            // ���� open_port() �򿪴����豸
            bool openSuccess = Chuniio.open_port();
            if (!openSuccess)
            {
                MessageBox.Show("�豸����ʧ�ܣ�", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DisconnectButton_Click(this, EventArgs.Empty);
            }
            else
            {
                //MessageBox.Show("�����Ѵ򿪣�");
                AutoConnectButton.Enabled = false;
                DisconnectButton.Enabled = true;
                isDeviceConnected = true;
                // ������̨�߳�ѭ����ѯ�豸״̬
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
            // ����������ż���� Label �ؼ�����
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

            // ѭ����ֵ������ data Ϊ 2��16�У�data[0,i] ��Ӧ������ǩ��31,29,...,1����data[1,i] ��Ӧż����ǩ��32,30,...,2��
            for (int i = 0; i < 16; i++)
            {
                // ������ǩ������ sliderInfo31, sliderInfo29, ...
                oddLabels[i].Text = $"{data[0, i]}";
                oddLabels[i].BackColor = data[0, i] >= THRESHOLD ? Color.Pink : Color.White;

                // ż����ǩ������ sliderInfo32, sliderInfo30, ...
                evenLabels[i].Text = $"{data[1, i]}";
                evenLabels[i].BackColor = data[1, i] >= THRESHOLD ? Color.Pink : Color.White;
            }

            // �� airdata ת��Ϊ 6 λ�������ַ��������Ϊ���λ��
            string binaryAir = Convert.ToString(airdata, 2).PadLeft(6, '0');

            // ������Ѿ��ڴ����Ϸ����� airInfo1 �� airInfo6 �� Label �ؼ������Խ����Ƿ�������
            Button[] airInfoLabels = new Button[]
            {
                airInfo1, airInfo2, airInfo3, airInfo4, airInfo5, airInfo6
            };

            for (int i = 0; i < 6; i++)
            {
                // ��ȡ��ǰλ�ַ�
                char bit = binaryAir[5 - i];
                airInfoLabels[i].Text = bit.ToString();
                airInfoLabels[i].BackColor = (bit == '1') ? Color.Pink : Color.White;
            }

            // �� 31 ���ؼ��������飬ȷ������˳���� UI ����ʾ�ı��һ�£�
            // sliderLight1 ��Ӧ 1 ��LED��sliderLight31 ��Ӧ 31 ��LED
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

            // �������� rgbdata ��93�ֽڣ�ÿ��LEDռ3�ֽڣ���31��LED
            // �����ǡ������������У����� sliderLight1 ��Ӧ�� LED������ rgbdata �����3�ֽ�
            for (int i = 0; i < 31; i++)
            {
                // ���������� rgbdata �е�ƫ�ƣ�sliderLight1 ��Ӧ���� 30��sliderLight2 ��Ӧ���� 29���Դ�����
                int offset = i * 3;
                // �� BRG ˳��ȡ����ɫ����
                byte blue = rgbdata[offset];
                byte red = rgbdata[offset + 1];
                byte green = rgbdata[offset + 2];
                // ������ɫ��Color.FromArgb �Ĳ���˳��Ϊ R, G, B��
                Color color = Color.FromArgb(red, green, blue);
                sliderLights[i].BackColor = color;
            }
        }

        private unsafe void ReadDeviceLoop()
        {
            // ����ɨ������
            Chuniio.slider_start_air_scan();
            Chuniio.slider_start_scan();

            // ��ʼ�� data ���飺2��16��
            int[,] data = new int[2, 16];
            int airdata = 0;



            // ���� LED ����һ��
            Chuniio.slider_send_leds(rgbdata);
            // ��ʼ�� response
            slider_packet_t response = new slider_packet_t();
            Chuniio.package_init(ref response);

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
                        // ���� response ����
                        Chuniio.package_init(ref response);
                        this.BeginInvoke(new Action(() =>
                        {
                            // ����UI
                            UpdateUIDisplay(data, airdata);
                        }));
                        break;
                    case (byte)SliderCmd.SLIDER_CMD_AUTO_AIR:
                        Chuniio.package_init(ref response);
                        break;
                    case 0xff: // 0xff ��ʾ��������ʧ�ܣ�
                        // TODO: ������
                        break;
                    default:
                        break;
                }

                Chuniio.slider_send_leds(rgbdata);
            }
        }

        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            isDeviceConnected = false;
            if (readThread != null && readThread.IsAlive)
            {
                // �ȴ��߳̽��������ȴ�1��
                readThread.Join(1000);
            }
            Chuniio.close_port();
            AutoConnectButton.Enabled = true;
            DisconnectButton.Enabled = false;
            ConnectedPortLabel.Text = "δ����";
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
            // ��ȡ��ǰ����ִ�еĳ���
            Assembly assembly = Assembly.GetExecutingAssembly();
            // ͨ������λ�û�ȡ�ļ��汾��Ϣ
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            ushort chuniioapiVersion = Chuniio.chuni_io_get_api_version();
            string chuniioVersion = "0x" + chuniioapiVersion.ToString("X4");
            MessageBox.Show($"�ж�������������Թ��ߣ�ͼ�ν���棩\n\n����汾��Ver. {fvi.FileVersion}\nChuniio�汾��{chuniioVersion}\n\n������ߣ�Tatanako\nӲ�����ߣ�Qinh\n�ر���л��Soda���ҵ�Ӳ�������̣�\n\n���������\nhttps://github.com/QHPaeek/Affine_IO/blob/master/chuniio/test.c\n��д��ɣ���лQinh��Դ~\n\n�������ȫ��ѣ����뿪Դ��GitHub��\nhttps://github.com/tatanakots/ChuniControllerTestToolGUI", "���ڱ����", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // ��ȡ�������ݣ������� URL �ַ���
            string url = e!.Link!.LinkData!.ToString() == null ? e!.Link!.LinkData!.ToString()! : "https://github.com/tatanakots/ChuniControllerTestToolGUI";
            var processStartInfo = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(processStartInfo);
        }
    }
}
