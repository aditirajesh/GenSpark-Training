create or replace function fn_GetSentBankTransactions(account_id int)
RETURNS TABLE(transaction_id int, receiver_id int, amount numeric, transfer_date timestamp)
AS 
$$
BEGIN 
	return query SELECT distinct "BankTransactionId","ReceiverId","AmountTransferred","TransferDate"
	from public."BankTransactions"
	WHERE "SenderId" = account_id AND "ReceiverId" IS NOT NULL;
END;
$$
LANGUAGE plpgsql;

create or replace function fn_GetReceivedBankTransactions(account_id int)
RETURNS TABLE(transaction_id int, sender_id int, amount numeric, transfer_date timestamp)
AS 
$$
BEGIN 
	return query SELECT distinct "BankTransactionId","SenderId","AmountTransferred","TransferDate"
	from public."BankTransactions"
	WHERE "ReceiverId" = account_id AND "SenderId" IS NOT NULL;
END;
$$
LANGUAGE plpgsql;

create or replace function fn_GetDeposits(account_id int)
RETURNS TABLE(transaction_id int,amount numeric, transfer_date timestamp)
AS 
$$
BEGIN 
	return query SELECT distinct "BankTransactionId","AmountTransferred","TransferDate"
	from public."BankTransactions"
	WHERE "ReceiverId" = account_id AND "SenderId" IS NULL;
END;
$$
LANGUAGE plpgsql;

create or replace function fn_GetWithdrawals(account_id int)
RETURNS TABLE(transaction_id int, amount numeric, transfer_date timestamp)
AS 
$$
BEGIN 
	return query SELECT distinct "BankTransactionId","AmountTransferred","TransferDate"
	from public."BankTransactions"
	WHERE "SenderId" = account_id AND "ReceiverId" IS NULL;
END;
$$
LANGUAGE plpgsql;