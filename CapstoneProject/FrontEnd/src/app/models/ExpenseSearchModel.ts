import { Range } from "./Range";
export interface ExpenseSearchModel {
  title?: string;               
  category?: string;            
  amountRange?: Range<number>;  
  dateRange?: Range<Date>;      
}