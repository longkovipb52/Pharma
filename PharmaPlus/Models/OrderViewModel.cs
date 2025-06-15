using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PharmaPlus.Models
{
    public class OrderHistoryViewModel
    {
        public List<OrderSummary> Orders { get; set; }
        public OrderFilterModel Filter { get; set; }
        public PaginationInfo Pagination { get; set; }
        
        public OrderHistoryViewModel()
        {
            Orders = new List<OrderSummary>();
            Filter = new OrderFilterModel();
            Pagination = new PaginationInfo();
        }
    }
    
    public class OrderSummary
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public int StatusCode { get; set; } // Thay đổi từ string sang int
        public string PaymentMethod { get; set; }
        public int ItemCount { get; set; }
        public bool CanCancel { get; set; }
    }
    
    public class OrderDetailViewModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public int StatusCode { get; set; } // Thay đổi từ string sang int
        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public bool CanCancel { get; set; }
        public List<OrderItemDetail> Items { get; set; }
        public List<OrderStatusHistory> StatusHistory { get; set; }
        
        public OrderDetailViewModel()
        {
            Items = new List<OrderItemDetail>();
            StatusHistory = new List<OrderStatusHistory>();
        }
    }
    
    public class OrderItemDetail
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        
        // Thêm hai thuộc tính để hỗ trợ đánh giá
        public bool HasReviewed { get; set; }
        public int UserRating { get; set; }
    }
    
    public class OrderStatusHistory
    {
        public string Status { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
    }
    
    public class OrderFilterModel
    {
        public string Keyword { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? Status { get; set; }
        
        public OrderFilterModel()
        {
            // Mặc định tìm kiếm 3 tháng gần đây
            FromDate = DateTime.Now.AddMonths(-3);
            ToDate = DateTime.Now;
        }
    }
    
    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        
        public PaginationInfo()
        {
            CurrentPage = 1;
            PageSize = 10;
        }
    }
}