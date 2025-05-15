--1) 1. Create a stored procedure to encrypt a given text
CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE OR REPLACE PROCEDURE sp_encrypt_text(input_text TEXT,OUT hashed_text BYTEA)
LANGUAGE plpgsql 
AS $$
DECLARE 
	salt TEXT := 'hello123';
BEGIN 
	hashed_text = pgp_sym_encrypt(input_text,salt);
END;
$$;


--2)Create a stored procedure to compare two encrypted texts
CREATE OR REPLACE PROCEDURE sp_compare_encrypted(first_text BYTEA, second_text BYTEA)
LANGUAGE plpgsql 
AS $$
DECLARE
	decrypted_text1 TEXT;
	decrypted_text2 TEXT;
	salt TEXT := 'hello123';
BEGIN 
	decrypted_text1 = pgp_sym_decrypt(first_text,salt);
	decrypted_text2 = pgp_sym_decrypt(second_text,salt);

	IF decrypted_text1 = decrypted_text2 THEN 
		RAISE NOTICE 'The two texts are the same.';
	ELSE 
		RAISE NOTICE 'The two texts are not the same.';
	END IF;
END;
$$;

--do block to see if comparison works properly.
DO $$
DECLARE 
	hashed_text1 BYTEA;
	hashed_text2 BYTEA;
	hashed_text3 BYTEA;
BEGIN 
	CALL sp_encrypt_text('arajesh@presidio.com',hashed_text1);
	CALL sp_encrypt_text('arajesh@presidio.com',hashed_text2);
	CALL sp_encrypt_text('aditir2607@gmail.com',hashed_text3);

	CALL sp_compare_encrypted(hashed_text1,hashed_text2);
	CALL sp_compare_encrypted(hashed_text1,hashed_text3);
END;
$$;


--3)Create a stored procedure to partially mask a given text

CREATE OR REPLACE PROCEDURE sp_mask_text(input_text TEXT,OUT masked_text TEXT)
LANGUAGE plpgsql 
AS $$
DECLARE 
	visible_char INT := 2; --2 from the start, 2 from the end.
BEGIN 
	masked_text = CONCAT(
						SUBSTRING(input_text FROM 1 FOR visible_char),
						REPEAT('*',LENGTH(input_text)-(2*visible_char)),
						SUBSTRING(input_text FROM LENGTH(input_text)-visible_char+1)
						);
	RAISE NOTICE 'The masked text is: %',masked_text;
END;
$$;

DO $$
DECLARE 
	out_masked_text TEXT;
BEGIN 
	CALL sp_mask_text('jon.doe@example.com',out_masked_text);
END;
$$;

--4) Create a procedure to insert into customer with encrypted email and masked name
CREATE TABLE Customer(
	customer_id SERIAL PRIMARY KEY,
	cust_name TEXT,
	email BYTEA
);

CREATE OR REPLACE PROCEDURE sp_InsertCustomer(cust_name TEXT,email TEXT)
LANGUAGE plpgsql 
AS $$
DECLARE 
	masked_custname TEXT;
	hashed_email BYTEA;
BEGIN 
	CALL sp_encrypt_text(email,hashed_email);
	CALL sp_mask_text(cust_name,masked_custname);
	INSERT INTO Customer(cust_name,email) VALUES(masked_custname,hashed_email);
END;
$$;

CALL sp_InsertCustomer('Srujana Srinivasan','srujana2607@gmail.com');

select * from customer;

--5)Create a procedure to fetch and display masked first_name and decrypted email for all customers
CREATE OR REPLACE PROCEDURE sp_FetchCustomer()
LANGUAGE plpgsql
AS $$
DECLARE 
	rec record;
	cur_getcustomer CURSOR FOR
		(SELECT * FROM Customer);
	decrypted_mail TEXT;
BEGIN 
	open cur_getcustomer;
	loop
		FETCH NEXT FROM cur_getcustomer INTO rec;
		EXIT WHEN NOT FOUND;
		
		decrypted_mail := pgp_sym_decrypt(rec.email,'hello123');
		RAISE NOTICE 'Customer ID: %, Customer name: %, Email: %',rec.customer_id,rec.cust_name,decrypted_mail;
	end loop;
END;
$$;

CALL sp_FetchCustomer();
		

