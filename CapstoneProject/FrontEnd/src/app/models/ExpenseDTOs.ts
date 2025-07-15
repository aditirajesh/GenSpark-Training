import { ExpenseSearchModel } from "./ExpenseSearchModel";
import { PaginationQuery } from "./Pagination";

export interface ExpenseAddRequestDto {
  title: string;                
  category: string;             
  notes: string;               
  amount: number;              
  receiptBill?: File;          
  targetUsername?: string;      
  ExpenseDate?: Date | string; // Match backend property name exactly (capital E)
}


export interface ExpenseUpdateRequestDto {
  id: string;                  
  title?: string;             
  category?: string;            
  notes?: string;           
  amount?: number;           
  receipt?: File; 
  ExpenseDate?: Date | string; // Match backend property name exactly (capital E)
}


export interface ExpenseFilterOptions {
  username?: string;           
  pagination?: PaginationQuery; 
  search?: ExpenseSearchModel;  
}

export interface ExpenseResponseDto {
  id: string;
  title: string;
  category: string;
  notes: string;
  amount: number;
  createdAt: Date;
  updatedAt: Date;
  createdBy: string;
  updatedBy?: string;
  username: string;
  receipt?: ReceiptInfoDto | null;
}

export interface ReceiptInfoDto {
  id: string;
  receiptName: string;
  category: string;
  createdAt: Date;
  fileData?: number[]; // Backend sends as number array
  fileSizeBytes?: number;
  contentType?: string;
}