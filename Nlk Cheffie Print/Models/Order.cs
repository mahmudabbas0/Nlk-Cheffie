using System;
using System.Collections.Generic;

namespace Nlk_Cheffie_Print.Models
{
    public class OrderItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public string Name { get; set; } = "";
        public string UnitPrice { get; set; } = "0.00";
        public string LineTotal { get; set; } = "0.00";
        public List<string> RemovedCustomizations { get; set; } = new List<string>();
        public List<string> AddedCustomizations { get; set; } = new List<string>();
        public string? Notes { get; set; }
        public string? TaxRate { get; set; }
        public string? TaxAmount { get; set; }
    }

    public class Order
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string OrderNumber { get; set; } = "";
        public string TableName { get; set; } = "";
        public string WaiterName { get; set; } = "";
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string Section { get; set; } = "kitchen"; // kitchen, cashier, courier
        public string TotalAmount { get; set; } = "0.00";
        public string Status { get; set; } = "pending"; // pending, accepted, preparing, ready, etc.
        public string PaymentMethod { get; set; } = "cash"; // cash, card, online
        public string PaymentStatus { get; set; } = "pending"; // pending, paid
        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string CustomerEmail { get; set; } = "";
        public string DeliveryAddress { get; set; } = "";
        public string OrderNote { get; set; } = "";
        public string Subtotal { get; set; } = "0.00";
        public string Tax { get; set; } = "0.00";
        public string ExtrasTotal { get; set; } = "0.00";
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
