using System;

namespace FakeConsult
{
    public partial class Class1
    {
        public Class1() {
            FgNeedInit = true;
        }


        void ReceiveComplete(byte rxbyte_bl) {
            Console.Write("rxComp: {0:x} ", rxbyte_bl);
            if (FgNeedInit)
            {
                if (!UartError && rxbyte_bl == (byte)Commands.Reset)
                {
                    if (GotOneFF)
                    {
                        FgNeedInit = false;
                        ReadyForEf = true;
                        GotOneFF = false;
                    }
                    else {
                        GotOneFF = true;

                        // cab5
                        FgNeedInit = true;
                        ReadyForEf = false;
                    }
                }
            } else if (ReadyForEf) {
                if (!UartError && rxbyte_bl == (byte)Commands.Init)
                {
                    Console.WriteLine("Connected");
                    ReadyForEf = false;
                    ReadyForCmd = true;
                    Respond3(rxbyte_bl);
                }
                else {
                    // cab5
                    FgNeedInit = true;
                    ReadyForEf = false;
                }
            } else if (ReadyForCmd) {
                if (UartError)
                    RequestStop();  // cb5e
                if (FgArg)
                {
                    ReadParam(rxbyte_bl);
                }
                else {
                    // cafe
                    if (rxbyte_bl == (byte)Commands.Reset)
                    {
                        Sub_cb03(rxbyte_bl);
                    }
                    else if (rxbyte_bl == (byte)Commands.Stop) {
                        FgStopRequested = true;
                        StopSending();
                        RespondCmd(rxbyte_bl);
                    }
                    else {
                        CommandSwitch(rxbyte_bl);
                    }
                }
            } else {
                // cc86: check for stop/go/regset/enqcmd
                if (FgStreamRegSet)
                {
                    Console.Write("streamcntl {0:x}", rxbyte_bl);

                    // cc8e
                    if (UartError)
                    {
                        ConsultError();
                    }
                    else if (rxbyte_bl == (byte)Commands.Reset) {
                        Sub_cb03(rxbyte_bl);
                    } else if (rxbyte_bl == (byte)Commands.Stop) {    // stop
                        // cb1a inlined
                        FgStopRequested = true;
                        StopSending();
                        RespondCmd(rxbyte_bl);
                    } else if (rxbyte_bl == (byte)Commands.Go) {    // go
                        // cca8
                        GotOneFF = false;
                        FgStreamRegSet = false;
                        Fg_9f0_20 = true;
                        // todo copy override flags to effective override flags
                    } else if (rxbyte_bl == (byte)Commands.RegisterSet) {
                        // ccca
                        if (RegSetChars > 5)
                        {
                            ConsultError();
                        } else {
                            FgReadRomRequested = false;
                            FgRegEnq = false;
                            FgStreamRegSet = false;
                            FgArg = true;
                            ReadyForCmd = true;
                            RespondCmd(NcRxByte);
                        }

                    } else {
                        Console.Write("diag {0:x}", rxbyte_bl);

                        //ccd7
                        if (NcEnqCmd == 0) {
                            if (rxbyte_bl == (byte)Commands.ClearDiag3)
                            {
                                StoreEnq2(NcRxByte);
                            }
                            else {
                                Sub_ccf4(NcRxByte);
                            }
                        } 
                        else if (NcEnqCmd == (byte)Commands.ClearDiag3) {
                            Sub_ccf4(rxbyte_bl);
                            StoreEnq2(rxbyte_bl);
                        
                        } else {
                            ConsultError();
                        }
                    }
                }
                else {
                    Console.Write("todo {0:x}", rxbyte_bl);
                    if (Fg_9f0_20)
                    {
                        if (UartError)
                        {
                            ConsultError();
                        }
                        else if (rxbyte_bl == (byte)Commands.Reset) {
                            Sub_cb03(rxbyte_bl);
                        }
                        else if (rxbyte_bl == (byte)Commands.Stop) {
                            //cb1a
                            FgStopRequested = true;
                            StopSending();
                            RespondCmd(rxbyte_bl);
                        }
                    }
                    // todo cd23
                }
            }

            Console.WriteLine();
        }

        void Sub_ccf4(byte val_bl) {
            if (RegisterStreamCursor > 20)
            {
                ConsultError();
            } else {
                switch ((Commands)val_bl)
                {
                    case Commands.MemoryEnquiry:
                        if (!FgRegEnq)
                        {
                            FgReadRomRequested = true;
                        }
                        else {
                            ConsultError();
                        }
                        FgStreamRegSet = false;
                        break;
                    case Commands.RegisterEnquiry:
                        if (!FgReadRomRequested)
                        {
                            FgRegEnq = true;
                        }
                        else {
                            ConsultError();
                        }
                        FgStreamRegSet = false;
                        ReadyForCmd = true;
                        FgArg = true;
                        break;
                    default:
                        ConsultError();
                        break;
                }

            }
        }

