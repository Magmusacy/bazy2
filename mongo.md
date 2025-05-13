# Dokumentowe bazy danych – MongoDB

Ćwiczenie/zadanie


---

**Imiona i nazwiska autorów:**
Paweł Gadomski, Jakub
--- 

Odtwórz z backupu bazę north0

```
mongorestore --nsInclude='north0.*' ./dump/
```

```
use north0
```


# Zadanie 1 - operacje wyszukiwania danych,  przetwarzanie dokumentów

# a)

stwórz kolekcję  `OrdersInfo`  zawierającą następujące dane o zamówieniach
- pojedynczy dokument opisuje jedno zamówienie

```js
[  
  {  
    "_id": ...
    
    OrderID": ... numer zamówienia
    
    "Customer": {  ... podstawowe informacje o kliencie skladającym  
      "CustomerID": ... identyfikator klienta
      "CompanyName": ... nazwa klienta
      "City": ... miasto 
      "Country": ... kraj 
    },  
    
    "Employee": {  ... podstawowe informacje o pracowniku obsługującym zamówienie
      "EmployeeID": ... idntyfikator pracownika 
      "FirstName": ... imie   
      "LastName": ... nazwisko
      "Title": ... stanowisko  
     
    },  
    
    "Dates": {
       "OrderDate": ... data złożenia zamówienia
       "RequiredDate": data wymaganej realizacji
    }

    "Orderdetails": [  ... pozycje/szczegóły zamówienia - tablica takich pozycji 
      {  
        "UnitPrice": ... cena
        "Quantity": ... liczba sprzedanych jednostek towaru
        "Discount": ... zniżka  
        "Value": ... wartośc pozycji zamówienia
        "product": { ... podstawowe informacje o produkcie 
          "ProductID": ... identyfikator produktu  
          "ProductName": ... nazwa produktu 
          "QuantityPerUnit": ... opis/opakowannie
          "CategoryID": ... identyfikator kategorii do której należy produkt
          "CategoryName" ... nazwę tej kategorii
        },  
      },  
      ...   
    ],  

    "Freight": ... opłata za przesyłkę
    "OrderTotal"  ... sumaryczna wartosc sprzedanych produktów

    "Shipment" : {  ... informacja o wysyłce
        "Shipper": { ... podstawowe inf o przewoźniku 
           "ShipperID":  
            "CompanyName":
        }  
        ... inf o odbiorcy przesyłki
        "ShipName": ...
        "ShipAddress": ...
        "ShipCity": ... 
        "ShipCountry": ...
    } 
  } 
]  
```

Kod:

