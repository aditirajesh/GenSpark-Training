-- Cursors 
-- 1)Write a cursor to list all customers and how many rentals each made. Insert these into a summary table.

SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public' AND table_type = 'BASE TABLE';

CREATE table CustomerSummary(
	customer_id INT REFERENCES customer(customer_id),
	num_rentals INT,
	PRIMARY KEY(customer_id)
)

CREATE OR REPLACE PROCEDURE proc_CustomerSummary()
LANGUAGE plpgsql 
AS $$
DECLARE 
	cur_NumRentals CURSOR FOR
		(Select customer_id, COUNT(rental_id) as NumRentals
		from rental 
		GROUP BY customer_id);
	custid INT;
	numrentals INT;
BEGIN 
	open cur_NumRentals;

	LOOP
		FETCH NEXT FROM cur_NumRentals INTO custid,numrentals;
		EXIT WHEN NOT FOUND;
		
		RAISE NOTICE 'Customer ID:%, Number of Rentals:%',custid,numrentals;
		INSERT INTO CustomerSummary VALUES(custid,numrentals);

	END LOOP;
	close cur_NumRentals;
END;
$$;

CALL proc_CustomerSummary()
select * from CustomerSummary;

--2)Using a cursor, print the titles of films in the 'Comedy' category rented more than 10 times.

CREATE OR REPLACE PROCEDURE proc_ComedyFilmsRent()
LANGUAGE plpgsql
AS $$ 
DECLARE
    cur_ComedyFilms CURSOR FOR
        SELECT f.title, COUNT(r.rental_id) AS NumRentals
        FROM film f
        INNER JOIN inventory i ON f.film_id = i.film_id
        INNER JOIN rental r ON i.inventory_id = r.inventory_id
        INNER JOIN film_category fc ON f.film_id = fc.film_id
        INNER JOIN category c ON fc.category_id = c.category_id
        WHERE c.name = 'Comedy'
        GROUP BY f.film_id
        HAVING COUNT(r.rental_id) > 10;

    filmtitle TEXT;
    numrentals INT;
BEGIN
    OPEN cur_ComedyFilms;
    RAISE NOTICE 'Comedy films with rentals > 10:';
    LOOP
        FETCH NEXT FROM cur_ComedyFilms INTO filmtitle, numrentals;
        EXIT WHEN NOT FOUND;
        RAISE NOTICE 'Title: %, Rentals: %', filmtitle, numrentals;
    END LOOP;
    CLOSE cur_ComedyFilms;
END;
$$;

CALL proc_ComedyFilmsRent()

--3)Create a cursor to go through each store and count the number of distinct films available, and insert results into a report table.
CREATE TABLE Film_report(
	report_id SERIAL PRIMARY KEY,
	store_id INT REFERENCES store(store_id),
	numfilms INT
);

CREATE OR REPLACE PROCEDURE proc_DistinctFilms()
LANGUAGE plpgsql 
AS $$
DECLARE 
	cur_FilmsByStore CURSOR FOR
		(SELECT s.store_id, COUNT(distinct f.film_id) as numfilms
		FROM store s
		INNER JOIN inventory i 
		ON s.store_id = i.store_id
		INNER JOIN film f 
		ON i.film_id = f.film_id
		GROUP BY s.store_id);
	storeid INT;
	num_films INT;

BEGIN 
	open cur_FilmsByStore;
	loop
		FETCH NEXT FROM cur_FilmsByStore INTO storeid,num_films;
		EXIT WHEN NOT FOUND;

		INSERT INTO Film_Report(store_id,numfilms) VALUES (storeid,num_films);

	end loop;
	close cur_FilmsByStore;
END;
$$;

CALL proc_DistinctFilms()

select * from Film_report;

--4)Loop through all customers who haven't rented in the last 6 months and insert their details into an inactive_customers table.

CREATE TABLE inactive_customer(
	customer_id INT REFERENCES customer(customer_id),
	last_rental timestamp
);

CREATE OR REPLACE PROCEDURE proc_InactiveCustomers()
LANGUAGE plpgsql
AS $$
DECLARE 
	cur_inactivecustomer CURSOR FOR 
		(SELECT c.customer_id, MAX(r.rental_date)
		FROM customer c
		INNER JOIN rental r 
		ON c.customer_id = r.rental_id
		GROUP BY c.customer_id
		HAVING EXTRACT(MONTH FROM AGE(CURRENT_DATE::timestamp,MAX(r.rental_date))) > 6
		);
	custid INT;
	rentaldate timestamp;

