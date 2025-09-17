// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Nalix.Host.Terminals;

internal interface IConsoleReader
{
    System.Boolean KeyAvailable { get; }

    System.ConsoleKeyInfo ReadKey(System.Boolean intercept);
}

internal class ConsoleReader : IConsoleReader
{
    public System.Boolean KeyAvailable => System.Console.KeyAvailable;

    public System.ConsoleKeyInfo ReadKey(System.Boolean intercept) => System.Console.ReadKey(intercept);
}