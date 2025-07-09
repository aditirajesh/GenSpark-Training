import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment.prod";
import { BehaviorSubject, catchError, filter, finalize, map, Observable, of, switchMap, take, tap, throwError } from "rxjs";
import { AuthState } from "../models/AuthState";
import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { Router } from "@angular/router";
import { JwtService } from "./jwt.service";
import { TokenStorageService } from "./tokenstorage.service";
import { AuthResponseDto, TokenRefreshRequestDto, UserLoginRequestDto, UserLoginResponseDto } from "../models/AuthDTOs";
import { CurrentUser } from "../models/CurrentUser";

@Injectable({
  providedIn: 'root'
})
export class AuthService {
    private readonly apiUrl = `${environment.apiBaseUrl}/api/auth`

    private authStateSubject = new BehaviorSubject<AuthState>({
        isAuthenticated: false,
        user: null,
        accessToken: null,
        refreshToken: null,
        tokenExpiry: null
    });

    public authState$ = this.authStateSubject.asObservable();
    public isAuthenticated$ = this.authState$.pipe(
        map(state => state.isAuthenticated)
    );
    public currentUser$ = this.authState$.pipe(
        map(state => state.user)
    );

    private refreshInProgress = false;

    constructor(
        private http: HttpClient,
        private router: Router,
        private jwtService: JwtService,
        private tokenStorage: TokenStorageService
    ) {
        this.initializeAuthState();
    }

    private initializeAuthState(): void {
        console.log('AuthService: Initializing authentication state');
        
        const accessToken = this.tokenStorage.getAccessToken();
        const refreshToken = this.tokenStorage.getRefreshToken();
        
        if (accessToken && refreshToken) {
            console.log('AuthService: Found both tokens, checking validity');
            const user = this.jwtService.extractUserFromToken(accessToken);
            const isExpired = this.jwtService.isTokenExpired(accessToken);

            console.log('User from token:', user);
            console.log('Access token expired:', isExpired);
            
            if (!isExpired && user) {
                console.log('AuthService: Valid tokens found, setting auth state');
                this.updateAuthState(true, user, accessToken, refreshToken);
                
                // ✅ ADDED: Role-based entry navigation on app initialization
                this.handleInitialNavigation(user);
            } else {
                console.log('AuthService: Expired or invalid tokens, clearing state');
                this.tokenStorage.clearTokens();
                this.updateAuthState(false, null, null, null);
            }
        } else {
            console.log('AuthService: No valid tokens found');
        }
    }

    // ✅ ADDED: Handle navigation on app initialization based on current route and user role
    private handleInitialNavigation(user: CurrentUser): void {
        const currentUrl = this.router.url;
        console.log('AuthService: Handling initial navigation for user role:', user.role, 'at URL:', currentUrl);
        
        // Don't redirect if user is already on the correct page or on shared pages
        const sharedRoutes = ['/expenses', '/reports', '/profile'];
        const isOnSharedRoute = sharedRoutes.some(route => currentUrl.startsWith(route));
        
        if (isOnSharedRoute) {
            console.log('AuthService: User on shared route, no redirect needed');
            return;
        }
        
        // Only redirect if user is on the wrong home page or login page
        if (user.role === 'Admin') {
            if (currentUrl === '/home' || currentUrl === '/login' || currentUrl === '/') {
                console.log('AuthService: Redirecting admin to admin-home');
                this.router.navigate(['/admin-home']);
            }
        } else {
            if (currentUrl === '/admin-home' || currentUrl === '/login' || currentUrl === '/') {
                console.log('AuthService: Redirecting user to home');
                this.router.navigate(['/home']);
            }
        }
    }

    login(credentials: UserLoginRequestDto): Observable<UserLoginResponseDto> {
        console.log('AuthService: Attempting login for user:', credentials.username);
        
        return this.http.post<UserLoginResponseDto>(`${this.apiUrl}/login`, credentials)
        .pipe(
            tap((response: UserLoginResponseDto) => {
                console.log('AuthService: Login successful for user:', response.username);
                
                if (response.accessToken && response.refreshToken) {
                    const user = this.jwtService.extractUserFromToken(response.accessToken);
                    this.tokenStorage.saveTokens(response.accessToken, response.refreshToken, user);
                    this.updateAuthState(true, user, response.accessToken, response.refreshToken);
                    
                    // ✅ ADDED: Role-based navigation after successful login
                    this.navigateAfterLogin(user);
                    
                    console.log('AuthService: User session established');
                } else {
                    console.error('AuthService: Login response missing tokens');
                    throw new Error('Invalid login response');
                }
            }),
            catchError(this.handleError.bind(this))
        );
    }

    // ✅ ADDED: Navigate user based on their role after login
    private navigateAfterLogin(user: CurrentUser | null): void {
        if (!user) {
            console.log('AuthService: No user provided for navigation');
            return;
        }
        
        console.log('AuthService: Navigating user based on role:', user.role);
        
        if (user.role === 'Admin') {
            console.log('AuthService: Redirecting admin to admin-home');
            this.router.navigate(['/admin-home']);
        } else {
            console.log('AuthService: Redirecting user to home');
            this.router.navigate(['/home']);
        }
    }

