import { inject } from "@angular/core";
import { AuthService } from "./services/auth.service";
import { Router } from "@angular/router";
import { map, take } from "rxjs";

export const authGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.checkAndRefreshToken().pipe(
    take(1),
    map(isAuthenticated => {
      if (isAuthenticated) {
        return true;
      } else {
        console.log('Access denied - redirecting to login');
        router.navigate(['/login']);
        return false;
      }
    })
  );
};