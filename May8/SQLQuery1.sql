select * from Products

create or alter procedure proc_GetCompCount(@pcpu nvarchar(20),@count int out)
AS
begin 
	set @count = (SELECT count(*) from Products
	WHERE try_cast(JSON_VALUE(details,'$.spec.cpu') as nvarchar(20)) = @pcpu)
end 

begin 
	declare @cnt int
	exec proc_GetCompCount 'i7',@cnt out
	print concat('The number of systems is ',@cnt)
end

create table People(
id int PRIMARY KEY, 
name nvarchar(20),
age int)

create table BulkInsertLog(
	LogId int identity(1,1) PRIMARY KEY,
	filepath nvarchar(500),
	status nvarchar(50) constraint chk_status CHECK(status in ('Success','Failure')),
	message nvarchar(1000),
	InsertedDate DateTime default GetDate())


create or alter procedure proc_BulkInsert(@filepath nvarchar(500))
as
begin
	BEGIN try
		declare @insertquery nvarchar(max)
		set @insertquery = 'BULK INSERT People from '''+@filepath+'''
		with(
		FIRSTROW = 2,
		FIELDTERMINATOR = '','',
		ROWTERMINATOR = ''\n'')'
		exec sp_executesql @insertquery 

		insert into BulkInsertLog(filepath,status,message)
		VALUES(@filepath,'Success','Bulk Insert Completed')
	end try 
	BEGIN catch
		insert into BulkInsertLog(filepath,status,message)
		VALUES(@filepath,'Failure',Error_Message())
	END catch
end 
		

exec proc_BulkInsert 'C:\Users\arajesh\Desktop\GensparkTraining\Day3\Data.csv'


select * from BulkInsertLog

WITH cteAuthors AS (
    SELECT au_id AS Author_Id, au_fname AS Author_Name
    FROM Authors
)
SELECT * FROM cteAuthors;
UPDATE cteAuthors
SET Author_Name = 'Aditi'
WHERE Author_Id = '722-51-5454';
SELECT * FROM cteAuthors;

declare @page int=1, @pageSize int=10
create or alter procedure proc_GetBooks(@ppage int, @ppageSize int)
as
begin 
	with PaginatedBooks as
	(select title_id,title,price,ROW_NUMBER() OVER (ORDER BY price DESC) as RowNum
	from titles)
	select * from PaginatedBooks where RowNum BETWEEN (@ppage-1)*(@ppageSize+1) AND (@ppage)*(@ppageSize)
end 

exec proc_GetBooks 1,10

select title_id,title,price
from titles
order by price desc 
offset 10 rows fetch next 10 rows only

select title_id,title,price
from titles
order by price desc 
offset 10 rows

create function fn_CalculateTax(@baseprice float, @tax float)
returns float 
as 
begin 
	return (@baseprice+(@baseprice*@tax/100))

end 

select title_id,title,dbo.fn_CalculateTax(price,12) as Final_Price 
from titles

create function fn_TableSample(@minprice float)
returns table 
as

	return select title_id,title,price from titles where price >= @minprice 

select * from dbo.fn_TableSample(10);

create function fn_TableSampleOld(@minprice float)
returns @Result table(Book_id nvarchar(50), Book_Name nvarchar(100),Price float)
as
begin 
	insert into @Result select title_id,title,price from titles where price >= @minprice
	return 
end 

select * from fn_TableSampleOld(10);