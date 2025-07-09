import { Injectable } from "@angular/core";

@Injectable({
  providedIn: 'root'
})
export class TokenStorageService {
  
  private readonly ACCESS_TOKEN_KEY = 'expense_tracker_access_token';
  private readonly REFRESH_TOKEN_KEY = 'expense_tracker_refresh_token';
  private readonly USER_KEY = 'expense_tracker_user';

  //saveToken
  saveTokens(accessToken: string, refreshToken: string, user: any): void {
    try {
      localStorage.setItem(this.ACCESS_TOKEN_KEY, accessToken);
      localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
      localStorage.setItem(this.USER_KEY, JSON.stringify(user));
      
      console.log('Tokens saved successfully');
    } catch (error) {
      console.error('Error saving tokens:', error);
    }
  }

  //getToken
  getAccessToken(): string | null {
    try {
      return localStorage.getItem(this.ACCESS_TOKEN_KEY);
    } catch (error) {
      console.error('Error getting access token:', error);
      return null;
    }
  }

  //getRefreshToken
  getRefreshToken(): string | null {
    try {
      return localStorage.getItem(this.REFRESH_TOKEN_KEY);
    } catch (error) {
      console.error('Error getting refresh token:', error);
      return null;
    }
  }

  //getUser
  getUser(): any {
    try {
      const userJson = localStorage.getItem(this.USER_KEY);
      return userJson ? JSON.parse(userJson) : null;
    } catch (error) {
      console.error('Error getting user data:', error);
      return null;
    }
  }

  //updateToken
  updateAccessToken(accessToken: string): void {
    try {
      localStorage.setItem(this.ACCESS_TOKEN_KEY, accessToken);
    } catch (error) {
      console.error('Error updating access token:', error);
    }
  }

  //clearToken
  clearTokens(): void {
    try {
      localStorage.removeItem(this.ACCESS_TOKEN_KEY);
      localStorage.removeItem(this.REFRESH_TOKEN_KEY);
      localStorage.removeItem(this.USER_KEY);
      console.log('All tokens cleared');
    } catch (error) {
      console.error('Error clearing tokens:', error);
    }
  }

  //check if user has token 
  hasTokens(): boolean {
    return !!(this.getAccessToken() && this.getRefreshToken());
  }
}