```js
db.orders.aggregate([
	{
		$match: { }
	},
	{
		$lookup: {
			from: "customers",
			localField: "CustomerID",
			foreignField: "CustomerID",
			as: "Customer_tmp"
		}
	},
	{
		$unwind: "$Customer_tmp"
	},
	{
		$lookup: {
			from: "employees",
			localField:"EmployeeID",
			foreignField: "EmployeeID",
			as: "Employee_tmp"
		}
	},
	{
		$unwind: "$Employee_tmp"
	},
	{
		$lookup: {
			from: "orderdetails",
			localField: "OrderID",
			foreignField: "OrderID",
			as: "OrderDetails_tmp"
		}
	},
	{
		$unwind: "$OrderDetails_tmp"
	},
	{
		$lookup: {
			from: "products",
			localField: "OrderDetails_tmp.ProductID",
			foreignField: "ProductID",
			as: "Product_tmp"
		}
	},
	{
		$unwind: "$Product_tmp"
	},
	{
		$lookup: {
			from: "categories",
			localField: "Product_tmp.CategoryID",
			foreignField: "CategoryID",
			as: "Category_tmp"
		}
	},
	{
		$unwind: "$Category_tmp"
	},
	{
		$lookup: {
			from: "shippers",
			localField: "ShipVia",
			foreignField: "ShipperID",
			as: "Shipper_tmp"
		}
	},
	{
		$unwind: "$Shipper_tmp"
	},
	{
		$addFields: {
			Customer: {
				CustomerID: "$Customer_tmp.CustomerID",
				CompanyName: "$Customer_tmp.CompanyName",
				City: "$Customer_tmp.City",
				Country: "$Customer_tmp.Country",
			},
			Employee: {
				EmployeeID: "$Employee_tmp.EmployeeID",
				FirstName: "$Employee_tmp.FirstName",
				LastName: "$Employee_tmp.LastName",
				Title: "$Employee_tmp.Title"
			},
			Dates: {
				OrderDate: "$OrderDate",
				RequiredDate: "$RequiredDate"
			},
			OrderDetails: {
				UnitPrice: "$OrderDetails_tmp.UnitPrice",
				Quantity: "$OrderDetails_tmp.Quantity",
				Discount: "$OrderDetails_tmp.Discount",
				Value: {
					$multiply: [ "$OrderDetails_tmp.UnitPrice", "$OrderDetails_tmp.Quantity", { $subtract: [1, "$OrderDetails_tmp.Discount"] } ]
				 },
				Product: {
					ProductID: "$Product_tmp.ProductID",
					ProductName: "$Product_tmp.ProductName",
					QuantityPerUnit: "$Product_tmp.QuantityPerUnit",
					CategoryID: "$Product_tmp.CategoryID",
					CategoryName: "$Category_tmp.CategoryName"
				}
			},
			Shipment: {
				Shipper: {
					ShipperID: "$Shipper_tmp.ShipperID",
					CompanyName: "$Shipper_tmp.CompanyName"
				},
				ShipName: "$ShipName",
				ShipAddress: "$ShipAddress",
				ShipCity: "$ShipCity",
				ShipCountry: "$ShipCountry"
			}
		}
	},
	{
		$group: {
			_id: "$OrderID",
			OrderTotal: { $sum : "$OrderDetails.Value"  },
			OrderDetails: { $push : "$OrderDetails" },
			OrderID: { $first: "$OrderID" },
			Customer: { $first: "$Customer" },
			Employee: { $first: "$Employee" },
			Dates: { $first: "$Dates" },
			Freight: { $first: "$Freight" },
			Shipment: { $first: "$Shipment" }
		}
	},
  {
		$out: "OrdersInfo"
	}
])
```

# b)

stwórz kolekcję  `CustomerInfo`  zawierającą następujące dane kazdym klencie
- pojedynczy dokument opisuje jednego klienta

```js
[  
  {  
    "_id": ...
    
    "CustomerID": ... identyfikator klienta
    "CompanyName": ... nazwa klienta
    "City": ... miasto 
    "Country": ... kraj 

	"Orders": [ ... tablica zamówień klienta o strukturze takiej jak w punkcie a) (oczywiście bez informacji o kliencie)
	  
	]

		  
]  
```

Kod:

```js

db.OrdersInfo.aggregate([
    {
        $group: {
            _id: "$Customer.CustomerID",
            CustomerID: { $first: "$Customer.CustomerID" },
            CompanyName: { $first: "$Customer.CompanyName" },
            City: { $first: "$Customer.City" },
            Country: { $first: "$Customer.Country" },
            Orders: {
                $push: {
                    OrderID: "$OrderID",
                    OrderTotal: "$OrderTotal",
                    OrderDetails: "$OrderDetails",
                    Employee: "$Employee",
                    Dates: "$Dates",
                    Freight: "$Freight",
                    Shipment: "$Shipment"
                }
            }
        }
    },
    {
        $out: "CustomerInfo"
    }
])

```

# c) 

Napisz polecenie/zapytanie: Dla każdego klienta pokaż wartość zakupionych przez niego produktów z kategorii 'Confections'  w 1997r
- Spróbuj napisać to zapytanie wykorzystując
	- oryginalne kolekcje (`customers, orders, orderdertails, products, categories`)
	- kolekcję `OrderInfo`
	- kolekcję `CustomerInfo`

- porównaj zapytania/polecenia/wyniki

```js
[  
  {  
    "_id": 
    
    "CustomerID": ... identyfikator klienta
    "CompanyName": ... nazwa klienta
	"ConfectionsSale97": ... wartość zakupionych przez niego produktów z kategorii 'Confections'  w 1997r

  }		  
]  
```

Kod: 