        void Sub_cb03(byte val_bl) {
            if (GotOneFF)
            {
                //cb08
                FgAtn1 = true;
                StopSending();
                RespondCmd(val_bl);
            } else {
                GotOneFF = true;
                Respond3(val_bl);
            }
        }

        void ReadParam(byte val_bl) {       // cb9f
            if (FgReadRomRequested)
            {
                Console.Write("romenq {0:x}", val_bl);
                // cba4
                RequestedRegisters[RegisterStreamCursor] = null; // XXX val_bl;
                RegisterStreamCursor++;

                if (FgMsbLsb) {
                    FgMsbLsb = false;
                    // can't make sense of what happens at cbc3
                } else {
                    FgMsbLsb = true;
                }
                RespondArg();
            }
            else if (FgRegEnq) {
                Console.Write("regenq {0:x}", val_bl);
                //cbd3
                if (val_bl > 0x51)
                {
                    ConsultError();
                    Console.WriteLine("%% invalid reg");
                }
                else
                {
                    Console.WriteLine($"added register {val_bl}");
                    AddRegisterToStream(val_bl);
                }
            } else if (FgRegisterAddress) {
                Console.Write("regwrite {0:x}", val_bl);

                //cc60
                ConsultOverrideTable[OverrideCursor] = val_bl;
                FgRegisterAddress = false;
                RespondArg();
            } else {
                Console.Write("todo {0:x}", val_bl);

                // cc0a
                //todo
            }
        }

        void RespondArg() { // cc77
            FgArg = false;
            ReadyForCmd = false;
            FgStreamRegSet = true;
            Respond2(NcRxByte);
        }

        void ConsultError() {
            FgStopRequested = true;
            StopSending();
            Respond2(0xfe);
        }

        /// <summary>
        /// 0xcb24
        /// </summary>
        /// <param name="cmd_bl">Cmd bl.</param>
        void CommandSwitch(byte cmd_bl) {
            Console.Write("cmd {0:x} ", cmd_bl);
            switch ((Commands)cmd_bl)
            {
                case Commands.RegisterSet:
                    FgReadRomRequested = false;
                    FgRegEnq = false;
                    FgArg = true;
                    RespondCmd(cmd_bl);
                    break;

                case Commands.ClearDiag3:
                    StoreEnq2(cmd_bl);
                    break;

                case Commands.RegisterEnquiry:
                    FgRegEnq = true;
                    // inlined cb43
                    FgArg = true;
                    RespondCmd(cmd_bl);
                    break;

                case Commands.MemoryEnquiry:
                    FgReadRomRequested = true;
                    // inlined cb43
                    FgArg = true;
                    RespondCmd(cmd_bl);
                    break;

                case Commands.EcuInfo:
                case Commands.GetDiag2:
                case Commands.ClearDiag2:
                    StoreEnqCmd(cmd_bl);
                    break;

                default:
                    RequestStop();
                    break;
            }
        }

        void StopSending() {        // cd64
            //todo
            // save 9f1 flags in 0608
            var temp = FgAtn1;

            if (FgStopRequested || FgAtn1)
            {
                // 9e1 bitflags
                ReadyForEf = false;
                ReadyForCmd = false;
                FgArg = false;
                FgStreamRegSet = false;
                Fg_9f0_20 = false;
                GotOneFF = false;

                // 9e0 bitflags
                FgRegisterAddress = false;
                FgReadRomRequested = false;
                FgRegEnq = false;

                // clear 09f2 04, 09f2 02
                FgErrorsCleared = false;
                Fg_9f2_02 = false;

                if (temp)
                    ReadyForEf = true;
                else
                    ReadyForCmd = true;

                NcEnqCmd = 0;
                RegisterStreamCursor = 0;
                RegSetChars = 0;

                // todo clear overrids 9e8, 9ea, 9ec, 9e0, 9e2, 9e4

                FgStopRequested = false;
                FgAtn1 = false;

                Uart_2e_02 = true;

            }
        }

        void StoreEnq2(byte cmd_bl)
        {
            // clear some regs, todo
            // these are the "3rd set" of diags from PLMS doc
            // see 0xcb6c
            StoreEnqCmd(cmd_bl);
        }

