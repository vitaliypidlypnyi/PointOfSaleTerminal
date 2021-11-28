using System.Reflection;
using System.Text.Json;
using PosTerminal.Models;
using Xunit;

namespace PosTerminal.Tests;

public class PosTerminalTests
{
	[Theory]
	[InlineData("ABCDABA", 13.25)]
	[InlineData("CCCCCCC", 6)]
	[InlineData("ABCD", 7.25)]
	public async Task CalculateTotal_MultipleItemsWithDiscount_ShouldReturnCorrectPriceWithDiscount(string items, decimal result)
	{
		//Arrange
		var configJson = await ReadJson("posConfig1.json");
		var config = JsonSerializer.Deserialize<PosTerminalConfig>(configJson, new JsonSerializerOptions{ PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
		var terminal = new PointOfSaleTerminal(config!);

		// Act
		foreach (var item in items)
		{
			terminal.Scan(item.ToString());
		}
		var total = terminal.CalculateTotal();

		//Assert
		Assert.Equal(result, total);
	}

	[Fact]
	public void Scan_NonExistingItem_ShouldThrow()
	{
		// Arrange
		var config = new PosTerminalConfig
		{
			Catalogue = new Dictionary<string, PosTerminalItemConfig>
			{
				{
					"A", new PosTerminalItemConfig
					{
						Name = "A",
						Price = 10
					}
				}
			}
		};
		var terminal = new PointOfSaleTerminal(config);

		// Act
		var scanAction = () => terminal.Scan("B");

		// Assert
		var exception = Assert.Throws<ArgumentException>(scanAction);
		Assert.Equal($"B does not exist in product catalogue.", exception.Message);
	}

	[Theory]
	[InlineData("AAAAABBBAAAAACCCCCCCDDDDCCC", true, 33.90)]
	[InlineData("AAAAABBBAAAAACCCCCCCDDDDCCC", false, 35.70)]
	public async Task CalculateTotal_WithAndWithoutMultipleDiscount_ShouldReturnCorrectPriceWithDiscount(string items, bool multiDiscountEnabled, decimal result)
	{
		// Arrange
		var configJson = await ReadJson("posConfig2.json");
		var config = JsonSerializer.Deserialize<PosTerminalConfig>(configJson, new JsonSerializerOptions{ PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
		config!.MultiDiscountEnabled = multiDiscountEnabled;
		var terminal = new PointOfSaleTerminal(config);

		// Act
		terminal.ScanMultiple(items.Select(c => c.ToString()).ToArray());
		var total = terminal.CalculateTotal();

		// Assert
		Assert.Equal(result, total);
	}

	private Task<string> ReadJson(string name)
	{
		var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, name);
		return File.ReadAllTextAsync(path);
	}
}