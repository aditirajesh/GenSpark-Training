import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { AuthService } from "../services/auth.service";
import { Observable, switchMap, throwError } from "rxjs";

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    constructor(private authService: AuthService) {}

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        if (this.isAuthRequest(req)) {
            return next.handle(req);
        }

        return this.authService.checkAndRefreshToken().pipe(
            switchMap((isAuthenticated: boolean) => {
                if (isAuthenticated) {
                // Add auth header and make request
                const authReq = this.addAuthHeader(req);
                return next.handle(authReq);
                } else {
                // Not authenticated, return 401 error
                return throwError(() => new HttpErrorResponse({
                    status: 401,
                    statusText: 'Unauthorized',
                    error: { message: 'Please log in again' }
                }));
                }
            })
        );
    }

    private addAuthHeader(request: HttpRequest<any>): HttpRequest<any> {
        const accessToken = this.authService.getAccessToken();
        
        if (accessToken) {
        return request.clone({
            setHeaders: {
            'Authorization': `Bearer ${accessToken}`
            }
        });
        }
        
        return request;
    }

    private isAuthRequest(request: HttpRequest<any>): boolean {
        // URLs that should NOT require authorization (public endpoints)
        const publicPaths = [
            '/api/auth/login',
            '/api/auth/getaccesstoken',
            '/api/auth/refresh',
            '/api/users/register'  
        ];
        
        const isPublicPath = publicPaths.some(path => 
            request.url.toLowerCase().includes(path.toLowerCase())
        );
        
        console.log(`URL: ${request.url}, Is Public/Auth Request: ${isPublicPath}`);
        return isPublicPath;
    }
}
