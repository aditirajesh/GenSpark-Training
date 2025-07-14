export interface UserLoginRequestDto {
  username: string;        
  password: string;       
}

export interface UserLoginResponseDto {
  username: string;       
  accessToken?: string;   
  refreshToken?: string;   
}

export interface TokenRefreshRequestDto {
  refreshToken: string;    
  username: string;       
}

export interface AuthResponseDto {
  accessToken: string;     
  refreshToken: string;   
}