```js
// Oryginalne kolekcje:
db.orders.aggregate([
	{
		$lookup: {
			from: "orderdetails",
			localField: "OrderID",
			foreignField: "OrderID",
			as: "Orderdetails_tmp"
		}
	},
	{
		$unwind: "$Orderdetails_tmp"
	},
	{
		$lookup: {
			from: "products",
			localField: "Orderdetails_tmp.ProductID",
			foreignField: "ProductID",
			as: "Products_tmp"
		}
	},
	{
		$unwind: "$Products_tmp"
	},
	{
		$lookup: {
			from: "categories",
			localField: "Products_tmp.CategoryID",
			foreignField: "CategoryID",
			as: "Category_tmp"
		}
	},
	{
		$unwind: "$Category_tmp"
	},
	{
		$match: {
			"Category_tmp.CategoryName": "Confections",
			$expr: {
				$eq: [{ $year: "$OrderDate" }, 1997]
			}
		 }
	},
	{
		$addFields: {
			Value: {
				$multiply: [ "$Orderdetails_tmp.UnitPrice", "$Orderdetails_tmp.Quantity", { $subtract: [1, "$Orderdetails_tmp.Discount"] } ]
			 },
		}
	},
	{
		$lookup: {
			from: "customers",
			localField: "CustomerID",
			foreignField: "CustomerID",
			as: "Customer_tmp"
		}
	},
	{
		$unwind: "$Customer_tmp"
	},
	{
		$group: {
			_id: "$CustomerID",
			CustomerID: { $first: "$CustomerID" },
			CompanyName: { $first: "$Customer_tmp.CompanyName" },
			ConfectionsSale97: { $sum: "$Value" }
		}
	},
	{
		$project: {
			_id: 0,
			CustomerID: 1,
			CompanyName: 1,
			ConfectionsSale97: { $round: ["$ConfectionsSale97", 2] }
		}
	}
])

// Kolekcja OrdersInfo
db.OrdersInfo.aggregate([
	{
		$match: {
			$expr: { $eq: [ {$year: "$Dates.OrderDate"}, 1997 ] },
		}
	},
	{
		$addFields: {
			filteredOrderDetails: {
				$filter: {
					input: "$OrderDetails",
					as: "od",
					cond: { $eq: [ "$$od.Product.CategoryName", "Confections" ] }
				}
			}
		}
	},
	{
		$addFields: {
			ConfectionsSale97: {
				$sum: "$filteredOrderDetails.Value"
			},
			CustomerID: "$Customer.CustomerID",
			CompanyName: "$Customer.CompanyName"
		}
	},
	{
		$match: {
			ConfectionsSale97: { $gt: 0 }
		}
	},
	{
		$group: {
				_id: "$CustomerID",
				CustomerID: { $first: "$CustomerID" },
				CompanyName: { $first: "$Customer.CompanyName" },
				ConfectionsSale97: { $sum: "$ConfectionsSale97" }
		}
	},
	{
		$project: {
			_id: 0,
			CustomerID: 1,
			CompanyName: 1,
			ConfectionsSale97: { $round: ["$ConfectionsSale97", 2] }
		}
	}
])

// Kolekcja CustomerInfo
db.CustomerInfo.aggregate([
	{
		$addFields: {
			Orders97: {
				$filter: {
					input: "$Orders",
					as: "od",
					cond: { $eq: [ {$year: "$$od.Dates.OrderDate"}, 1997 ] }
					}
			}
		}
	},
	{
		$unwind: "$Orders97"
	},
	{
		$addFields: {
			Confections97: {
				$filter: {
					input: "$Orders97.OrderDetails",
					as: "od",
					cond: { $eq: [ "$$od.Product.CategoryName", "Confections" ] }
				}
			}
		}
	},
	{
		$match: {
			Confections97: { $ne: [], $exists: true }
		 }
	},
	{
		$unwind: "$Confections97"
	},
	{
		$group: {
			_id: "$CustomerID",
			CustomerID: { $first: "$CustomerID" },
			CompanyName: { $first: "$CompanyName"},
			ConfectionsSale97: { $sum: "$Confections97.Value" }
		}
	},
		{
  		$project: {
  			_id: 0,
  			CustomerID: 1,
  			CompanyName: 1,
  			ConfectionsSale97: { $round: ["$ConfectionsSale97", 2] }
  		}
  	}
])
```

