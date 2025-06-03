// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

ProdContext prodContext = new ProdContext();
Console.WriteLine("Podaj nazwę produktu: ");
String? prodName = Console.ReadLine();
Product flamajster = new Product { ProductName = prodName };
prodContext.Products.Add(flamajster);
prodContext.SaveChanges();

var query = from prod in prodContext.Products select prod.ProductName;

foreach (var pName in query)
{
  Console.WriteLine(pName);
}