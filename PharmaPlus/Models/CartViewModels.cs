using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PharmaPlus.Models
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }

        public string FormattedTotalAmount
        {
            get { return TotalAmount.ToString("N0") + " VNĐ"; }
        }

        public bool HasItems
        {
            get { return Items != null && Items.Any(); }
        }
    }

    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public decimal SubTotal { get; set; }
        public int Stock { get; set; }
        public bool PrescriptionRequired { get; set; }

        public string FormattedPrice
        {
            get { return Price.ToString("N0") + " VNĐ"; }
        }

        public string FormattedSubTotal
        {
            get { return SubTotal.ToString("N0") + " VNĐ"; }
        }

        public bool IsOutOfStock
        {
            get { return Stock <= 0; }
        }

        public bool IsLowStock
        {
            get { return Stock > 0 && Stock <= 5; }
        }

        public string StockStatus
        {
            get
            {
                if (IsOutOfStock) return "out-of-stock";
                if (IsLowStock) return "low-stock";
                return "in-stock";
            }
        }
    }

    // Session cart item (lightweight)
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }

        public decimal SubTotal
        {
            get { return Price * Quantity; }
        }
    }
}