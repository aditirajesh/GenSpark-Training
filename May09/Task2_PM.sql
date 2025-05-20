-- 1)Write a cursor that loops through all films and prints titles longer than 120 minutes.

select * from film
select * from film_category

SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_type = 'BASE TABLE';


DO $$
DECLARE 
	cur_LongTitles CURSOR FOR
		(Select title from film WHERE length > 120);
	movietitle TEXT;
BEGIN 
	open cur_LongTitles ;

	LOOP
		FETCH NEXT from cur_LongTitles INTO movietitle;
		EXIT WHEN NOT FOUND;
		RAISE NOTICE '%',movietitle;
	END LOOP;

	close cur_LongTitles;
END;
$$;

-- 2)Create a cursor that iterates through all customers and counts how many rentals each made.
DO $$
DECLARE 
	cur_RentalsCount CURSOR FOR(
		SELECT c.customer_id,COUNT(r.rental_id) as RentCount
		FROM customer c
		LEFT JOIN rental r 
		ON c.customer_id = r.customer_id
		GROUP BY c.customer_id
		ORDER by 1
	);
	customerid INT;
	rentalcount INT;

BEGIN 
	open cur_RentalsCount;
	LOOP
		FETCH NEXT FROM cur_RentalsCount INTO customerid,rentalcount;
		EXIT WHEN NOT FOUND;

		RAISE NOTICE 'CustomerID: %, RentCount: %',customerid,rentalcount;
	END LOOP;

	close cur_RentalsCount;
END;
$$
select * from rental;
SELECT * from film;

--3) Using a cursor, update rental rates: Increase rental rate by $1 for films with less than 5 rentals.
DO $$
DECLARE 
	cur_UpdateRentalRate CURSOR FOR
	(SELECT f.film_id,COUNT(r.rental_id) as RentCount
	FROM film f 
	INNER JOIN inventory i 
	ON f.film_id = i.film_id
	INNER JOIN rental r
	ON i.inventory_id = r.inventory_id
	GROUP BY f.film_id
	HAVING COUNT(r.rental_id) < 5);

	filmid INT;
	rentcount INT;
BEGIN 
	open cur_UpdateRentalRate;
	LOOP
		FETCH NEXT from cur_UpdateRentalRate into filmid,rentcount;
		EXIT WHEN NOT FOUND;

		UPDATE film 
		SET rental_rate = rental_rate+1
		WHERE film_id = filmid;

		RAISE NOTICE 'Updated filmID:% RentCount:% ',filmid,rentcount;

	END LOOP;
	CLOSE cur_UpdateRentalRate;
END;
$$

--4)Create a function using a cursor that collects titles of all films from a particular category.
create or replace function fn_GetTitles(categoryid INT)
returns VOID 
AS $$

DECLARE 
	cur_films CURSOR FOR 
		SELECT f.film_id,f.title
		from film f
		INNER JOIN film_category fc
		ON f.film_id = fc.film_id
		WHERE fc.category_id = categoryid;

	filmid INT;
	filmtitle TEXT;
	
BEGIN 
	open cur_films;
	LOOP
		FETCH NEXT FROM cur_films INTO filmid,filmtitle;
		EXIT WHEN NOT FOUND;

		RAISE NOTICE 'FilmID:% Title:%',filmid,filmtitle;
	END LOOP;
	close cur_films;
END;
$$ language plpgsql;

SELECT fn_GetTitles(6)	;

select * from film
select * from rental
select * from inventory
select COUNT(film_id) from film
--5)Loop through all stores and count how many distinct films are available in each store using a cursor.
DO $$
DECLARE 
	cur_CountFilms CURSOR FOR
		SELECT s.store_id,COUNT(DISTINCT i.film_id) as FilmCount
		FROM store s 
		INNER JOIN inventory i 
		ON s.store_id = i.store_id
		GROUP BY s.store_id;
		
	storeid INT;
	filmcount INT;

BEGIN 
	open cur_CountFilms;
	LOOP
		FETCH NEXT FROM cur_CountFilms INTO storeid,filmcount;
		EXIT WHEN NOT FOUND;

		RAISE NOTICE 'StoreID: %,FilmCount: %',storeid,filmcount;
	END LOOP;
	close cur_CountFilms;
END;
$$;

-- 6)Write a trigger that logs whenever a new customer is inserted.
CREATE TABLE CustomerLog(
	customer_id INT REFERENCES Customer(customer_id),
	insert_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	date DATE default CURRENT_DATE
)

drop table CustomerLog

create or replace function trigger_function()
RETURNS TRIGGER 
LANGUAGE plpgsql
AS $$
BEGIN 
	INSERT INTO CustomerLog(customer_id)VALUES(NEW.customer_id);
	RETURN NEW;
END;
$$;

CREATE TRIGGER trig_CustomerInsert
AFTER INSERT ON Customer 
FOR EACH ROW
EXECUTE FUNCTION trigger_function();

insert into Customer VALUES(600,1,'Emma','Watson','emmawatson@gmail.com',5,true,CURRENT_DATE,CURRENT_TIMESTAMP,1);
select * from CustomerLog;

