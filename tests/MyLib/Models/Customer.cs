using System.Collections.Generic;

namespace MyLib.Models;

public class Customer
{
    public string OrganizationName { get; set; } = string.Empty;
    public string? OrganizationDetails { get; set; }

    public List<CustomerContact> Contacts { get; set; } = new();
}