# d)

Napisz polecenie/zapytanie:  Dla każdego klienta poaje wartość sprzedaży z podziałem na lata i miesiące
Spróbuj napisać to zapytanie wykorzystując
	- oryginalne kolekcje (`customers, orders, orderdertails, products, categories`)
	- kolekcję `OrderInfo`
	- kolekcję `CustomerInfo`

- porównaj zapytania/polecenia/wyniki

```js
[  
  {  
    "_id": 
    
    "CustomerID": ... identyfikator klienta
    "CompanyName": ... nazwa klienta

	"Sale": [ ... tablica zawierająca inf o sprzedazy
	    {
            "Year":  ....
            "Month": ....
            "Total": ...	    
	    }
	    ...
	]
  }		  
]  
```

Kod:


```js

// Oryginalne kolekcje
db.orders.aggregate([
  {
    $lookup: {
        from: "orderdetails",
        localField: "OrderID",
        foreignField: "OrderID",
        as: "Details"
    }
  },
  {
    $unwind: "$Details"
  },
  {
    $lookup: {
        from: "products",
        localField: "Details.ProductID",
        foreignField: "ProductID",
        as: "Product"
    }
  },
  {
    $unwind: "$Product"
  },
  {
    $lookup: {
        from: "customers",
        localField: "CustomerID",
        foreignField: "CustomerID",
        as: "Customer"
    }
  },
  {
    $unwind: "$Customer"
  },
  {
    $addFields: {
        Year: { $year: "$OrderDate" },
        Month: { $month: "$OrderDate" },

        Value: {
          $multiply: [
          "$Details.UnitPrice",
          "$Details.Quantity",
                    {$subtract: [1, "$Details.Discount"] }
          ]
        }
    }
  },
  {
    $group: {
    _id: {
    CustomerID: "$Customer.CustomerID",
    Year: "$Year",
    Month: "$Month"
          },
    CompanyName: { $first: "$Customer.CompanyName" },
    Total: { $sum: "$Value" }
    }
  },
  {
    $group: {
    _id: "$_id.CustomerID",
    CustomerID: { $first: "$_id.CustomerID" },
    CompanyName: { $first: "$CompanyName" },
    Sale: {
      $push: {
        Year: "$_id.Year",
        Month: "$_id.Month",
        Total: { $round: ["$Total", 2] }
      }
    }
    }
  }
])

// OrdersInfo

db.OrdersInfo.aggregate([
  {
    $project: {
      customerID: "$Customer.CustomerID",
      companyName: "$Customer.CompanyName",
      year: { $year: "$Dates.OrderDate" },
      month: { $month: "$Dates.OrderDate" },
      orderTotal: "$OrderTotal"
    }
  },
  {
    $group: {
      _id: {
        customerID: "$customerID",
        year: "$year",
        month: "$month"
      },
      companyName: { $first: "$companyName" },
      Total: { $sum: "$orderTotal" }
    }
  },
  {
    $group: {
      _id: "$_id.customerID",
      CustomerID: { $first: "$_id.customerID"},
      CompanyName: { $first: "$companyName" },
      Sale: {
        $push: {
          Year: "$_id.year",
          Month: "$_id.month",
          Total: {$round: ["$Total", 2]}
        }
      }
    }
  }
])

//CustomerInfo

db.CustomerInfo.aggregate([
  { $unwind: "$Orders" },
  {
    $project: {
      CustomerID: 1,
      CompanyName: 1,
      Year: { $year: "$Orders.Dates.OrderDate" },
      Month: { $month: "$Orders.Dates.OrderDate" },
      OrderTotal: "$Orders.OrderTotal"
    }
  },
  {
    $group: {
      _id: {
        CustomerID: "$CustomerID",
        Year: "$Year",
        Month: "$Month"
      },
      CompanyName: { $first: "$CompanyName" },
      TotalSales: { $sum: "$OrderTotal" }
    }
  },
  {
    $group: {
      _id: "$_id.CustomerID",
      CustomerID: {$first: "$_id.CustomerID" },
      CompanyName: { $first: "$CompanyName" },
      MonthlySales: {
        $push: {
          Year: "$_id.Year",
          Month: "$_id.Month",
          Total: {$round: ["$TotalSales", 2]}
        }
      }
    }
  }
])



```


