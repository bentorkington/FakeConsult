using System;
using FakeConsult;

namespace Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var consult = new FakeConsult.Class1();

            consult.Uart_2e_02 = true;


            byte[] instructions = {
                0xff, 0xff, 0xef,
                //0xd0,
                (byte)Commands.RegisterEnquiry, 0x0,
                (byte)Commands.RegisterEnquiry, 0x1,
                (byte)Commands.RegisterEnquiry, 0x2,
                (byte)Commands.Go,
                //(byte)Commands.Stop,
             };

            foreach (var item in instructions)
            {
                consult.Uart0Rx = item;
                consult.Uart_2e_01 = true;
                consult.Uart0Cntl1 = 0x0f;

                consult.UartVector();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                if ((consult.Uart0Cntl1 & 0x2) == 0)
                {
                    Console.WriteLine("Received: {0:x}", consult.Uart0Tx);
                }

            }

            for (int i = 0; i < 80; ++i ) {
                consult.UartVector();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                if ((consult.Uart0Cntl1 & 0x2) == 0)
                {
                    Console.WriteLine("Received: {0:x}", consult.Uart0Tx);
                    consult.Uart0Cntl1 |= 0x02;
                }
            }

            foreach (var item in new byte[] {0x30})
            {
                consult.Uart0Rx = item;
                consult.Uart_2e_01 = true;
                consult.Uart0Cntl1 = 0x0f;

                consult.UartVector();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                if ((consult.Uart0Cntl1 & 0x2) == 0)
                {
                    Console.WriteLine("Received: {0:x}", consult.Uart0Tx);
                }

            }

            for (int i = 0; i < 80; ++i)
            {
                consult.UartVector();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                consult.ConsultHook();
                if ((consult.Uart0Cntl1 & 0x2) == 0)
                {
                    Console.WriteLine("Received: {0:x}", consult.Uart0Tx);
                    consult.Uart0Cntl1 |= 0x02;
                }
            }
        }
    }
}
