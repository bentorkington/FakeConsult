using System;
namespace FakeConsult
{
    public partial class Class1
    {
        public void ConsultHook()
        { //cdaf
            //enable interrupts
            if (!Fg_9f0_20 || FgSendFrame)
            {
                // disable interrupts and 
                return;
            }
            else
            {
                // disable interrupts
            }
            // use the masks at 0x1940-0x194d against several vars
            switch ((Commands)NcEnqCmd)
            {
                case Commands.EcuInfo:
                    //cd4f
                    ReadEcuPart();
                    break;
                case Commands.ClearDiag2:
                    //ce69
                    ReadFaults();
                    break;
                case Commands.GetDiag2:
                    // ce75
                    ReadFaults();
                    break;
                default:
                    Sub_cf29();
                    // cf29
                    break;
            }
        }

        void ReadEcuPart()
        {    // cd4f
            byte[] ecuId = { 
                0x00, 0x20, 0x16, 0x80, 0xC0, 0x00, 0x00, 0xBF, 
                0x80, 0x00, 0xC3, 0x00, 0x00, 0x00, 0x00, 0xFF, 
                0xFF, 0x35, 0x4a, 0x30, 0x30, 0x30 }; // "5J000"

            for (int i = 0; i < ecuId.Length; i++)
            {
                TransmitBuffer[2 + i] = ecuId[i];
            }
            SendFrame((byte)(ecuId.Length * 2));    //??? don't ask me why, this is how they did it on 5J000
        }

        void ReadFaults()
        {  // ce75
            TransmitBuffer[3] = 0x55;
            TransmitBuffer[4] = 0;
            SendFrame(2);
        }

        void Sub_cf29()
        {
            if (FgReadRomRequested)
            {
                // cf34
            }
            else if (FgRegEnq)
            {
                // cf67
                for (int i = 0; i < RegisterStreamCursor; i++)
                {

                    TransmitBuffer[2 + i] = RequestedRegisters[i].GetValue();
                }
                Sub_cf96((byte)(RegisterStreamCursor - 1));
            }
            else
            {
                Sub_cf96(0);
            }
        }

    }
}
