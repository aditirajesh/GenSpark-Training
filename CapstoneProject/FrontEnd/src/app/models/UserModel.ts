export interface User {
  username: string;           
  password: string;          
  role: string;              
  phone: string;            
  isDeleted: boolean;       
  createdAt: Date;          
  updatedAt: Date;          
  createdBy: string;         
  updatedBy?: string; 
  refreshToken?: string;
  refreshTokenExpiryTime?: Date;        
  expenses?: any[];         
  receipts?: any[];          
}