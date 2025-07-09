import { ReceiptInfo } from "./ReceiptInfo";

export interface Expense {
  id: string;                   
  title: string;                 
  category: string;              
  notes: string;                
  amount: number;                
  createdAt: Date;              
  createdBy: string;           
  updatedBy?: string;           
  updatedAt: Date;             
  username: string;             
  user?: any;   
  receipt?: ReceiptInfo|null;               
}
