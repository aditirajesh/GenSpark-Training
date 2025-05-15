select * from rental_log;

-- confirming if record appears in secondary after procedure execution in primary 
SELECT * FROM rental_log ORDER BY log_id DESC LIMIT 1;

select * from customer;