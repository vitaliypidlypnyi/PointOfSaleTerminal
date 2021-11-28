namespace PosTerminal.Models;

public record PosTerminalItemConfig
{
	public string Name { get; set; } = default!;
	public decimal Price { get; set; }
	public PosTerminalItemDiscountConfig? DiscountPrice { get; set; }
}

public record PosTerminalItemDiscountConfig
{
	public int Amount { get; set; }
	public decimal Price { get; set; }
}