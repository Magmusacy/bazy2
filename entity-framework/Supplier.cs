public class Supplier
{
  public required String CompanyName { get; set; }
  public required String Street { get; set; }
  public required String City { get; set; }

  public ICollection<Product> Products { get; set; } = new List<Product>();
}