-- 1)List all films with their length and rental rate, sorted by length descending.
SELECT title,length,rental_rate from film
ORDER BY length DESC

-- 2)Find the top 5 customers who have rented the most films.
SELECT customer_id, COUNT(rental_id) as RentedFilms
from rental
GROUP BY customer_id
ORDER BY 2 DESC 
LIMIT 5;

-- 3) Display all films that have never been rented.
select f.film_id,f.title
from film f
LEFT JOIN inventory i 
ON f.film_id = i.film_id
LEFT JOIN rental r 
ON r.inventory_id = i.inventory_id
where r.rental_id IS NULL
ORDER BY f.film_id

-- 4) List all actors who appeared in the film ‘Academy Dinosaur’.

SELECT a.actor_id, CONCAT(a.first_name ,a.last_name) AS ActorName
FROM film f
JOIN film_actor fa ON f.film_id = fa.film_id
JOIN actor a ON fa.actor_id = a.actor_id
WHERE f.title = 'Academy Dinosaur'
 

--5) List each customer along with the total number of rentals they made and the total amount paid.
SELECT customer_id, COUNT(rental_id) as TotalRentals, SUM(amount) as TotalAmount
from payment
GROUP BY customer_id
ORDER BY customer_id

--6) Using a CTE, show the top 3 rented movies by number of rentals.
WITH TopRentedMovies as (
SELECT f.film_id,COUNT(rental_id) as RentCount
from film f
INNER JOIN inventory i
ON f.film_id = i.film_id
INNER JOIN rental r 
ON i.inventory_id = r.inventory_id
GROUP BY f.film_id
ORDER BY 2 DESC
LIMIT 3
)

SELECT * FROM TopRentedMovies

--7) Find customers who have rented more than the average number of films.
with GreaterRentals as (
	select c.customer_id, COUNT(r.rental_id) as RentCount
	from customer c 
	INNER JOIN rental r 
	ON c.customer_id = r.customer_id
	GROUP BY c.customer_id
	HAVING COUNT(r.rental_id) > (
		SELECT AVG(rental_count) FROM (
			SELECT count(rental_id) as rental_count from rental 
			GROUP BY customer_id
		) as avg_table
	)
	ORDER BY 2 DESC
)

SELECT * from GreaterRentals

with Rentals AS(
	Select customer_id,COUNT(rental_id) as RentCount
	from rental 
	GROUP BY customer_id
	HAVING COUNT(rental_id) > (
		Select AVG(rentcount) FROM 
		( SELECT COUNT(rental_id) as rentcount from rental
		GROUP BY customer_id) as avg_table
	)
)

select * from Rentals
--8) Write a function that returns the total number of rentals for a given customer ID.
create or replace function fn_TotalRentals(customerid INT)
returns int as $$ 
DECLARE 
	total int;

BEGIN
	SELECT COUNT(rental_id) INTO total
	FROM rental
	WHERE customer_id = customerid;
	
	RETURN total;
END;
$$ LANGUAGE plpgsql;

SELECT fn_TotalRentals(1) as totalrental;

--9) Write a stored procedure that updates the rental rate of a film by film ID and new rate.
create or replace procedure proc_UpdateRentalRate(filmid INT, newrate NUMERIC)
language plpgsql
AS $$
BEGIN
	UPDATE film 
	SET rental_Rate = newrate WHERE film_id = filmid;
END;
$$;

CALL proc_UpdateRentalRate(1,19)

select * from film where film_id = 1

--10)Write a procedure to list overdue rentals (return date is NULL and rental date older than 7 days).
create or replace procedure proc_OverdueRentals()
language plpgsql
AS $$
DECLARE 
	r rental%ROWTYPE;
BEGIN 
	FOR r IN
		SELECT * from rental
		WHERE return_date IS NULL 
		AND EXTRACT(day from AGE(NOW(),rental_date)) > 7
	LOOP 
		RAISE NOTICE 'Overdue rental ID:%, Date: %',r.rental_id,r.rental_date;
	END LOOP;
END;
$$

CALL proc_OverdueRentals(); 

