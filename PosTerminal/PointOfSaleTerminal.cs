using System.Collections.Concurrent;
using PosTerminal.Models;

namespace PosTerminal;

public sealed class PointOfSaleTerminal
{
	private readonly PosTerminalConfig _config;
	private readonly ConcurrentBag<string> _scannedItems;

	/// <summary>
	/// Creates new instance of POS terminal
	/// </summary>
	/// <param name="config"></param>
	public PointOfSaleTerminal(PosTerminalConfig config)
	{
		_config = config;
		_scannedItems = new ConcurrentBag<string>();
	}

	/// <summary>
	/// Scans item
	/// </summary>
	/// <param name="productName"></param>
	/// <exception cref="ArgumentException">If item does not exist in product catalogue</exception>
	public void Scan(string productName)
	{
		ValidateProductExists(productName);

		_scannedItems.Add(productName);
	}

	/// <summary>
	/// Scans multiple items
	/// </summary>
	/// <param name="productNames"></param>
	/// <exception cref="ArgumentException">If item does not exist in product catalogue</exception>
	public void ScanMultiple(params string[] productNames)
	{
		foreach (var productName in productNames)
		{
			ValidateProductExists(productName);

			_scannedItems.Add(productName);
		}
	}

	/// <summary>
	/// Returns total price
	/// </summary>
	/// <returns></returns>
	public decimal CalculateTotal()
	{
		var total = 0m;
		var groupedProducts = _scannedItems.GroupBy(i => i);
		foreach (var productGroup in groupedProducts)
		{
			// We assume item was validated on Scanning step 
			var productConfig = _config.Catalogue[productGroup.Key];
			total += GetItemsPrice(productGroup.Count(), productConfig, _config.MultiDiscountEnabled);
		}

		return total;
	}

	#region Private members

	private void ValidateProductExists(string productName)
	{
		if (!_config.Catalogue.ContainsKey(productName))
		{
			throw new ArgumentException($"{productName} does not exist in product catalogue.");
		}
	}

	private static decimal GetItemsPrice(int amount, PosTerminalItemConfig config, bool multiDiscountEnabled)
	{
		var discountPriceConfig = config.DiscountPrice;
		if (discountPriceConfig == null)
		{
			return amount * config.Price;
		}

		var discountAmount = discountPriceConfig.Amount;
		var discountPrice = discountPriceConfig.Price;
		if (!multiDiscountEnabled)
		{
			return amount > discountAmount
				? (amount - discountAmount) * config.Price + discountPrice
				: amount * config.Price;
		}

		var discountGroups = (int)Math.Floor((decimal)amount / discountAmount);
		var leftover = amount - discountGroups * discountAmount;

		return discountGroups * discountPrice + leftover * config.Price;
	}

	#endregion
}