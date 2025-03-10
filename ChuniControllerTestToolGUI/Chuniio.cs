using System;
using System.Runtime.InteropServices;

namespace ChuniControllerTestToolGUI
{

    public enum SliderCmd : byte
    {
        SLIDER_CMD_NOP = 0,
        SLIDER_CMD_AUTO_SCAN = 0x01,
        SLIDER_CMD_SET_LED = 0x02,
        SLIDER_CMD_AUTO_SCAN_START = 0x03,
        SLIDER_CMD_AUTO_SCAN_STOP = 0x04,
        SLIDER_CMD_AUTO_AIR = 0x05,
        SLIDER_CMD_AUTO_AIR_START = 0x06,
        SLIDER_CMD_SET_AIR_LED_LEFT = 0x07,
        SLIDER_CMD_SET_AIR_LED_RIGHT = 0x08,
        SLIDER_CMD_DIVA_UNK_09 = 0x09,
        SLIDER_CMD_DIVA_UNK_0A = 0x0A,
        SLIDER_CMD_RESET = 0x10,
        SLIDER_CMD_GET_BOARD_INFO = 0xF0
    }

    // 对应 C 中的 slider_packet_t
    // 使用 Explicit 布局，并且 Size = 128 对应 BUFSIZE
    [StructLayout(LayoutKind.Explicit, Size = 128)]
    public unsafe struct slider_packet_t
    {
        // 整个 128 字节数据块
        [FieldOffset(0)]
        public fixed byte data[128];

        // 前 3 字节分别为 syn, cmd, size
        [FieldOffset(0)]
        public byte syn;

        [FieldOffset(1)]
        public byte cmd;

        [FieldOffset(2)]
        public byte size;

        // 以下是 union 部分，从偏移 3 开始
        // 1. 作为 led 控制：led_unk + leds[96]
        [FieldOffset(3)]
        public byte led_unk; // 占 1 字节
        // leds 数组从 offset 4 开始，共 96 字节
        [FieldOffset(4)]
        public fixed byte leds[96];

        // 2. 作为 version 字符串，32 字节（ANSI编码）
        [FieldOffset(3)]
        public fixed sbyte version[32];

        // 3. 作为压力数据和空气状态：
        // pressure 数组占 32 字节，从 offset 3 开始
        [FieldOffset(3)]
        public fixed byte pressure[32];
        // 紧跟在 pressure 数组后，air_status 放在 offset 3+32 = 35
        [FieldOffset(35)]
        public byte air_status;

        // 4. 作为 air_leds 数组，9 字节，从 offset 3 开始
        [FieldOffset(3)]
        public fixed byte air_leds[9];

        // 5. 另外一个单字节字段 _air_status（与 union 中其它字段重叠）
        [FieldOffset(3)]
        public byte _air_status;
    }

    // 对应 C 中的 Queue 结构
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Queue
    {
        public IntPtr items; // 指向 char 数组的指针
        public int front;
        public int rear;
        public int size;
        public int capacity;
    }

    public static class Chuniio
    {
        // 1. const char* GetSerialPortByVidPid(const char* vid, const char* pid);
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetSerialPortByVidPid(string vid, string pid);

        // 2. Queue* createQueue(int capacity);
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr createQueue(int capacity);

        // 3. void enqueue(Queue* queue, char item);
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void enqueue(ref Queue queue, byte item);

        // 4. char dequeue(Queue* queue);
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte dequeue(ref Queue queue);

        // 5. BOOL open_port();
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool open_port();

        // 6. void close_port();
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void close_port();

        // 7. BOOL IsSerialPortOpen();
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsSerialPortOpen();

        // 8. void sliderserial_writeresp(slider_packet_t *request);
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void sliderserial_writeresp(ref slider_packet_t request);

        // 9. DWORD WINAPI sliderserial_read_thread(LPVOID param);
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint sliderserial_read_thread(IntPtr param);

        // 10. BOOL serial_read1(uint8_t *result);
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool serial_read1(ref byte result);

        // 11. uint8_t serial_read_cmd(slider_packet_t *reponse);
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte serial_read_cmd(ref slider_packet_t reponse);

        // 12. void package_init(slider_packet_t *request);
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void package_init(ref slider_packet_t request);

        // 13. void slider_rst();
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void slider_rst();

        // 14. void slider_start_scan();
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void slider_start_scan();

        // 15. void slider_stop_scan();
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void slider_stop_scan();

        // 16. void slider_send_leds(const uint8_t *rgb);
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void slider_send_leds(byte[] rgb);

        // 17. void slider_start_air_scan();
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void slider_start_air_scan();

        // 获取 DLL 支持的 API 版本
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort chuni_io_get_api_version();

        // 初始化 JVS 输入，返回 HRESULT
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int chuni_io_jvs_init();

        // 轮询 JVS 输入，opbtn 返回测试/服务按键状态，beams 返回 IR 光束状态（6 个光束）
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void chuni_io_jvs_poll(byte[] opbtn, byte[] beams);

        // 读取投币器计数（总计币数），通过传入指针更新 total
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void chuni_io_jvs_read_coin_counter(ref ushort total);

        // 初始化触摸滑条，返回 HRESULT
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int chuni_io_slider_init();

        // 定义回调函数类型：传入一个指向 32 字节压力数据的指针
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void chuni_io_slider_callback_t(IntPtr state);

        // 启动滑条轮询，并设置回调函数，DLL 内部会开启一个线程周期性调用该回调
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void chuni_io_slider_start(chuni_io_slider_callback_t callback);

        // 停止滑条轮询
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void chuni_io_slider_stop();

        // 设置滑条 LED 灯光，传入一个指向 96 字节 LED 数据的指针
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void chuni_io_slider_set_leds(byte[] rgb);

        // 设置单块 LED 颜色，第一个参数 board 表示板号，rgb 为颜色数据指针（格式由硬件决定）
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void chuni_io_led_set_colors(byte board, byte[] rgb);

        // 初始化 LED 控制，返回 HRESULT
        [DllImport("chuniio_affine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int chuni_io_led_init();
    }
}
