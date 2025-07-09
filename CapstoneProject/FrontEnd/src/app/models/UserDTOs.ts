export interface UserAddRequestDto {
  username: string;         
  phone: string;            
  role: string;             
  password: string;         
}

export interface UserUpdateRequestDto {
  phone?: string;           
  role?: string;           
  password?: string;      
}