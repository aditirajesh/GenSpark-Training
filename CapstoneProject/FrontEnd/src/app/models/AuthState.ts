import { CurrentUser } from "./CurrentUser";

export interface AuthState {
  isAuthenticated: boolean;
  user: CurrentUser | null;
  accessToken: string | null;
  refreshToken: string | null;
  tokenExpiry: Date | null;
}