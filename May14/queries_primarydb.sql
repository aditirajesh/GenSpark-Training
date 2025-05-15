--create a table in primary

CREATE TABLE rental_log (
    log_id SERIAL PRIMARY KEY,
    rental_time TIMESTAMP,
    customer_id INT,
    film_id INT,
    amount NUMERIC,
    logged_on TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

select * from rental_log;

--stored procedure to insert new rental_log entry 

CREATE OR REPLACE PROCEDURE proc_InsertRentalLog(p_custid INT,p_filmid INT,p_amount NUMERIC)
LANGUAGE plpgsql 
AS $$
BEGIN 
	BEGIN
		INSERT INTO rental_log(rental_time,customer_id,film_id,amount) 
		VALUES (NOW(),p_custid,p_filmid,p_amount);

	EXCEPTION WHEN OTHERS THEN 
		RAISE NOTICE 'Transaction failed: %',sqlerrm;
	END;
END;
$$;

CALL proc_InsertRentalLog(1, 100, 4.99);
