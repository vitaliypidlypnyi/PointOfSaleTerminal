using System.Collections.Generic;

namespace PosTerminal.Models;

public sealed record PosTerminalConfig
{
	public bool MultiDiscountEnabled { get; set; }

	public Dictionary<string, PosTerminalItemConfig> Catalogue { get; set; } = new();
}