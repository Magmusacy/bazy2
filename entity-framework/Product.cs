public class Product
{
  public int ProductID { get; set; }
  public String? ProductName { get; set; }
  public int UnitsInStock { get; set; }

  public int SupplierID { get; set; }
  public Supplier? Supplier { get; set; }
}