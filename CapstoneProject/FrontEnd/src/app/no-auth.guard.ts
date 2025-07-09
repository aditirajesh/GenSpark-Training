import { map, take } from "rxjs";
import { AuthService } from "./services/auth.service";
import { inject } from "@angular/core";
import { Router } from "@angular/router";

export const noAuthGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isAuthenticated$.pipe(
    take(1),
    map(isAuthenticated => {
      if (isAuthenticated) {
        console.log('Already authenticated - redirecting to home');
        router.navigate(['/home']);
        return false;
      } else {
        return true;
      }
    })
  );
};