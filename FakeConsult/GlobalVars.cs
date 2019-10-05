using System;
namespace FakeConsult
{
    public partial class Class1
    {
        byte[] ConsultOverrideTable = new byte[999]; // 09b0

        bool GotOneFF = false;          // 09f0 80   
        bool Fg_9f0_20 = false;         // 09f0 20
        bool FgStreamRegSet = false;    // 09f0 10
        bool FgArg = false;             // 09f0 08
        bool ReadyForCmd = false;       // 09f0 04
        bool ReadyForEf = false;        // 09f0 02
        bool FgNeedInit = false;        // 09f0 01

        bool FgRegisterAddress = false; // 09f1 80
        bool FgReadRomRequested = false;// 09f1 40
        bool FgRespondCmd = false;      // 09f1 20
        bool FgSendFrame = false;       // 09f1 10
        bool FgRegEnq = false;          // 09f1 08
        //bool Fg_09f1_04 = false;        // 09f1 04 Unused
        bool FgStopRequested = false;   // 09f1 02
        bool FgAtn1 = false;            // 09f1 01

        bool FgErrorsCleared = false;   // 09f2 04
        bool Fg_9f2_02 = false;         // 09f2 02
        bool FgMsbLsb = false;          // 09f2 01

        // flags of 0x0a0e
        bool UartError = false;     // 80
        bool UartRxComp;            // 08   cleared when LSB of buffer is read
        bool UartRxEnable;          // 04
        bool UartTxBufEmp;          // 02  set when copied to tx reg, cleared when tx buf is written
        bool UartTxEnable;          // 01


        byte NcRxByte = 0;              // 0a0f
        byte NcDeferByte = 0;           // 0a10
        byte TxBufCnt;                  // 0a11
        byte NcEnqCmd = 0;              // 0a12
        byte RegisterStreamCursor = 0;  // 0a13 
        byte RegSetChars = 0;           // 0a14
        byte OverrideCursor = 0;        // 0a15

        byte[] TransmitBuffer = new byte[999]; // 0a1a

        SimulatedRegister[] RequestedRegisters = new SimulatedRegister[256]; // 0a52 RegisterStreamCursor is index

        // IO

        // 0x2e, init to 0xfc or 0xf8
        public bool Uart_2e_01;         // must be true for ReceiveComplete() to run
        public bool Uart_2e_02;         // set in StopSending()
                                        // cleared in SendFrame when FgSendFrame is set


        public bool Uart_2f_04;        // interrupt was for UART1
        public bool Uart_2f_02;        // interrupt was for receive 

        public byte Uart0Tx           // 0x32
        {
            get { return _uart_tx; }
            set { _uart_tx = value; Console.WriteLine("txwrite: {0:x}", value); }
        }

        public byte Uart0Cntl1;        // 35
        public byte Uart0Rx           // 36
        { get { Uart0Cntl1 &= 0xf7; return _uart_rx; } set { _uart_rx = value; Uart0Cntl1 |= 0x08; } }


        private byte _uart_rx;
        private byte _uart_tx;


        // statics



        static byte[] ConsultErrorCode = {     // 0xd0d1
            0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
            0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28,
            0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38,
            0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
            0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58,
            0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68,
            0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78,
            0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88,
            0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98,
            0xa1, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6, 0xa7, 0xa8,
            0xb1, 0xb2, 0xb3, 0xb4, 0xb5, 0xb6, 0xb7, 0xb8,
            0xc1, 0xc2, 0xc3, 0xc4, 0xc5, 0xc6, 0xc7, 0xc8,
            0xd1, 0xd2, 0xd3, 0xd4, 0xd5, 0xd6, 0xd7, 0xd8,
            0xe1, 0xe2, 0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8,
        };

        /*
        ushort[] ConsultRegisterAddresses = {   // 0x2ee8
            0x0a75, // tach msb
            0x0a74, // tach lsb
            0x0685,
            0x0684,
            0x07e3, // maf msb
            0x07e2, // maf lsb
            0xffff,     // maf # 2 msb
            0xffff,
            0x067e, // coolant temp
            0x0a68, // L/H O2
            0xffff,     // R/H O2
            // ... 
        };
        */

        SimulatedRegister[] ConsultRegisterAddresses = {
            new SimulatedRegister(1),
            new SimulatedRegister(200),
            new SimulatedRegister(1),
            new SimulatedRegister(200),
            new SimulatedRegister(1),
            new SimulatedRegister(200),
            null,
            null,
            new SimulatedRegister(100),
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
        };

    }
}
