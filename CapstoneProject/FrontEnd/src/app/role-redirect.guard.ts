import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Observable, map } from 'rxjs';
import { AuthService } from './services/auth.service';


@Injectable({
  providedIn: 'root'
})
export class RoleRedirectGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(): Observable<boolean> {
    return this.authService.currentUser$.pipe(
      map(user => {
        if (user) {
          if (user.role === 'Admin') {
            this.router.navigate(['/admin-home']);
            return false;
          } else {
            return true;
          }
        } else {
          this.router.navigate(['/login']);
          return false;
        }
      })
    );
  }
}