        void StoreEnqCmd(byte cmd_bl)
        {
            NcEnqCmd = cmd_bl;
            ReadyForCmd = false;
            FgStreamRegSet = true;

            RespondCmd(cmd_bl);
        }

        void RequestStop()
        {        // cb5e
            FgStopRequested = true;
            StopSending();
            Respond2(0xfe);
        }

        void AddRegisterToStream(byte val_bl)
        {  // cbe0
            var vect = ConsultRegisterAddresses[val_bl];
            if (vect == null)
            {
                ConsultError();
            }
            else
            {
                RequestedRegisters[RegisterStreamCursor] = vect;
                RegisterStreamCursor++;
                RespondArg();
            }
        }

        /// <summary>
        /// Has side effects!
        /// </summary>
        /// <param name="val_bl">Value bl.</param>
        void Respond2(byte val_bl)  
        {
            if (FgSendFrame || !UartTxBufEmp)
            {
                Uart_2e_02 = false;
                FgRespondCmd = true;
                NcDeferByte = val_bl;
            }
            else
            {
                StopSending();
                Uart0Tx = val_bl;
                Uart0Cntl1 &= 0xfd; // explicitly clear the TxBufEmp flag
            }
        }

        void Respond3(byte val_bl)
        {
            Respond2((byte)~val_bl);
        }

        void RespondCmd(byte val_bl)
        {
            GotOneFF = false;
            Respond3(val_bl);
        }

        void SendFrame(byte length_bl)
        {  // d048
            TransmitBuffer[0] = 0xff;
            TransmitBuffer[1] = length_bl;

            // enable interrupts

            if (Fg_9f0_20)
            {
                if ((Uart0Cntl1 & 0x02) == 0 || !FgRespondCmd) // (TxBufEmp)
                {
                    Uart_2e_02 = false;
                    FgSendFrame = true;
                }

            }
            // disable interrupts

        }





        void Sub_cf3c(byte val_bl)
        {
            int i = 0;
            while ((i * 2) < RegisterStreamCursor)
            {
                var addr = RequestedRegisters[i];

                TransmitBuffer[2 + i] = 0; // todo simulate lookup
            }
            Sub_cf96(val_bl);
        }

        void Sub_cf96(byte val_bl)
        {
            if (NcEnqCmd == 0x51)
            {
                //cfad todo
            }
            else
            {
                if (val_bl != 0 || RegSetChars != 0)
                {
                    SendFrame(val_bl);
                }
            }
        }




        public void UartVector()
        {      // ca19
            if (Uart_2f_04)    // was this interrupt for UART1?  
            {
                // ldm 0x00, dp+0x3d
                //sub_63ec();  process UART1
            }
            else if (false)         // if bit 01 of 0x229f is set, which is false
            {
                // ba8d, some other version similar to Consult?
            }
            else
            {
                UartError = (Uart0Cntl1 & 0x80) != 0;
                UartRxComp = (Uart0Cntl1 & 0x08) != 0;
                UartRxEnable = (Uart0Cntl1 & 0x04) != 0;
                UartTxBufEmp = (Uart0Cntl1 & 0x02) != 0;
                UartTxEnable = (Uart0Cntl1 & 0x01) != 0;

                NcRxByte = Uart0Rx; 

                if (!Uart_2e_02 && UartTxBufEmp)
                {
                    if (FgSendFrame)
                    {
                        int temp = TxBufCnt;
                        TxBufCnt++;
                        // ca59
                        if (temp > TransmitBuffer[1])
                        {
                            if (!FgRespondCmd)
                            {
                                Uart_2e_02 = true;
                            }
                            FgSendFrame = false;
                            TxBufCnt = 0;
                        }
                        // ca7a
                        Sub_ca89(TransmitBuffer[temp]);
                    }
                    else
                    {
                        Uart_2e_02 = true;
                        FgRespondCmd = false;
                        StopSending();
                        Sub_ca89(NcDeferByte);
                    }
                }
                //ca8b

                if (Uart_2e_01)
                {
                    if (UartError || UartRxComp)
                    {
                        ReceiveComplete(NcRxByte);
                    }
                }

            }
            ExitInterrupt();
        }


        void Sub_ca89(byte val_al)
        {
            Uart0Tx = val_al;
            Uart0Cntl1 &= 0xfd; // explicitly clear the TxBufEmp flag
        }

        void ExitInterrupt()
        {
            /* volatile! */
        }





    }

    public class SimulatedRegister {
        public byte GetValue() {
            return (byte)_a;
        }

        int _a;

        public SimulatedRegister(int a) {
            _a = a;
        }
    }

}