BEGIN
	open cur_inactivecustomer;
	loop
		FETCH NEXT FROM cur_inactivecustomer INTO custid,rentaldate;
		EXIT WHEN NOT FOUND;

		INSERT INTO inactive_customer VALUES(custid,rentaldate);
	end loop;
	close cur_inactivecustomer;
END;
$$;

CALL proc_InactiveCustomers()

select * from inactive_customer;

-- Transactions 
--1)Write a transaction that inserts a new customer, adds their rental, and logs the payment – all atomically.
select * from payment
CREATE OR REPLACE PROCEDURE proc_InsertCustomer(p_storeid INT,
	p_firstname TEXT, p_lastname TEXT, p_email TEXT, p_addressid INT, p_active INT,
	p_rentaldate TIMESTAMP, p_inventoryid INT, p_returndate TIMESTAMP, p_staffid INT,
	p_amount NUMERIC, p_paymentdate TIMESTAMP
	)
LANGUAGE plpgsql
AS $$
BEGIN

	BEGIN
		with new_customer as 
		(
			insert into customer (store_id, first_name, last_name, email, address_id, active) values (p_storeid, p_firstname, p_lastname, p_email, p_addressid, p_active)
			returning customer_id
		),
		new_rental as 
		(
			insert into rental (rental_date, inventory_id, customer_id, return_date, staff_id) 
			values (now(), p_inventoryid, (select customer_id from new_customer), p_returndate, p_staffid)
			returning customer_id, rental_id
		)
		
		insert into payment(customer_id,staff_id,rental_id,amount,payment_date) VALUES
		((SELECT customer_id from new_rental),p_staffid,(SELECT rental_id from new_rental),p_amount,p_paymentdate)
	EXCEPTION WHEN OTHERS THEN 
		RAISE NOTICE 'Transaction failed %',sqlerrm;
	END;
END;
$$;


--2)Simulate a transaction where one update fails (e.g., invalid rental ID), and ensure the entire transaction rolls back.

START TRANSACTION;
insert into customer VALUES (
    603, 1, 'John', 'Doe', 'john.doe@example.com', 123,
    TRUE, '2025-05-13 10:00:00', '2025-05-13 10:00:00', 1
);

insert into rental VALUES(
	16050,'2025-05-13 14:30:00', 101, 602, '2025-05-23 14:30:00', 1, '2025-05-13 14:30:00'	
);

ROLLBACK;

select * from customer where customer_id=603;

--3)Use SAVEPOINT to update multiple payment amounts. Roll back only one payment update using ROLLBACK TO SAVEPOINT.

START TRANSACTION;
UPDATE payment 
SET amount = amount - 1.00 WHERE payment_id IN (17503,17504,17505);

SAVEPOINT multi_payment_update;

UPDATE payment 
SET amount = amount + 1.00 WHERE payment_id=17506;

ROLLBACK TO multi_payment_update;

select * from payment where payment_id IN (17503,17504,17505,17506); --first 3 payments different except the last

--4)Perform a transaction that transfers inventory from one store to another (delete + insert) safely.

START TRANSACTION;
UPDATE inventory 
SET store_id=2 WHERE inventory_id=2;

COMMIT;

--5)Create a transaction that deletes a customer and all associated records (rental, payment), ensuring referential integrity.
CREATE OR REPLACE PROCEDURE proc_DeleteCustomer(p_custid INT)
LANGUAGE plpgsql 
AS $$
BEGIN --beginning of the stored procedure
	BEGIN --beginning of the transaction 
		delete from CustomerSummary WHERE customer_id=p_custid;
		delete from inactive_customer WHERE customer_id=p_custid;
		delete from payment WHERE customer_id=p_custid;
		delete from rental WHERE customer_id=p_custid;
		delete from customer WHERE customer_id=p_custid;

	--no commit statement required 
	EXCEPTION WHEN OTHERS THEN 
		RAISE NOTICE 'Transaction failed %',sqlerrm;
	END;
END;
$$;

CALL proc_DeleteCustomer(6)

select * from customer where customer_id=6
	

--Triggers
--1)Create a trigger to prevent inserting payments of zero or negative amount.

select * from payment;

create or replace function fn_PreventPaymentInsert()
RETURNS TRIGGER 
LANGUAGE plpgsql
AS $$
BEGIN
	IF NEW.amount <= 0 THEN 
		RAISE NOTICE 'INSERT INVALID, AMOUNT MUST BE GREATER THAN 0';
	END IF;
	RETURN NEW;
END;
$$;

CREATE OR REPLACE TRIGGER trig_BeforePaymentInsert
BEFORE INSERT ON payment
FOR EACH ROW
EXECUTE FUNCTION fn_PreventPaymentInsert();

insert into payment VALUES(32100,602,1,16051,0,NOW());

