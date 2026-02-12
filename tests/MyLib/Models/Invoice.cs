using System;
using System.Collections.Generic;
using System.Linq;

namespace MyLib.Models
{
    /// <summary>
    /// Represents an invoice issued to a client.
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// Unique identifier for the invoice.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The client that the invoice is issued to.
        /// </summary>
        public Client Client { get; set; }

        /// <summary>
        /// The date the invoice was created.
        /// </summary>
        public DateTime IssueDate { get; set; }

        /// <summary>
        /// The due date for payment.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// The status of the invoice.
        /// </summary>
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        /// <summary>
        /// The currency code for the invoice (ISO 4217, e.g., "USD").
        /// </summary>
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// The line items that make up the invoice.
        /// </summary>
        public List<InvoiceLineItem> LineItems { get; set; } = new();

        /// <summary>
        /// The total amount of the invoice.
        /// </summary>
        public decimal TotalAmount => LineItems?.Sum(item => item.Total) ?? 0m;
    }

    /// <summary>
    /// Represents a single line item on an invoice.
    /// </summary>
    public class InvoiceLineItem
    {
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => Quantity * UnitPrice;
    }

    /// <summary>
    /// Possible statuses for an invoice.
    /// </summary>
    public enum InvoiceStatus
    {
        Draft,
        Sent,
        Paid,
        Overdue,
        Cancelled
    }
}