--7)Create a trigger that prevents inserting a payment of amount 0.
create or replace function fn_PreventInsert()
RETURNS TRIGGER 
LANGUAGE PLPGSQL
AS $$
BEGIN 
	IF NEW.amount <= 0 THEN 
		RAISE EXCEPTION 'Amount cannot be zero';
	END IF;
	RETURN NEW;
END;
$$;

CREATE TRIGGER trig_insertamount
BEFORE INSERT ON Payment
FOR EACH ROW
EXECUTE FUNCTION fn_PreventInsert();

INSERT INTO Payment VALUES(18503,3,3,2128,0,NOW());

-- 8)Set up a trigger to automatically set last_update on the film table before update.
create or replace function fn_AutomateLastUpdate()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN 
	NEW.last_update := NOW();
	RETURN NEW;
END;
$$;

CREATE TRIGGER trig_Update
BEFORE UPDATE OR INSERT ON film
FOR EACH ROW 
EXECUTE FUNCTION fn_AutomateLastUpdate()

select * from film where film_id=2;
UPDATE film set title = 'Vedi Movies' where film_id=2;

--9)Create a trigger to log changes in the inventory table (insert/delete).
CREATE TABLE InventoryLog(
	log_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	inventory_id INT REFERENCES inventory(inventory_id),
	logdate DATE default CURRENT_DATE
);

CREATE or REPLACE function fn_TrackInventory()
RETURNS TRIGGER
LANGUAGE PLPGSQL
AS $$
BEGIN 
	INSERT INTO InventoryLog(inventory_id) VALUES(NEW.inventory_id);
	RETURN NEW;
END;
$$;

CREATE TRIGGER trig_InventoryChanges
AFTER INSERT OR DELETE ON Inventory 
FOR EACH ROW 
EXECUTE FUNCTION fn_TrackInventory();


insert into inventory VALUES(4582,1,2,NOW());
select * from InventoryLog;

--10)Write a trigger that ensures a rental can’t be made for a customer who owes more than $50.
select * from customer;
select * from rental;
select * from payment;
select * from film;

create or replace function fn_DueCustomers(customerid int)
RETURNS NUMERIC 
LANGUAGE plpgsql
AS $$
DECLARE 
	balance NUMERIC;
BEGIN 
	SELECT SUM(f.rental_rate)-SUM(p.amount) INTO balance
	from customer c
	INNER JOIN rental r 
	ON c.customer_id = r.customer_id
	INNER JOIN payment p
	ON p.rental_id = r.rental_id
	INNER JOIN inventory i
	ON r.inventory_id = i.inventory_id
	INNER JOIN film f 
	ON i.film_id = f.film_id
	WHERE c.customer_id = customerid;

	return balance;
END;
$$;

create or replace function fn_TriggerInvalidRental()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN 
	IF fn_DueCustomers(NEW.customer_id) > 50 THEN
		RAISE EXCEPTION 'Cannot rent for this customer, due more than 50';
	END IF;
	RETURN NEW;
END;
$$;

CREATE TRIGGER trig_InvalidRental 
BEFORE INSERT OR UPDATE ON rental
FOR EACH ROW 
EXECUTE FUNCTION fn_TriggerInvalidRental()


-- 11)Write a transaction that inserts a customer and an initial rental in one atomic operation.
BEGIN;
INSERT INTO customer VALUES(601,1,'Aditi','Rajesh','aditirajesh@gmail.com',530,true,CURRENT_DATE,NOW(),1);
INSERT INTO rental VALUES(16050,NOW(),1523,601,CURRENT_DATE+ INTERVAL '10 days',2,NOW());
COMMIT;

--12)Simulate a failure in a multi-step transaction (update film + insert into inventory) and roll back.
select * from film
select * from inventory

BEGIN;
UPDATE film SET title='Interstellar' WHERE film_id=8;
INSERT INTO inventory VALUES(4583,8,1,NOW());
ROLLBACK;

SELECT * from inventory where inventory_id=4583;
SELECT * from film where film_id=8;

--13)Create a transaction that transfers an inventory item from one store to another.
BEGIN;
UPDATE inventory 
SET store_id=2 
WHERE inventory_id=1 AND store_id=1;

COMMIT;

--14)Demonstrate SAVEPOINT and ROLLBACK TO SAVEPOINT by updating payment amounts, then undoing one.
BEGIN;
SAVEPOINT before_paymentupdate;
UPDATE payment
SET amount=10.00 WHERE payment_id=17503;
ROLLBACK TO before_paymentupdate;

select amount from payment where payment_id=17503

select * from payment

--15)Write a transaction that deletes a customer and all associated rentals and payments, ensuring atomicity.
create or replace function fn_massdelete(customerid INT)
RETURNS VOID
LANGUAGE plpgsql
AS $$
BEGIN 
	DELETE FROM payment WHERE customer_id=customerid;
	DELETE FROM rental WHERE customer_id=customerid;
	DELETE FROM customer WHERE customer_id=customerid;
END;
$$

BEGIN;
SELECT fn_massdelete(1);
COMMIT;

select * from customer where customer_id=1;









