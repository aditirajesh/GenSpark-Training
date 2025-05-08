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

create or alter procedure proc_BulkInsertPeople(@filepath nvarchar(500))
as
begin 
	declare @insertquery nvarchar(max)
	set @insertquery = 'BULK INSERT People from '''+@filepath+''',
	WITH(
	FIRSTROW = 2,
	FIELDTERMINATOR = '','',
	ROWTERMINATOR = ''\n'')'
	exec sp_executesql @insertquery 
end 

exec proc_BulkInsertPeople 'C:\Users\arajesh\Desktop\GensparkTraining\Day3\Data.csv'

select * from People
