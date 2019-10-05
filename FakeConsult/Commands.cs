using System;
namespace FakeConsult
{
    public enum Commands : byte
    {
        RegisterSet     = 0x0a,
        Stop            = 0x30,
        ClearDiag3      = 0x51,
        RegisterEnquiry = 0x5a,
        ClearDiag2      = 0xc1,
        MemoryEnquiry   = 0xc9,
        EcuInfo         = 0xd0,
        GetDiag2        = 0xd1,
        Go              = 0xf0,
        Init            = 0xef,
        Reset           = 0xff,
    }
}
