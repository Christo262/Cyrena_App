using System;

namespace MyLib.Models
{
    /// <summary>
    /// Represents a client that can receive invoices.
    /// A client can be either a business or an individual person.
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Unique identifier for the client.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the client. For a business this is the company name;
        /// for a person this is the full name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Email address used for invoice delivery.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Physical address of the client.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Indicates whether the client is a business or an individual.
        /// </summary>
        public ClientType Type { get; set; }

        /// <summary>
        /// For business clients this is the VAT / tax identifier.
        /// For person clients this can be left null.
        /// </summary>
        public string TaxId { get; set; }
    }

    /// <summary>
    /// Enumerates the supported client categories.
    /// </summary>
    public enum ClientType
    {
        Person,
        Business
    }
}