    checkAndRefreshToken(): Observable<boolean> {
        console.log('=== checkAndRefreshToken called ===');
        
        const accessToken = this.getAccessToken();
        const refreshToken = this.tokenStorage.getRefreshToken();
        
        console.log('Access token exists:', !!accessToken);
        console.log('Refresh token exists:', !!refreshToken);
        console.log('Current auth state:', this.authStateSubject.value.isAuthenticated);
        
        if (!accessToken || !refreshToken) {
            console.log('Missing tokens, returning false');
            return of(false);
        }
        
        const isExpired = this.jwtService.isTokenExpired(accessToken);
        console.log('Access token expired:', isExpired);
        
        if (!isExpired) {
            console.log('Token valid, returning true');
            return of(true);
        }
        
        // Token expired, try to refresh
        console.log('Access token expired, attempting refresh...');
        return this.refreshAccessToken().pipe(
            map(() => {
                console.log('Refresh successful, returning true');
                return true;
            }),
            catchError((error) => {
                console.error('Refresh failed:', error);
                console.log('Prompting re-login, returning false');
                this.promptReLogin();
                return of(false);
            })
        );
    }

    private promptReLogin(): void {
        console.log('AuthService: Prompting user to re-login');
        
        this.tokenStorage.clearTokens();
        this.updateAuthState(false, null, null, null);
        
        alert('Your session has expired. Please log in again.');
        
        this.router.navigate(['/login']);
    }

    logout(): void {
        console.log('AuthService: User logging out');
        this.tokenStorage.clearTokens();
        this.updateAuthState(false, null, null, null);
        this.router.navigate(['/login']);
    }

    private refreshAccessToken(): Observable<AuthResponseDto> {
        if (this.refreshInProgress) {
            return throwError(() => new Error('Refresh already in progress'));
        }
        
        const refreshToken = this.tokenStorage.getRefreshToken();
        const currentUser = this.getCurrentUser();
        
        if (!refreshToken || !currentUser) {
            return throwError(() => new Error('No refresh token available'));
        }
        
        this.refreshInProgress = true;
        
        const refreshRequest: TokenRefreshRequestDto = {
            username: currentUser.username,
            refreshToken: refreshToken
        };
        
        return this.http.post<AuthResponseDto>(`${this.apiUrl}/getaccesstoken`, refreshRequest)
        .pipe(
            tap((response: AuthResponseDto) => {
                console.log('AuthService: Token refresh successful');
                const user = this.jwtService.extractUserFromToken(response.accessToken);
                this.tokenStorage.saveTokens(response.accessToken, response.refreshToken, user);
                this.updateAuthState(true, user, response.accessToken, response.refreshToken);
            }),
            catchError((error) => {
                console.error('AuthService: Token refresh failed:', error);
                return throwError(() => error);
            }),
            finalize(() => {
                this.refreshInProgress = false;
            })
        );
    }

    // ✅ ADDED: Get auth token for HTTP interceptor
    getAuthToken(): string | null {
        return this.getAccessToken();
    }

    getAccessToken(): string | null {
        return this.authStateSubject.value.accessToken;
    }

    getCurrentUser(): CurrentUser | null {
        return this.authStateSubject.value.user;
    }

    isAuthenticated(): boolean {
        return this.authStateSubject.value.isAuthenticated;
    }

    isAdmin(): boolean {
        const user = this.getCurrentUser();
        return user?.role === 'Admin';
    }

    // ✅ ADDED: Check if current route matches user role and redirect if necessary
    shouldRedirectBasedOnRole(): boolean {
        const user = this.getCurrentUser();
        const currentUrl = this.router.url;
        
        if (!user) return false;
        
        console.log('AuthService: Checking route redirect for user role:', user.role, 'at URL:', currentUrl);
        
        // Admin trying to access regular home
        if (user.role === 'Admin' && currentUrl === '/home') {
            console.log('AuthService: Admin accessing user home, redirecting to admin-home');
            this.router.navigate(['/admin-home']);
            return true;
        }
        
        // Regular user trying to access admin home
        if (user.role !== 'Admin' && currentUrl === '/admin-home') {
            console.log('AuthService: User accessing admin home, redirecting to home');
            this.router.navigate(['/home']);
            return true;
        }
        
        return false;
    }

    // ✅ ADDED: Get user role display name
    getUserRoleDisplayName(): string {
        const user = this.getCurrentUser();
        if (!user) return 'Guest';
        
        switch (user.role) {
            case 'Admin':
                return 'Administrator';
            case 'User':
                return 'User';
            default:
                return user.role || 'User';
        }
    }

    private updateAuthState(
        isAuthenticated: boolean, 
        user: CurrentUser | null, 
        accessToken: string | null, 
        refreshToken: string | null
    ): void {
        const tokenExpiry = accessToken ? this.jwtService.getExpiration(accessToken) : null;
        
        this.authStateSubject.next({
            isAuthenticated,
            user,
            accessToken,
            refreshToken,
            tokenExpiry
        });
    }

    private handleError(error: HttpErrorResponse): Observable<never> {
        console.error('AuthService: HTTP Error occurred:', error);
        
        let errorMessage = 'An unknown error occurred';
        
        if (error.error instanceof ErrorEvent) {
            // Client-side error
            errorMessage = `Network error: ${error.error.message}`;
        } else {
            // Server-side error
            switch (error.status) {
                case 400:
                    errorMessage = error.error?.message || 'Invalid request data';
                    break;
                case 401:
                    errorMessage = 'Invalid username or password';
                    break;
                case 403:
                    errorMessage = 'Access denied';
                    break;
                case 500:
                    errorMessage = 'Server error. Please try again later';
                    break;
                default:
                    errorMessage = error.error?.message || `Server error: ${error.status}`;
            }
        }
        
        return throwError(() => new Error(errorMessage));
    }
}