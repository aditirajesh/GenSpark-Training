use master 
go 

-- 1) List all orders with the customer name and the employee who handled the order.
SELECT OrderId, ContactName as CustomerName, CONCAT(FirstName, ' ',LastName) as EmployeeName
from Orders
INNER JOIN Customers
ON Orders.CustomerID = Customers.CustomerID
INNER JOIN Employees 
ON Orders.EmployeeID = Employees.EmployeeID

-- 2) Get a list of products along with their category and supplier name.
SELECT ProductId, ProductName, CategoryName as Category, CompanyName as Supplier
from Products P
INNER JOIN Categories C
ON C.CategoryID = P.CategoryID
INNER JOIN Suppliers S 
ON P.SupplierID = S.SupplierID

-- 3) Show all orders and the products included in each order with quantity and unit price.
SELECT Ord.OrderID as OrderId, Ord.ProductID ProductID, P.ProductName ProductName, Ord.UnitPrice UnitPrice, Ord.Quantity Quantity
from OrderDetails Ord 
INNER JOIN Products P
ON Ord.ProductID = P.ProductID

-- 4) List employees who report to other employees (manager-subordinate relationship).
SELECT emps.EmployeeID, CONCAT(emps.FirstName,' ',emps.LastName) AS EmployeeName, CONCAT(mans.FirstName,' ',mans.LastName) AS ManagerName
from Employees emps 
INNER JOIN Employees mans
ON emps.ReportsTo = mans.EmployeeID

-- 5) Display each customer and their total order count.
SELECT C.CustomerID, COUNT(O.OrderID) as TotalOrders
from Customers C
INNER JOIN Orders O
ON O.CustomerID = C.CustomerID
GROUP BY C.CustomerID

-- 6) Find the average unit price of products per category.
SELECT CategoryName, AVG(UnitPrice) as AveragePrice
from Products
INNER JOIN Categories
ON Products.CategoryID = Categories.CategoryID
GROUP BY CategoryName

-- 7) List customers where the contact title starts with 'Owner'.
SELECT * From Customers WHERE ContactTitle LIKE 'Owner%'

-- 8) Show the top 5 most expensive products.
SELECT TOP 5 * From Products
ORDER BY UnitPrice DESC

-- 9)Return the total sales amount (quantity × unit price) per order.
SELECT OrderID, SUM(Quantity*UnitPrice) AS TotalSalesAmount
from OrderDetails 
GROUP BY OrderID


-- 10) Create a stored procedure that returns all orders for a given customer ID.
create or alter procedure proc_GetCustomerOrders(@pcustomerid nchar(5))
as
begin 
	SELECT * from Orders
	WHERE CustomerID = @pcustomerid
end

exec proc_GetCustomerOrders 'AROUT'

select * from Products

-- 11) Write a stored procedure that inserts a new product.
create or alter procedure proc_InsertProduct(@name nvarchar(40),@supid int, @caid int, @unitprice money)
as
begin 
	INSERT INTO Products(ProductName,SupplierID,CategoryID,UnitPrice)
	VALUES(@name,@supid,@caid,@unitprice)
end 

exec proc_InsertProduct 'Monster Energy Drink',12,1,'50.00'


-- 12)Create a stored procedure that returns total sales per employee.
create or alter procedure proc_GetEmployeeTotalSales(@empid int)
as
begin 
	select EmployeeID, COUNT(OrderID) as TotalSales
	from Orders
	GROUP BY EmployeeID
end 

exec proc_GetEmployeeTotalSales 1

SELECT compatibility_level
FROM sys.databases
WHERE name = 'master';

-- 13) Use a CTE to rank products by unit price within each category.
;WITH RankProducts AS (
    SELECT ProductID, CategoryID, UnitPrice,
	RANK() OVER (PARTITION BY CategoryID ORDER BY UnitPrice DESC) as ProductRank
	from Products
)
SELECT * FROM RankProducts;

-- 14)Create a CTE to calculate total revenue per product and filter products with revenue > 10,000
WITH ProductRevenue AS (
	Select ProductID, TotalRevenue FROM(
	Select ProductID, SUM(Quantity*UnitPrice) as TotalRevenue
	FROM OrderDetails
	GROUP BY ProductID
	) as ProductRevenue
	WHERE TotalRevenue > 10000
)

select * from ProductRevenue

-- 15)Use a CTE with recursion to display employee hierarchy.

WITH EmployeeHirearchy(EmpID,EmpName,ManagerID,EmployeeLevel)
AS(
	SELECT EmployeeID, CONCAT(FirstName,' ',LastName) as EmployeeName, ReportsTo as ManagerID, 1
	FROM Employees 
	WHERE ReportsTo IS NULL
	UNION ALL
	SELECT E.EmployeeID, CONCAT(E.FirstName,' ',E.LastName) as EmployeeName, E.ReportsTo as ManagerID, CTE.EmployeeLevel+1
	FROM Employees e 
	INNER JOIN EmployeeHirearchy CTE
	ON E.ReportsTo = CTE.EmpID
)

SELECT * FROM EmployeeHirearchy