# e)

Załóżmy że pojawia się nowe zamówienie dla klienta 'ALFKI',  zawierające dwa produkty 'Chai' oraz "Ikura"
- pozostałe pola w zamówieniu (ceny, liczby sztuk prod, inf o przewoźniku itp. możesz uzupełnić wg własnego uznania)
Napisz polecenie które dodaje takie zamówienie do bazy
- aktualizując oryginalne kolekcje `orders`, `orderdetails`
- aktualizując kolekcję `OrderInfo`
- aktualizując kolekcję `CustomerInfo`

Napisz polecenie 
- aktualizując oryginalną kolekcję orderdetails`
- aktualizując kolekcję `OrderInfo`
- aktualizując kolekcję `CustomerInfo`

Kod: 
```js
// aktualizując oryginalne kolekcje `orders`, `orderdetails`

const chaiProductInfo = db.products.findOne({ ProductName: "Chai" });
const ikuraProductInfo = db.products.findOne({ ProductName: "Ikura" });

const lastOrderDoc = db.orders.find().sort({ OrderID: -1 }).limit(1).toArray()[0];
const newOrderID = lastOrderDoc.OrderID + 1;

db.orders.find()

const orderResult = db.orders.insertOne({
	OrderID: newOrderID,
	CustomerID: "ALFKI",
	EmployeeID: 3,
	OrderDate: new Date(2024, 4, 11),
	RequiredDate: new Date(2025, 5, 11),
	ShippedDate: new Date(2025, 4, 8),
	ShipVia: 2,
	Freight: 15.80,
	ShipName: "Myslovitz",
	ShipAddress: "Szkolna 17",
	ShipCity: "Białystok",
	ShipRegion: null,
	ShipPostalCode: "23-535",
	ShipCountry: "Poland"
})

db.orderdetails.insertMany([{
	Discount: 0,
	OrderID: db.orders.find().sort({ OrderID: -1 }).limit(1).toArray()[0],
	ProductID: chaiProductInfo.ProductID,
	Quantity: 5,
	UnitPrice: chaiProductInfo.UnitPrice
},
{
	Discount: 0,
	OrderID: db.orders.find().sort({ OrderID: -1 }).limit(1).toArray()[0],
	ProductID: ikuraProductInfo.ProductID,
	Quantity: 5,
	UnitPrice: ikuraProductInfo.UnitPrice
}])

// aktualizując kolekcję `OrderInfo`
const alfki = db.customers.findOne({ CustomerID: "ALFKI" });
const employee = db.employees.findOne({ EmployeeID: 4 });
const newOrderID = db.OrdersInfo.find().sort({_id: -1}).limit(1).toArray()[0].OrderID + 1;

const chaiValue = chaiProductInfo.UnitPrice * 5;
const ikuraValue = ikuraProductInfo.UnitPrice * 10;
const orderTotal = chaiValue + ikuraValue;

db.OrdersInfo.find().sort({OrderID: -1})

