using System;

namespace SnapchatLib;

internal interface IClientLogger
{
    void Debug(string msg);
    void Info(string msg);
}

internal class ClientLogger : IClientLogger
{
    private SnapchatLockedConfig m_Config;
    
    public ClientLogger(SnapchatLockedConfig config)
    {
        m_Config = config;
    }
    
    public void Debug(string msg)
    {
        if (m_Config.Debug)
            Console.WriteLine($"[DEBUG]: {msg}");
    }

    public void Info(string msg)
    {
        Console.WriteLine($"[INFO]: {msg}");
    }
}