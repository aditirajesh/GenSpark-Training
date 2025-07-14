import { Range } from "./Range";

export interface UserSearchModel {
  username?: string;        
  role?: string;            
  createdAtRange?: Range<Date>;
}