select *  from payment where payment_id=31200;
select * from film;
--2)Set up a trigger that automatically updates last_update on the film table when the title or rental rate is changed.
drop trigger last_updated ON film;
drop trigger trig_update ON film;

create or replace function fn_AutomaticUpdate()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE 
	col_name text := TG_ARGV[0];
	old_value text;
	new_value text;
BEGIN 
	IF col_name = 'title' or col_name = 'rental_rate' THEN
		EXECUTE FORMAT('select ($1).%I::TEXT',col_name) INTO old_value USING OLD;
		EXECUTE FORMAT('select ($1).%I::TEXT',col_name) INTO new_value USING NEW;
		IF old_value IS DISTINCT from new_value THEN 
			UPDATE film SET last_update=NOW() WHERE film_id = NEW.film_id;
		END IF;
	END IF;
	RETURN NEW;
END;
$$;

CREATE OR REPLACE TRIGGER trig_AutoUpdateFilm
AFTER UPDATE ON film
FOR EACH ROW
EXECUTE FUNCTION fn_AutomaticUpdate('title');

update film set title='Ghilli' where film_id=2;
select * from film where film_id=2;

--3)Write a trigger that inserts a log into rental_log whenever a film is rented more than 3 times in a week.
CREATE TABLE rental_log (
    log_id SERIAL PRIMARY KEY,
    film_id INT NOT NULL,
    rental_count INT NOT NULL,
    log_timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


CREATE OR REPLACE FUNCTION fn_InsertRentalLog()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    -- Check if the film has been rented more than 3 times in the current week
    IF (
        SELECT COUNT(*)
        FROM rental r
        WHERE r.inventory_id = NEW.inventory_id
          AND r.rental_date >= CURRENT_DATE - INTERVAL '7 days'
    ) > 3 THEN
        -- Insert a log entry into rental_log
        INSERT INTO rental_log (film_id, rental_count, log_timestamp)
        VALUES (
            (SELECT film_id FROM inventory WHERE inventory_id = NEW.inventory_id),
            (
                SELECT COUNT(*)
                FROM rental r
                WHERE r.inventory_id = NEW.inventory_id
                  AND r.rental_date >= CURRENT_DATE - INTERVAL '7 days'
            ),
            CURRENT_TIMESTAMP
        );
    END IF;
    RETURN NEW;
END;
$$;

CREATE OR REPLACE TRIGGER trig_FilmNumRental
AFTER INSERT on rental 
FOR EACH ROW
EXECUTE FUNCTION fn_InsertRentalLog();

INSERT INTO rental (rental_id,rental_date, inventory_id, customer_id, return_date, staff_id, last_update)
VALUES
    (16502,'2025-05-10 10:00:00', 3, 101, '2025-05-20 12:00:00', 1, CURRENT_TIMESTAMP),
    (16503,'2025-05-11 11:00:00', 3, 102, '2025-05-21 13:00:00', 2, CURRENT_TIMESTAMP),
    (16504,'2025-05-12 14:00:00', 3, 103, '2025-05-22 16:00:00', 1, CURRENT_TIMESTAMP),
    (16505,'2025-05-13 09:00:00', 3, 104, '2025-05-23 11:00:00', 2, CURRENT_TIMESTAMP);

SELECT * FROM rental_log;

DELETE FROM rental WHERE rental_id IN (16502,16503,16504,16505)
DELETE from rental_log
select * from film where film_id=4;
select * from inventory where inventory_id=4;
select * from rental where rental_id=4;

--loop through all the films and update the rental rate by +1 for teh films when rental count < 5
CREATE OR REPLACE PROCEDURE proc_UpdateRentalRate()
LANGUAGE plpgsql 
AS $$
DECLARE 
	rec record;
	cur_film_rentcount CURSOR FOR(
		SELECT f.film_id,f.rental_rate,COUNT(r.rental_id) AS rentcount
		from film f 
		LEFT JOIN inventory i 
		ON f.film_id = i.film_id
		LEFT JOIN rental r 
		ON i.inventory_id = r.inventory_id
		GROUP BY f.film_id,f.rental_rate
		HAVING COUNT(r.rental_id) < 5);

BEGIN 
	open cur_film_rentcount;
	LOOP
		FETCH NEXT FROM cur_film_rentcount INTO rec;
		EXIT WHEN NOT FOUND;

		UPDATE film SET rental_rate = rental_rate+1
		WHERE film_id = rec.film_id;

		RAISE NOTICE 'UPDATED rental_rate with film_id %. New rental rate is: %',rec.film_id,rec.rental_rate+1;
	END LOOP;
	close cur_film_rentcount;
END;
$$;

CALL proc_UpdateRentalRate();

SELECT version();