db.OrdersInfo.insertOne({
	Customer: {
		CustomerID: alfki.CustomerID,
		CompanyName: alfki.CompanyName,
		City: alfki.City,
		Country: alfki.Country
	},
	Dates: {
		OrderDate: new Date(2024, 4, 11),
		RequiredDate: new Date(2025, 5, 11)
	},
	Employee: {
		EmployeeID: employee.EmployeeID,
		FirstName: employee.FirstName,
		LastName: employee.LastName,
		Title: employee.Title
	},
	Freight: 312.3,
	OrderDetails: [
		{
			UnitPrice: chaiProductInfo.UnitPrice,
			Quantity: 5,
			Discount: 0,
			Value: chaiValue,
			Product: {
				ProductID: chaiProductInfo.ProductID,
				ProductName: chaiProductInfo.ProductName,
				QuantityPerUnit: chaiProductInfo.QuantityPerUnit,
				CategoryID: chaiProductInfo.CategoryID,
				CategoryName: db.categories.find( { CategoryID: chaiProductInfo.CategoryID } ).toArray()[0].CategoryName
			}
		},
		{
			UnitPrice: ikuraProductInfo.UnitPrice,
			Quantity: 10,
			Discount: 0,
			Value: ikuraValue,
			Product: {
				ProductID: ikuraProductInfo.ProductID,
				ProductName: ikuraProductInfo.ProductName,
				QuantityPerUnit: ikuraProductInfo.QuantityPerUnit,
				CategoryID: ikuraProductInfo.CategoryID,
				CategoryName: db.categories.find( { CategoryID: ikuraProductInfo.CategoryID } ).toArray()[0].CategoryName
			}
		}
	],
	OrderID: newOrderID,
	OrderTotal: orderTotal,
	Shipment: {
		Shipper: {
			ShipperID: 30,
			CompanyName: "Kult"
		},
		ShipName: "Brooklyn",
		ShipAddress: "Bruhhh",
		ShipCity: "Kraków",
		ShipCountry: "Poland",
	}
})
```

# f)

Napisz polecenie które modyfikuje zamówienie dodane w pkt e)  zwiększając zniżkę  o 5% (dla każdej pozycji tego zamówienia) 

Napisz polecenie 
- aktualizując oryginalną kolekcję `orderdetails`
- aktualizując kolekcję `OrderInfo`
- aktualizując kolekcję `CustomerInfo`



UWAGA:
W raporcie należy zamieścić kod poleceń oraz uzyskany rezultat, np wynik  polecenia `db.kolekcka.fimd().limit(2)` lub jego fragment


## Zadanie 1  - rozwiązanie

> Wyniki: 
> 
> przykłady, kod, zrzuty ekranów, komentarz ...

a)

```js
--  ...
```

b)


```js
--  ...
```

....

# Zadanie 2 - modelowanie danych


Zaproponuj strukturę bazy danych dla wybranego/przykładowego zagadnienia/problemu

Należy wybrać jedno zagadnienie/problem (A lub B lub C)

Przykład A
- Wykładowcy, przedmioty, studenci, oceny
	- Wykładowcy prowadzą zajęcia z poszczególnych przedmiotów
	- Studenci uczęszczają na zajęcia
	- Wykładowcy wystawiają oceny studentom
	- Studenci oceniają zajęcia

Przykład B
- Firmy, wycieczki, osoby
	- Firmy organizują wycieczki
	- Osoby rezerwują miejsca/wykupują bilety
	- Osoby oceniają wycieczki

Przykład C
- Własny przykład o podobnym stopniu złożoności

a) Zaproponuj  różne warianty struktury bazy danych i dokumentów w poszczególnych kolekcjach oraz przeprowadzić dyskusję każdego wariantu (wskazać wady i zalety każdego z wariantów)
- zdefiniuj schemat/reguły walidacji danych
- wykorzystaj referencje
- dokumenty zagnieżdżone
- tablice

b) Kolekcje należy wypełnić przykładowymi danymi

c) W kontekście zaprezentowania wad/zalet należy zaprezentować kilka przykładów/zapytań/operacji oraz dla których dedykowany jest dany wariant

W sprawozdaniu należy zamieścić przykładowe dokumenty w formacie JSON ( pkt a) i b)), oraz kod zapytań/operacji (pkt c)), wraz z odpowiednim komentarzem opisującym strukturę dokumentów oraz polecenia ilustrujące wykonanie przykładowych operacji na danych

Do sprawozdania należy kompletny zrzut wykonanych/przygotowanych baz danych (taki zrzut można wykonać np. za pomocą poleceń `mongoexport`, `mongdump` …) oraz plik z kodem operacji/zapytań w wersji źródłowej (np. plik .js, np. plik .md ), załącznik powinien mieć format zip

## Zadanie 2  - rozwiązanie

> Wyniki: 
> 
> przykłady, kod, zrzuty ekranów, komentarz ...

```js
--  ...
```

---

Punktacja:

|         |     |
| ------- | --- |
| zadanie | pkt |
| 1       | 1   |
| 2       | 1   |
| razem   | 2   |



